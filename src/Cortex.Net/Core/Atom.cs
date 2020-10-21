// <copyright file="Atom.cs" company="Michel Weststrate, Jan-Willem Spuij">
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
    using System.Text;

    /// <summary>
    /// Implements an Atom.
    /// Atoms can be used to signal Cortex.Net that some observable data source has been observed or changed.
    /// And Cortex.Net will signal the atom whenever it is used or no longer in use.
    /// </summary>
    public class Atom : IAtom
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Atom"/> class.
        /// </summary>
        /// <param name="sharedState">The shared state where this atom is created on.</param>
        /// <param name="name">The name for this Atom.</param>
        internal Atom(ISharedState sharedState, string name)
        {
            // resolve state state from context if necessary.
            this.SharedState = Net.SharedState.ResolveState(sharedState);

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            this.Name = name;
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
        public bool IsPendingUnobservation { get; set; }

        /// <summary>
        /// Gets or sets the Id of the derivation run that last accessed this observable.
        /// If this Id equals the <see cref="IDerivation.RunId"/> of the current derivation
        /// the dependency is already established.
        /// </summary>
        public int LastAccessedBy { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the observable is being observed.
        /// An observable is being observed when at least one derivation actually accesses its
        /// value.
        /// </summary>
        public bool IsBeingObserved { get; set; }

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
        /// Invoke this method after this atom has changed to signal Cortex.Net that all its observers should invalidate.
        /// </summary>
        public void ReportChanged()
        {
            this.SharedState.StartBatch();
            this.PropagateChanged();
            this.SharedState.EndBatch();
        }

        /// <summary>
        /// Invoke this method to notify Cortex.Net that your atom has been used somehow.
        /// </summary>
        /// <returns>Returns true if there is currently a reactive context.</returns>
        public bool ReportObserved()
        {
            return ObservableExtensions.ReportObserved(this);
        }

        /// <summary>
        /// Returns the name of this <see cref="Atom"/>.
        /// </summary>
        /// <returns>The name of the Atom.</returns>
        public override string ToString()
        {
            return this.Name;
        }
    }
}
