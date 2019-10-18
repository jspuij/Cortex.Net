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

            if (derivation.DependenciesState == IDerivationState.NotTracking)
            {
                throw new ArgumentOutOfRangeException(
                    paramName: nameof(derivation),
                    message: string.Format(CultureInfo.CurrentCulture, Resources.CanOnlyAddTrackedDependencies, IDerivationState.NotTracking));
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
                QueueForUnobservation(observable);
            }
        }

        /// <summary>
        /// Queues an observable for global unobservation.
        /// </summary>
        /// <param name="observable">The observable to queue.</param>
        private static void QueueForUnobservation(IObservable observable)
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
    }
}
