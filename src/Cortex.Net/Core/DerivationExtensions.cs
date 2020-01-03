// <copyright file="DerivationExtensions.cs" company="Michel Weststrate, Jan-Willem Spuij">
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
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using Cortex.Net.Properties;

    /// <summary>
    /// Extension methods for <see cref="IDerivation"/> interface implementations.
    /// </summary>
    public static class DerivationExtensions
    {
        /// <summary>
        /// Checks whether the <see cref="IDerivation"/>instance should recompute itself.
        /// </summary>
        /// <param name="derivation">The derivation.</param>
        /// <returns>True when it needs to recompute, false otherwise.</returns>
        /// <exception cref="ArgumentNullException">When any of the arguments is null.</exception>
        /// <remarks>Might throw any other exception that a getter for <see cref="IObservable"/> will thow.</remarks>
        public static bool ShouldCompute(this IDerivation derivation)
        {
            if (derivation is null)
            {
                throw new ArgumentNullException(nameof(derivation));
            }

            switch (derivation.DependenciesState)
            {
                case DerivationState.UpToDate:
                    return false;
                case DerivationState.NotTracking:
                case DerivationState.Stale:
                    return true;
                case DerivationState.PossiblyStale:
                    var previousAllowStateReads = derivation.SharedState.StartAllowStateReads(true);

                        // no need for those computeds to be reported, they will be picked up in trackDerivedFunction.
                    var previousDerivation = derivation.SharedState.StartUntracked();

                    foreach (var observable in derivation.Observing)
                    {
                        if (observable.IsComputedValue())
                        {
                            if (observable.SharedState.Configuration.DisableErrorBoundaries)
                            {
                                var value = (observable as IComputedValue).Value;
                            }
                            else
                            {
                                try
                                {
                                    var value = (observable as IComputedValue).Value;
                                }
#pragma warning disable CA1031 // Do not catch general exception types
                                catch
#pragma warning restore CA1031 // Do not catch general exception types
                                {
                                    // we are not interested in the value *or* exception at this moment, but if there is one, notify all
                                    derivation.SharedState.EndTracking(previousDerivation);
                                    derivation.SharedState.EndAllowStateReads(previousAllowStateReads);
                                    return true;
                                }
                            }

                            if (derivation.DependenciesState == DerivationState.Stale)
                            {
                                derivation.SharedState.EndTracking(previousDerivation);
                                derivation.SharedState.EndAllowStateReads(previousAllowStateReads);
                                return true;
                            }
                        }
                    }

                    derivation.ChangeLowestObserverStateOnObservablesToUpToDate();
                    derivation.SharedState.EndTracking(previousDerivation);
                    derivation.SharedState.EndAllowStateReads(previousAllowStateReads);
                    return false;
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Executes the provided function and tracks which observables are being accessed.
        /// The tracking information is stored on the <see cref="IDerivation"/> instance and the derivation is registered
        /// as observer of any of the accessed observables.
        /// </summary>
        /// <typeparam name="T">The return type of the function.</typeparam>
        /// <param name="derivation">The derivation to use.</param>
        /// <param name="function">The function to execute.</param>
        /// <returns>A tuple containing the return value of the function or an Exception.</returns>
        public static (T, Exception) TrackDerivedFunction<T>(this IDerivation derivation, Func<T> function)
        {
            if (derivation is null)
            {
                throw new ArgumentNullException(nameof(derivation));
            }

            if (function is null)
            {
                throw new ArgumentNullException(nameof(function));
            }

            T result;
            Exception exception = null;

            var previousAllowStateReads = derivation.SharedState.StartAllowStateReads(true);
            ChangeLowestObserverStateOnObservablesToUpToDate(derivation);
            derivation.NewObserving.Clear();
            derivation.RunId = derivation.SharedState.IncrementRunId();
            var previousDerivation = derivation.SharedState.StartTracking(derivation);

            if (derivation.SharedState.Configuration.DisableErrorBoundaries)
            {
                result = function();
            }
            else
            {
                try
                {
                    result = function();
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    result = default;
                    exception = e;
                }
            }

            derivation.SharedState.EndTracking(previousDerivation);
            derivation.BindDependencies();
            derivation.WarnWithoutDependencies();
            derivation.SharedState.EndAllowStateReads(previousAllowStateReads);

            return (result, exception);
        }

        /// <summary>
        /// Cleans the Observing collection with notification of the observables.
        /// </summary>
        /// <param name="derivation">The derivation to clean.</param>
        public static void ClearObserving(this IDerivation derivation)
        {
            if (derivation == null)
            {
                throw new ArgumentNullException(nameof(derivation));
            }

            var toCallRemoveOn = derivation.Observing.ToList();
            derivation.Observing.Clear();
            foreach (var observable in toCallRemoveOn)
            {
                observable.RemoveObserver(derivation);
            }

            derivation.DependenciesState = DerivationState.NotTracking;
        }

        /// <summary>
        /// Binds the new tracked Dependencies on the <see cref="IDerivation"/> instance.
        /// </summary>
        /// <param name="derivation">The derivation to use.</param>
        /// <exception cref="InvalidOperationException">When the state of the derivation's dependencies is Not tracking.</exception>
        /// <exception cref="NullReferenceException">When the newObserving set is null.</exception>
        private static void BindDependencies(this IDerivation derivation)
        {
            if (derivation.DependenciesState == DerivationState.NotTracking)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.BindDependenciesExpectsStateNonEqual, derivation.DependenciesState));
            }

            if (derivation.NewObserving == null)
            {
                throw new NullReferenceException(string.Format(CultureInfo.CurrentCulture, Resources.IsNull, nameof(derivation.NewObserving)));
            }

            var lowestNewObservingDerivationState = DerivationState.UpToDate;

            foreach (var dependency in derivation.NewObserving)
            {
                if (dependency is IDerivation)
                {
                    lowestNewObservingDerivationState = (DerivationState)Math.Max((int)lowestNewObservingDerivationState, (int)(dependency as IDerivation).DependenciesState);
                }
            }

            foreach (var dependency in derivation.Observing)
            {
                if (!derivation.NewObserving.Contains(dependency))
                {
                    derivation.Observing.Remove(dependency);
                    dependency.RemoveObserver(derivation);
                }
            }

            foreach (var dependency in derivation.NewObserving.ToList())
            {
                if (!derivation.Observing.Contains(dependency))
                {
                    derivation.Observing.Add(dependency);
                    dependency.AddObserver(derivation);
                }
            }

            if (lowestNewObservingDerivationState != DerivationState.UpToDate)
            {
                derivation.DependenciesState = lowestNewObservingDerivationState;
                derivation.OnBecomeStale();
            }
        }

        /// <summary>
        /// Warn about dependencies without derivations.
        /// </summary>
        /// <param name="derivation">The derivation to check.</param>
        private static void WarnWithoutDependencies(this IDerivation derivation)
        {
            if (derivation.Observing.Any())
            {
                return;
            }

            if (derivation.SharedState.Configuration.ReactionRequiresObservable || derivation.RequiresObservable)
            {
                Trace.WriteLine($"[Cortex.Net] {derivation.Name} is created / updated without reading any observable value.");
            }
        }

        /// <summary>
        /// Changes the <see cref="IObservable.LowestObserverState"/> ona ll <see cref="IDerivation.Observing"/> to <see cref="DerivationState.UpToDate"/>
        /// when this derivation changes to <see cref="DerivationState.UpToDate"/>.
        /// </summary>
        /// <param name="derivation">The derivation to use.</param>
        private static void ChangeLowestObserverStateOnObservablesToUpToDate(this IDerivation derivation)
        {
            if (derivation.DependenciesState == DerivationState.UpToDate)
            {
                return;
            }

            derivation.DependenciesState = DerivationState.UpToDate;
            foreach (var observable in derivation.Observing.Reverse())
            {
                observable.LowestObserverState = DerivationState.UpToDate;
            }
        }
    }
}
