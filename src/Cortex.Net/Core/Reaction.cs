// <copyright file="Reaction.cs" company="Michel Weststrate, Jan-Willem Spuij">
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
    /// A reaction is an <see cref="IDerivation"/> implementation that always run and does not have observers itself.
    /// </summary>
    /// <remarks>
    /// Reactions are a special kind of derivations. Several things distinguishes them from normal reactive computations:
    ///
    /// 1) They will always run, not like derivations that only run when unobserved. This means that they are very suitable
    ///     for triggering side effects like logging, updating the DOM and making network requests.
    /// 2) They are not observable themselves
    /// 3) They will always run after any 'normal' derivations
    /// 4) They are allowed to change the state and thereby triggering themselves again, as long as they make sure the state
    ///    propagates to a stable state in a reasonable amount of iterations.
    ///
    /// The state machine of a Reaction is as follows:
    ///
    /// 1) after creating, the reaction should be started by calling `runReaction` or by scheduling it(see also `autorun`)
    /// 2) the `onInvalidate` handler should somehow result in a call to `this.track(someFunction)`
    /// 3) all observables accessed in `someFunction` will be observed by this reaction.
    /// 4) as soon as some of the dependencies has changed the Reaction will be rescheduled for another run (after the
    ///    current mutation or transaction). `isScheduled` will yield true once a dependency is stale and during this period.
    /// 5) `onInvalidate` will be called, and we are back at step 1.
    /// </remarks>
    public sealed class Reaction : IDerivation, IDisposable
    {
        /// <summary>
        /// Indicates whether this reaction is scheduled.
        /// </summary>
        private bool isScheduled;

        /// <summary>
        /// Indicates whether this reaction is disposed.
        /// </summary>
        private bool isDisposed;
        private bool isTrackPending;

        /// <summary>
        /// Initializes a new instance of the <see cref="Reaction"/> class.
        /// </summary>
        /// <param name="sharedState">The shared state to use.</param>
        /// <param name="name">The name to use.</param>
        public Reaction(ISharedState sharedState, string name)
        {
            this.SharedState = sharedState ?? throw new ArgumentNullException(nameof(sharedState));
            this.Name = !string.IsNullOrEmpty(name) ? name : $"{this.SharedState.GetUniqueId()}";
        }

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
        /// Disposes the reaction by removing it from the list of reactions.
        /// </summary>
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Propagates confirmation of a possible change to all observers of
        /// this <see cref="Reaction"/> for delayed computation.
        /// </summary>
        public void OnBecomeStale()
        {
            this.Schedule();
        }

        private void Schedule()
        {
            if (!this.isScheduled)
            {
                this.isScheduled = true;
                this.SharedState.PendingReactions.Enqueue(this);
            }
        }

        /// <summary>
        /// Runs this single reaction.
        /// </summary>
        internal void RunReaction()
        {
            if (this.isDisposed)
            {
                return;
            }

            this.SharedState.StartBatch();
            this.isScheduled = false;

            if (this.ShouldCompute(() => { }))
            {
                this.isTrackPending = true;

                try
                {
                    this.OnInvalidate()
                    if (
                        this._isTrackPending &&
                        isSpyEnabled() &&
                        process.env.NODE_ENV !== "production"
                    )
                    {
                        // onInvalidate didn't trigger track right away..
                        spyReport({
                        name: this.name,
                            type: "scheduled-reaction"
                        })
                    }
                }
                catch (e)
                {
                    this.reportExceptionInDerivation(e)
                }
            }
            this.SharedState.EndBatch();
        }
    }
}
