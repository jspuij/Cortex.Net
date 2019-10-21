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

    /// <summary>
    /// A node in the state dependency root that observes other nodes, and can be observed itself.
    /// </summary>
    public class ComputedValue : IObservable, IDerivation
    {
        /// <summary>
        /// Indicates whether this computedValue requires a reactive context.
        /// </summary>
        private readonly bool requiresReaction;

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
        public DerivationState DependenciesState { get; set; }

        /// <summary>
        /// Gets or sets the id of the current run of a derivation. Each time the derivation is tracked
        /// this number is increased by one. This number is unique within the current shared state.
        /// </summary>
        public int RunId { get; set; }

        /// <summary>
        /// Gets a set of <see cref="IObservable"/> instances that are currently observed.
        /// </summary>
        public ISet<IObservable> Observing { get; }

        /// <summary>
        /// Gets a set of <see cref="IObservable"/> instances that have been hit during a new derivation run.
        /// </summary>
        public ISet<IObservable> NewObserving { get; }

        /// <summary>
        /// Gets or sets the trace mode of this Derivation.
        /// </summary>
        public TraceMode IsTracing { get; set; }

        /// <summary>
        /// Gets a value indicating whether to warn if this derivation is required to visit at least one observable.
        /// </summary>
        public bool RequiresObservable { get; }

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
        /// this <see cref="ComputedValue"/> for delayed computation.
        /// </summary>
        public void OnBecomeStale()
        {
            this.PropagateMaybeChanged();
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
