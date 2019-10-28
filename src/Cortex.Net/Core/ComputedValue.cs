// <copyright file="ComputedValue.cs" company="Michel Weststrate, Jan-Willem Spuij">
// Copyright 2019 Michel Weststrate, Jan-Willem Spuij
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy,
// modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom
// the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
// ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>

namespace Cortex.Net.Core
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Text;
    using Cortex.Net.Properties;
    using Cortex.Net.Spy;

    /// <summary>
    /// A node in the state dependency root that observes other nodes, and can be observed itself.
    /// </summary>
    /// <typeparam name="T">The type of the computed value.</typeparam>
    public class ComputedValue<T> : IObservable, IDerivation, IComputedValue<T>
    {
        /// <summary>
        /// The derivation function to execute to get the value.
        /// </summary>
        private readonly Func<T> derivation;

        /// <summary>
        /// The optional setter function which can serve as the inverse function of the computed value.
        /// </summary>
        private readonly Action<T> setter;

        /// <summary>
        /// The subject of the getter / setter.
        /// </summary>
        private readonly object scope;

        /// <summary>
        /// The equality comparer that is used.
        /// </summary>
        private readonly IEqualityComparer<T> equalityComparer;

        /// <summary>
        /// A value indicating whether the computed value keeps calculating, even when it is not observed.
        /// </summary>
        private readonly bool keepAlive;

        /// <summary>
        /// Indicates whether this computedValue requires a reactive context.
        /// </summary>
        private readonly bool requiresReaction;

        /// <summary>
        /// To check for evaluation cycles.
        /// </summary>
        private bool isComputing = false;

        /// <summary>
        /// To check for setter cycles.
        /// </summary>
        private bool isRunningSetter = false;

        /// <summary>
        /// The computed value.
        /// </summary>
        private T value;

        /// <summary>
        /// The last exception after accessing Value.
        /// </summary>
        private Exception lastException;

        /// <summary>
        /// Initializes a new instance of the <see cref="ComputedValue{T}"/> class.
        /// </summary>
        /// <param name="sharedState">The shared state this computedValue is connected to.</param>
        /// <param name="options">An <see cref="ComputedValueOptions{T}"/> instance that define the options for this computed value.</param>
        public ComputedValue(ISharedState sharedState, ComputedValueOptions<T> options)
        {
            if (sharedState is null)
            {
                throw new ArgumentNullException(nameof(sharedState));
            }

            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            this.Name = options.Name;
            this.derivation = options.Getter;
            this.SharedState = sharedState;

            if (options.Setter != null)
            {
                this.setter = sharedState.CreateAction($"{options.Name}-setter", options.Context, options.Setter);
            }

            this.scope = options.Context;
            this.equalityComparer = options.EqualityComparer;
            this.keepAlive = options.KeepAlive;
            this.requiresReaction = options.RequiresReaction;
        }

        /// <summary>
        /// Event that will fire after the <see cref="Atom"/> has become observed.
        /// </summary>
        public event EventHandler BecomeObserved;

        /// <summary>
        /// Event that will fire after the observable has become unobserved.
        /// </summary>
        public event EventHandler BecomeUnobserved;

        /// <summary>
        /// Gets the Observers.
        /// </summary>
        public ISet<IDerivation> Observers { get; } = new HashSet<IDerivation>();

        /// <summary>
        /// Gets or sets a value indicating whether this atom is pending Unobservation.
        /// </summary>
        public bool IsPendingUnobservation { get; set; } = false;

        /// <summary>
        /// Gets or sets the Id of the derivation run that last accessed this observable.
        /// If this Id equals the <see cref="IDerivation.RunId"/> of the current derivation
        /// the dependency is already established.
        /// </summary>
        public int LastAccessedBy { get; set; } = 0;

        /// <summary>
        /// Gets or sets a value indicating whether the observable is being observed.
        /// An observable is being observed when at least one derivation actually accesses its
        /// value.
        /// </summary>
        public bool IsBeingObserved { get; set; } = false;

        /// <summary>
        /// Gets or sets the lowest <see cref="DerivationState"/> on any of it's observers.
        /// </summary>
        public DerivationState LowestObserverState { get; set; } = DerivationState.NotTracking;

        /// <summary>
        /// Gets the Name of the <see cref="Atom"/> instance.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the Shared State of all the nodes in the dependency graph.
        /// </summary>
        public ISharedState SharedState { get; }

        /// <summary>
        /// Gets or sets the state of the dependencies of this <see cref="IDerivation"/> instance.
        /// </summary>
        public DerivationState DependenciesState { get; set; } = DerivationState.NotTracking;

        /// <summary>
        /// Gets or sets the id of the current run of a derivation. Each time the derivation is tracked
        /// this number is increased by one. This number is unique within the current shared state.
        /// </summary>
        public int RunId { get; set; }

        /// <summary>
        /// Gets a set of <see cref="IObservable"/> instances that are currently observed.
        /// </summary>
        public ISet<IObservable> Observing { get; } = new HashSet<IObservable>();

        /// <summary>
        /// Gets a set of <see cref="IObservable"/> instances that have been hit during a new derivation run.
        /// </summary>
        public ISet<IObservable> NewObserving { get; } = new HashSet<IObservable>();

        /// <summary>
        /// Gets or sets the trace mode of this Derivation.
        /// </summary>
        public TraceMode IsTracing { get; set; } = TraceMode.None;

        /// <summary>
        /// Gets a value indicating whether to warn if this derivation is required to visit at least one observable.
        /// </summary>
        public bool RequiresObservable { get; }

        /// <summary>
        /// Gets or sets the underlying value.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when a cycle in computation is detected or when an inner exception is thrown by one of the referenced observables.</exception>
        public T Value
        {
            get
            {
                if (this.isComputing)
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.CycleDetectedInComputation, this.Name, this.derivation));
                }

                Exception caughtException = null;

                if (this.SharedState.InBatch && this.HasObservers() && !this.keepAlive)
                {
                    if (this.ShouldCompute())
                    {
                        this.WarnAboutUntrackedRead();
                        this.SharedState.StartBatch();
                        (this.value, caughtException) = this.ComputeValue(false);
                        this.SharedState.EndBatch();
                    }
                }
                else
                {
                    this.ReportObserved();
                    if (this.ShouldCompute())
                    {
                        if (this.TrackAndCompute())
                        {
                            this.PropagateChangeConfirmed();
                        }
                    }
                }

                if (caughtException != null)
                {
                    throw new InvalidOperationException(Resources.CaughtExceptionDuringGet, caughtException);
                }

                return this.value;
            }

            set
            {
                if (this.setter != null)
                {
                    if (this.isRunningSetter)
                    {
                        throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.CycleDetectedInSetter, this.Name));
                    }

                    this.isRunningSetter = true;
                    try
                    {
                        this.setter(value);
                    }
                    finally
                    {
                        this.isRunningSetter = false;
                    }
                }
                else
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.CannotAssignComputedValue, this.Name));
                }
            }
        }

        /// <summary>
        /// Gets or sets the value.
        /// Explicit implementation of <see cref="IComputedValue.Value"/>.
        /// </summary>
        object IComputedValue.Value
        {
            get => (object)this.Value;
            set => this.Value = (T)value;
        }

        /// <summary>
        /// Method that triggers event <see cref="BecomeObserved"/>.
        /// </summary>
        public void OnBecomeObserved()
        {
            this.BecomeObserved?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Method that triggers event <see cref="BecomeUnobserved"/>.
        /// </summary>
        public void OnBecomeUnobserved()
        {
            this.BecomeUnobserved?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Propagates confirmation of a possible change to all observers of
        /// this <see cref="ComputedValue{T}"/> for delayed computation.
        /// </summary>
        public void OnBecomeStale()
        {
            this.PropagateMaybeChanged();
        }

        /// <summary>
        /// Suspends computation of this computed value when the last observer leaves.
        /// Computed values are automatically teared down when the last observer leaves.
        /// This process happens recursively, this computed might be the last observabe of another, etc.
        /// </summary>
        public void Suspend()
        {
            this.ClearObserving();
            this.value = default;
            this.lastException = null;
        }

        /// <summary>
        /// Track computed value by calling the getter.
        /// </summary>
        /// <returns>Whether the value has changed.</returns>
        private bool TrackAndCompute()
        {
            this.SharedState.OnSpy(this, new ComputedSpyEventArgs()
            {
                Context = this.scope,
                Name = this.Name,
            });

            T oldValue = this.value;

            var wasSuspended = this.DependenciesState == DerivationState.NotTracking;

            (T newValue, Exception caughtException) = this.ComputeValue(true);

            var changed = wasSuspended ||
                this.lastException != null ||
                caughtException != null ||
                (this.equalityComparer != null ? !this.equalityComparer.Equals(oldValue, newValue) : !object.Equals(oldValue, newValue));

            if (changed)
            {
                this.value = newValue;
                this.lastException = caughtException;
            }

            return changed;
        }

        /// <summary>
        /// Computes a value.
        /// </summary>
        /// <param name="track">Track this derived function.</param>
        /// <returns>The value or an exception.</returns>
        private (T, Exception) ComputeValue(bool track)
        {
            this.isComputing = true;
            this.SharedState.ComputationDepth++;

            T result;
            Exception caughtException = null;

            if (track)
            {
                (result, caughtException) = this.TrackDerivedFunction(this.derivation);
            }
            else
            {
                if (this.SharedState.Configuration.DisableErrorBoundaries)
                {
                    result = this.derivation();
                }
                else
                {
                    try
                    {
                        result = this.derivation();
                    }
#pragma warning disable CA1031 // Do not catch general exception types
                    catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
                    {
                        result = default;
                        caughtException = e;
                    }
                }
            }

            this.SharedState.ComputationDepth--;
            this.isComputing = false;
            return (result, caughtException);
        }

        /// <summary>
        /// Warn about an untracked read of this computed value.
        /// </summary>
        private void WarnAboutUntrackedRead()
        {
            if (this.requiresReaction)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.ReadOutsideReaction, this.Name));
            }

            if (this.SharedState.Configuration.ComputedRequiresReaction || this.IsTracing != TraceMode.None)
            {
                Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, Resources.ReadOutsideReaction, this.Name));
            }

            if (this.IsTracing == TraceMode.Break)
            {
                Debugger.Break();
            }
        }
    }
}
