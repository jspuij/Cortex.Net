// <copyright file="SharedState.cs" company="Michel Weststrate, Jan-Willem Spuij">
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
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using Cortex.Net.Core;
    using Cortex.Net.Properties;
    using Cortex.Net.Spy;

    /// <summary>
    /// Holds the Shared state that all nodes of the Dependency Graph share.
    /// </summary>
    public sealed partial class SharedState : ISharedState
    {
        /// <summary>
        /// Batch counter to support reentrance of Start and EndBatch.
        /// </summary>
        private int batchCount = 0;

        /// <summary>
        /// A unique Id that is incremented and used to identify instances.
        /// </summary>
        private int uniqueId = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="SharedState"/> class.
        /// </summary>
        /// <param name="configuration">The configuration to initialize <see cref="SharedState"/> instance with.</param>
        /// <exception cref="ArgumentNullException">Thrown when any of the arguments is null.</exception>
        public SharedState(CortexConfiguration configuration)
        {
            this.Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Spy event that fires when any observable attached to this Shared State reports a significant change.
        /// Can be used to implement a state inspection tool or something like react-dev-tools.
        /// </summary>
        public event EventHandler<SpyEventArgs> SpyEvent;

        /// <summary>
        /// Gets a queue of all pending Unobservations.
        /// </summary>
        public Queue<IObservable> PendingUnobservations { get; } = new Queue<IObservable>();

        /// <summary>
        /// Gets a value indicating whether the Dependency Graph is in Batch mode.
        /// </summary>
        public bool InBatch => this.batchCount > 0;

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        public CortexConfiguration Configuration { get; private set; }

        /// <summary>
        /// Gets a value indicating whether it is allowed to change observables at this point.
        /// </summary>
        public bool AllowStateChanges { get; private set; }

        /// <summary>
        /// Gets a value indicating whether it is allowed to read observables at this point.
        /// </summary>
        public bool AllowStateReads { get; private set; }

        /// <summary>
        /// Gets the <see cref="IDerivation"/> instance that the shared state is currently tracking.
        /// </summary>
        public IDerivation TrackingDerivation { get; private set; }

        /// <summary>
        /// Gets the shared derivation RunId counter.
        /// </summary>
        public int RunId { get; private set; } = 0;

        /// <summary>
        /// Gets or sets the computation depth.
        /// </summary>
        public int ComputationDepth { get; set; } = 0;

        /// <summary>
        /// Gets or sets a value indicating whether to suppress reaction errors.
        /// Suppressing happens when an action is the root cause of reactions to fail further because of the incorrect state.
        /// </summary>
        public bool SuppressReactionErrors { get; set; } = false;

        /// <summary>
        /// Gets or sets the Id of the Current Action.
        /// </summary>
        public int CurrentActionId { get; set; } = 0;

        /// <summary>
        /// Gets or sets the Id of the Next Action.
        /// </summary>
        public int NextActionId { get; set; } = 1;

        /// <summary>
        /// Gets a queue of pending reactions.
        /// </summary>
        public Queue<Reaction> PendingReactions { get; } = new Queue<Reaction>();

        /// <summary>
        /// Gets a value indicating whether this shared state is running reactions.
        /// </summary>
        public bool IsRunningReactions { get; private set; }

        /// <summary>
        /// Gets or sets the Scheduler function. The Scheduler function can call
        /// it's inner action to run the default reaction algorithm in Cortex.NET.
        /// </summary>
        public Action<Action> ReactionScheduler { get; set; } = x => x();

        /// <summary>
        /// Gets a unique Id that is incremented every time and identifies unique instances.
        /// </summary>
        /// <returns>The new unique Id.</returns>
        public int GetUniqueId()
        {
            return ++this.uniqueId;
        }

        /// <summary>
        /// Starts a Batch.
        /// </summary>
        /// <remarks>
        /// This method can be called multiple times but should always be balanced with an equal amount of <see cref="EndBatch"/> calls.
        /// </remarks>
        public void StartBatch()
        {
            this.batchCount++;
        }

        /// <summary>
        /// Ends a Batch.
        /// </summary>
        /// <remarks>
        /// This method can be called multiple times but should always be balanced with an equal amount of <see cref="StartBatch"/> calls.
        /// </remarks>
        public void EndBatch()
        {
            if (--this.batchCount == 0)
            {
                this.RunReactions();

                // the batch is actually about to finish, all unobserving should happen here.
                foreach (var observable in this.PendingUnobservations)
                {
                    observable.IsPendingUnobservation = false;
                    if (observable.HasObservers())
                    {
                        if (observable.IsBeingObserved)
                        {
                            observable.IsBeingObserved = false;
                            observable.OnBecomeUnobserved();
                        }

                        if (observable is IComputedValue<object> computedValue)
                        {
                            computedValue.Suspend();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Starts an untracked part of a derviation.
        /// </summary>
        /// <returns>The current derivation to restore later.</returns>
        public IDerivation StartUntracked()
        {
            var result = this.TrackingDerivation;
            this.TrackingDerivation = null;
            return result;
        }

        /// <summary>
        /// Start of a section where allowedStateReads is modified.
        /// </summary>
        /// <param name="allowStateReads">Whether to allow State reads.</param>
        /// <returns>The previous value.</returns>
        public bool StartAllowStateReads(bool allowStateReads)
        {
            var result = this.AllowStateReads;
            this.AllowStateReads = allowStateReads;
            return result;
        }

        /// <summary>
        /// Increments the RunId and returns the new value.
        /// </summary>
        /// <returns>The new RunId.</returns>
        public int IncrementRunId()
        {
            return ++this.RunId;
        }

        /// <summary>
        /// Starts tracking the <see cref="IDerivation"/> instance given as paramteter.
        /// </summary>
        /// <param name="derivation">The derivation to track.</param>
        /// <returns>The prevous derivation.</returns>
        public IDerivation StartTracking(IDerivation derivation)
        {
            var result = this.TrackingDerivation;
            this.TrackingDerivation = derivation;
            return result;
        }

        /// <summary>
        /// Ends tracking the current <see cref="IDerivation"/> instance and restores the previous derivation.
        /// </summary>
        /// <param name="previousDerivation">The previous derivation.</param>
        public void EndTracking(IDerivation previousDerivation)
        {
            this.TrackingDerivation = previousDerivation;
        }

        /// <summary>
        /// End of a section where allowedStateReads is modified.
        /// </summary>
        /// <param name="previousAllowStateReads">The previous value to restore.</param>
        public void EndAllowStateReads(bool previousAllowStateReads)
        {
            this.AllowStateReads = previousAllowStateReads;
        }

        /// <summary>
        /// Creates a new Atom that references this shared Storage.
        /// </summary>
        /// <param name="name">The name for this Atom. A unique name will be generated in case a name is not provided.</param>
        /// <param name="onBecomeObserved">An <see cref="Action"/> that will be executed when the Atom changes from unobserved to observed.</param>
        /// <param name="onBecomeUnobserved">An <see cref="Action"/> that will be executed when the Atom changes from observed to unobserved.</param>
        /// <returns>A new instance that implements <see cref="IAtom"/>.</returns>
        public IAtom CreateAtom(string name = null, Action onBecomeObserved = null, Action onBecomeUnobserved = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                name = $"Atom@{this.GetUniqueId()}";
            }

            var result = new Atom(this, name);

            if (onBecomeObserved != null)
            {
                result.BecomeObserved += (s, e) => onBecomeObserved();
            }

            if (onBecomeUnobserved != null)
            {
                result.BecomeUnobserved += (s, e) => onBecomeUnobserved();
            }

            return result;
        }

        /// <summary>
        /// Triggers the Spy event handler with the specified event args.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="spyEventArgs">The event arguments for the spy event.</param>
        public void OnSpy(object sender, SpyEventArgs spyEventArgs)
        {
            this.SpyEvent?.Invoke(sender, spyEventArgs);
        }

        /// <summary>
        /// Start of a section where <see cref="AllowStateChanges"/> is modified.
        /// </summary>
        /// <param name="allowStateChanges">Whether to allow State changes.</param>
        /// <returns>The previous value.</returns>
        public bool StartAllowStateChanges(bool allowStateChanges)
        {
            var result = this.AllowStateChanges;
            this.AllowStateChanges = allowStateChanges;
            return result;
        }

        /// <summary>
        /// End of a section where <see cref="AllowStateChanges"/> is modified.
        /// </summary>
        /// <param name="previousAllowStateChanges">The previous value to restore.</param>
        public void EndAllowStateChanges(bool previousAllowStateChanges)
        {
            this.AllowStateChanges = previousAllowStateChanges;
        }

        /// <summary>
        /// Runs reactions. This implementation will run the default Reaction Scheduler function.
        /// </summary>
        public void RunReactions()
        {
            // Trampolining, if runReactions are already running, new reactions will be picked up
            if (this.InBatch || this.IsRunningReactions)
            {
                return;
            }

            this.ReactionScheduler(new Action(this.RunReactionsDefaultAction));
        }

        /// <summary>
        /// Default action that is ran when a new iteration of reactions is scheduled.
        /// </summary>
        private void RunReactionsDefaultAction()
        {
            this.IsRunningReactions = true;
            var iterations = 0;

            try
            {
                // While running reactions, new reactions might be triggered.
                // Hence we work with two variables and check whether
                // we converge to no remaining reactions after a while.
                while (this.PendingReactions.Any())
                {
                    if (++iterations == this.Configuration.MaxReactionIteractions)
                    {
                        var reaction = this.PendingReactions.Peek();
                        this.PendingReactions.Clear();
                        throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.ReactionDoesNotConverge, iterations, reaction.Name));
                    }

                    var remainingReactions = this.PendingReactions.ToList();
                    this.PendingReactions.Clear();

                    foreach (var reaction in remainingReactions)
                    {
                        reaction.RunReaction();
                    }
                }
            }
            finally
            {
                this.IsRunningReactions = false;
            }
        }
    }
}
