// <copyright file="ISharedState.cs" company="Michel Weststrate, Jan-Willem Spuij">
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
    using Cortex.Net.Core;
    using Cortex.Net.Spy;

    /// <summary>
    /// Interface that defines the Shared state that all nodes of the Dependency Graph share.
    /// </summary>
    public interface ISharedState
    {
        /// <summary>
        /// Spy event that fires when any observable attached to this Shared State reports a significant change.
        /// Can be used to implement a state inspection tool or something like react-dev-tools.
        /// </summary>
        event EventHandler<SpyEventArgs> SpyEvent;

        /// <summary>
        /// Event that fires when a reaction produces an unhandled exception.
        /// </summary>
        event EventHandler<UnhandledExceptionEventArgs> UnhandledReactionException;

        /// <summary>
        /// Gets a queue of all pending Unobservations.
        /// </summary>
        Queue<IObservable> PendingUnobservations { get; }

        /// <summary>
        /// Gets a value indicating whether the Dependency Graph is in Batch mode.
        /// </summary>
        bool InBatch { get; }

        /// <summary>
        /// Gets the Configuration for the Shared State.
        /// </summary>
        CortexConfiguration Configuration { get; }

        /// <summary>
        /// Gets a value indicating whether it is allowed to change observables at this point.
        /// </summary>
        bool AllowStateChanges { get; }

        /// <summary>
        /// Gets a value indicating whether it is allowed to read observables at this point.
        /// </summary>
        bool AllowStateReads { get; }

        /// <summary>
        /// Gets the <see cref="IDerivation"/> instance that the shared state is currently tracking.
        /// </summary>
        IDerivation TrackingDerivation { get; }

        /// <summary>
        /// Gets the shared derivation RunId counter.
        /// </summary>
        int RunId { get; }

        /// <summary>
        /// Gets or sets the computation depth.
        /// </summary>
        int ComputationDepth { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to suppress reaction errors.
        /// Suppressing happens when an action is the root cause of reactions to fail further because of the incorrect state.
        /// </summary>
        bool SuppressReactionErrors { get; set; }

        /// <summary>
        /// Gets or sets the Id of the Current Action.
        /// </summary>
        int CurrentActionId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the Next Action.
        /// </summary>
        int NextActionId { get; set; }

        /// <summary>
        /// Gets a queue of pending reactions.
        /// </summary>
        Queue<Reaction> PendingReactions { get; }

        /// <summary>
        /// Gets a list of enhancers.
        /// </summary>
        IList<IEnhancer> Enhancers { get; }

        /// <summary>
        /// Gets a value indicating whether the action should be invoked on the original thread that created the context.
        /// </summary>
        bool ShouldInvoke { get; }

        /// <summary>
        /// Gets a unique Id that is incremented every time.
        /// </summary>
        /// <returns>The new unique Id.</returns>
        int GetUniqueId();

        /// <summary>
        /// Starts a Batch.
        /// </summary>
        /// <remarks>
        /// This method can be called multiple times but should always be balanced with an equal amount of <see cref="EndBatch"/> calls.
        /// </remarks>
        void StartBatch();

        /// <summary>
        /// Ends a Batch.
        /// </summary>
        /// <remarks>
        /// This method can be called multiple times but should always be balanced with an equal amount of <see cref="StartBatch"/> calls.
        /// </remarks>
        void EndBatch();

        /// <summary>
        /// Starts an untracked part of a derviation.
        /// </summary>
        /// <returns>The current derivation to restore later.</returns>
        IDerivation StartUntracked();

        /// <summary>
        /// Start of a section where <see cref="AllowStateChanges"/> is modified.
        /// </summary>
        /// <param name="allowStateChanges">Whether to allow State changes.</param>
        /// <returns>The previous value.</returns>
        bool StartAllowStateChanges(bool allowStateChanges);

        /// <summary>
        /// Start of a section where <see cref="AllowStateReads"/> is modified.
        /// </summary>
        /// <param name="allowStateReads">Whether to allow State reads.</param>
        /// <returns>The previous value.</returns>
        bool StartAllowStateReads(bool allowStateReads);

        /// <summary>
        /// Increments the RunId and returns the new value.
        /// </summary>
        /// <returns>The new RunId.</returns>
        int IncrementRunId();

        /// <summary>
        /// Starts tracking the <see cref="IDerivation"/> instance given as paramteter.
        /// </summary>
        /// <param name="derivation">The derivation to track.</param>
        /// <returns>The prevous derivation.</returns>
        IDerivation StartTracking(IDerivation derivation);

        /// <summary>
        /// Ends tracking the current <see cref="IDerivation"/> instance and restores the previous derivation.
        /// </summary>
        /// <param name="previousDerivation">The previous derivation.</param>
        void EndTracking(IDerivation previousDerivation);

        /// <summary>
        /// End of a section where <see cref="AllowStateReads"/> is modified.
        /// </summary>
        /// <param name="previousAllowStateReads">The previous value to restore.</param>
        void EndAllowStateReads(bool previousAllowStateReads);

        /// <summary>
        /// End of a section where <see cref="AllowStateChanges"/> is modified.
        /// </summary>
        /// <param name="previousAllowStateChanges">The previous value to restore.</param>
        void EndAllowStateChanges(bool previousAllowStateChanges);

        /// <summary>
        /// Triggers the Spy event handler with the specified event args.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="spyEventArgs">The event arguments for the spy event.</param>
        void OnSpy(object sender, SpyEventArgs spyEventArgs);

        /// <summary>
        /// Runs reactions.
        /// </summary>
        void RunReactions();

        /// <summary>
        /// Fires the <see cref="UnhandledReactionException"/> event.
        /// </summary>
        /// <param name="reaction">The reaction that caused the unhandled exception.</param>
        /// <param name="unhandledExceptionEventArgs">The event arguments for the exception.</param>
        void OnUnhandledReactionException(Reaction reaction, UnhandledExceptionEventArgs unhandledExceptionEventArgs);
    }
}
