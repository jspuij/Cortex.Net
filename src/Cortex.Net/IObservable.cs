// <copyright file="IObservable.cs" company="Michel Weststrate, Jan-Willem Spuij">
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

namespace Cortex.Net
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Represents something that is Observable.
    /// </summary>
    public interface IObservable : IDependencyNode
    {
        /// <summary>
        /// Event that will fire after the observable has become observed.
        /// </summary>
        event EventHandler BecomeObserved;

        /// <summary>
        /// Gets the Observers.
        /// </summary>
        ISet<IDerivation> Observers { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this IObservable is pending Unobservation.
        /// </summary>
        bool IsPendingUnobservation { get; set; }

        /// <summary>
        /// Gets or sets the Id of the derivation run that last accessed this observable.
        /// If this Id equals the <see cref="IDerivation.RunId"/> of the current derivation
        /// the dependency is already established.
        /// </summary>
        int LastAccessedBy { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the observable is being observed.
        /// An observable is being observed when at least one derivation actually accesses its
        /// value.
        /// </summary>
        bool IsBeingObserved { get; set; }

        /// <summary>
        /// Gets or sets the lowest <see cref="DerivationState"/> on any of it's observers.
        /// </summary>
        DerivationState LowestObserverState { get; set; }

        /// <summary>
        /// Method that at least must be implented to trigger event <see cref="BecomeObserved"/>.
        /// </summary>
        void OnBecomeObserved();
    }
}
