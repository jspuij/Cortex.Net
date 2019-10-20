// <copyright file="ObservableExtensions.cs" company="Michel Weststrate, Jan-Willem Spuij">
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
    using Cortex.Net.Properties;

    /// <summary>
    /// Extension methods for the <see cref="IObservable"/> interface.
    /// </summary>
    public static class ObservableExtensions
    {
        /// <summary>
        /// Indicates whether the observable has Observers or not.
        /// </summary>
        /// <param name="observable">The observable to check for observers.</param>
        /// <returns>A boolean indicating whether the observable has observers or not.</returns>
        /// <exception cref="ArgumentNullException">When any of the arguments is null.</exception>
        public static bool HasObservers(this IObservable observable)
        {
            if (observable is null)
            {
                throw new ArgumentNullException(nameof(observable));
            }

            return observable.Observers.Count > 0;
        }

        /// <summary>
        /// Adds an observer that implements <see cref="IDerivation"/> into the set of observers.
        /// for this <see cref="IObservable"/> instance.
        /// </summary>
        /// <param name="observable">The observable to use.</param>
        /// <param name="derivation">The observer to add.</param>
        /// <exception cref="ArgumentNullException">When any of the arguments is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">When the derivation is in the not tracking state.</exception>
        /// <exception cref="InvalidOperationException">When the derivation was already added.</exception>
        public static void AddObserver(this IObservable observable, IDerivation derivation)
        {
            if (observable is null)
            {
                throw new ArgumentNullException(nameof(observable));
            }

            if (derivation is null)
            {
                throw new ArgumentNullException(nameof(derivation));
            }

            if (derivation.DependenciesState == DerivationState.NotTracking)
            {
                throw new ArgumentOutOfRangeException(
                    paramName: nameof(derivation),
                    message: string.Format(CultureInfo.CurrentCulture, Resources.CanOnlyAddTrackedDependencies, DerivationState.NotTracking));
            }

            if (observable.Observers.Contains(derivation))
            {
                throw new InvalidOperationException(
                    string.Format(CultureInfo.CurrentCulture, Resources.AlreadyAddedObserverToObservable, derivation.Name, observable.Name));
            }

            observable.Observers.Add(derivation);
            if (observable.LowestObserverState > derivation.DependenciesState)
            {
                observable.LowestObserverState = derivation.DependenciesState;
            }
        }

        /// <summary>
        /// Removes an Observer <see cref="IDerivation"/> from the set of observers.
        /// </summary>
        /// <param name="observable">The observable to use.</param>
        /// <param name="derivation">The observer to add.</param>
        /// <exception cref="ArgumentNullException">When any of the arguments is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when shared state is not in batch mode.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the derivation is not in the set of observers.</exception>
        public static void RemoveObserver(this IObservable observable, IDerivation derivation)
        {
            if (observable is null)
            {
                throw new ArgumentNullException(nameof(observable));
            }

            if (derivation is null)
            {
                throw new ArgumentNullException(nameof(derivation));
            }

            if (!observable.SharedState.InBatch)
            {
                throw new InvalidOperationException(Resources.RemoveOnlyInBatch);
            }

            if (!observable.Observers.Contains(derivation))
            {
                throw new InvalidOperationException(
                    string.Format(CultureInfo.CurrentCulture, Resources.ObserverNotInObservable, derivation.Name, observable.Name));
            }

            observable.Observers.Remove(derivation);
            if (!observable.HasObservers())
            {
                // deleted last observer.
                observable.QueueForUnobservation();
            }
        }

        /// <summary>
        /// Report an observable as being observed to the current tracking
        /// derivation (observer).
        /// </summary>
        /// <param name="observable">The observable.</param>
        /// <returns>True when this observable is added to the "new observing" set of the derivation.</returns>
        /// <exception cref="ArgumentNullException">When any of the arguments is null.</exception>
        /// <remarks>This method will write to the debug log when state reads are currently not allowed.</remarks>
        public static bool ReportObserved(this IObservable observable)
        {
            if (observable is null)
            {
                throw new ArgumentNullException(nameof(observable));
            }

            observable.CheckIfStateReadsAreAllowed();

            var derivation = observable.SharedState.TrackingDerivation;

            if (derivation != null)
            {
                /*
                 * Many derivations reference the same observable multiple times.
                 * E.g. z = (x * y) + x references x twice.
                 * Simple optimization, give each derivation run an unique id (runId)
                 * Check if last time this observable was accessed the same runId is used
                 * if this is the case, the relation is already known.
                 */
                if (derivation.RunId != observable.LastAccessedBy)
                {
                    observable.LastAccessedBy = derivation.RunId;
                    derivation.NewObserving.Add(observable);

                    if (!observable.IsBeingObserved)
                    {
                        observable.IsBeingObserved = true;
                        observable.OnBecomeObserved();
                    }
                }

                return true;
            }
            else if (!observable.HasObservers() && observable.SharedState.InBatch)
            {
                observable.QueueForUnobservation();
            }

            return false;
        }

        /// <summary>
        /// Propagates a change to all observers of this observable.
        /// </summary>
        /// <param name="observable">The observable.</param>
        /// <exception cref="ArgumentNullException">When any of the arguments is null.</exception>
        /// <remarks>Called by Atom when its value has changed.</remarks>
        public static void PropagateChanged(this IObservable observable)
        {
            if (observable is null)
            {
                throw new ArgumentNullException(nameof(observable));
            }

            if (observable.LowestObserverState == DerivationState.Stale)
            {
                return;
            }

            observable.LowestObserverState = DerivationState.Stale;

            foreach (var derivation in observable.Observers)
            {
                if (derivation.DependenciesState == DerivationState.UpToDate)
                {
                    if (derivation.IsTracing != TraceMode.None)
                    {
                        // TODO: implement logging of trace info.
                        // logTraceInfo(derivation, observable);
                    }

                    derivation.OnBecomeStale();
                }

                derivation.DependenciesState = DerivationState.Stale;
            }
        }

        /// <summary>
        /// Propagates confirmation of a change to all observers of this observable.
        /// </summary>
        /// <param name="observable">The observable.</param>
        /// <exception cref="ArgumentNullException">When any of the arguments is null.</exception>
        /// <remarks>Called by ComputedValue when it recalculate and its value changed.</remarks>
        public static void PropagateChangeConfirmed(this IObservable observable)
        {
            if (observable is null)
            {
                throw new ArgumentNullException(nameof(observable));
            }

            if (observable.LowestObserverState == DerivationState.Stale)
            {
                return;
            }

            observable.LowestObserverState = DerivationState.Stale;

            foreach (var derivation in observable.Observers)
            {
                if (derivation.DependenciesState == DerivationState.PossiblyStale)
                {
                    derivation.DependenciesState = DerivationState.Stale;
                }
                else if (derivation.DependenciesState == DerivationState.UpToDate)
                {
                    // TODO: JWS: this seems incorrect when multiple derivations reference this observable. Devise test!
                    // this happens during computing of `derivation`, just keep lowestObserverState up to date.
                    observable.LowestObserverState = DerivationState.UpToDate;
                }
            }
        }

        /// <summary>
        /// Propagates confirmation of a possible change to all observers of
        /// this observable for delayed computation.
        /// </summary>
        /// <param name="observable">The observable.</param>
        /// <exception cref="ArgumentNullException">When any of the arguments is null.</exception>
        /// <remarks>
        /// Called by ComputedValue when its dependency changed,
        /// but we don't wan't to immediately recompute.
        /// </remarks>
        public static void PropagateMaybeChanged(this IObservable observable)
        {
            if (observable is null)
            {
                throw new ArgumentNullException(nameof(observable));
            }

            if (observable.LowestObserverState != DerivationState.UpToDate)
            {
                return;
            }

            observable.LowestObserverState = DerivationState.Stale;

            foreach (var derivation in observable.Observers)
            {
                if (derivation.DependenciesState == DerivationState.UpToDate)
                {
                    derivation.DependenciesState = DerivationState.PossiblyStale;
                    if (derivation.IsTracing != TraceMode.None)
                    {
                        // TODO: implement logging of trace info.
                        // logTraceInfo(derivation, observable);
                    }

                    derivation.OnBecomeStale();
                }
            }
        }

        /// <summary>
        /// Checks whether the observable is a derivation as well.
        /// </summary>
        /// <param name="observable">The observable to check.</param>
        /// <returns>True if the observable is a derivation, false otherwise.</returns>
        public static bool IsDerivation(this IObservable observable)
        {
            return observable is IDerivation;
        }

        /// <summary>
        /// Queues an observable for global unobservation.
        /// </summary>
        /// <param name="observable">The observable to queue.</param>
        /// <exception cref="InvalidOperationException">Thrown when the observable still has observers.</exception>
        private static void QueueForUnobservation(this IObservable observable)
        {
            if (observable.IsPendingUnobservation == false)
            {
                if (observable.HasObservers())
                {
                    throw new InvalidOperationException(Resources.GlobalUnobservationOnlyWithoutObservers);
                }

                observable.IsPendingUnobservation = true;
                observable.SharedState.PendingUnobservations.Enqueue(observable);
            }
        }

        /// <summary>
        /// Checks if State reads are allowed and writes a warning to the Trace log.
        /// </summary>
        /// <param name="observable">The observable to report.</param>
        private static void CheckIfStateReadsAreAllowed(this IObservable observable)
        {
            var sharedState = observable.SharedState;
            var configuration = sharedState.Configuration;

            if (!sharedState.AllowStateReads && configuration.ObservableRequiresReaction)
            {
                Trace.WriteLine($"[Cortex.Net] Observable {observable.Name} being read outside a reactive context.");
            }
        }
    }
}
