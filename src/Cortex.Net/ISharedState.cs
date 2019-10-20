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
    using System.Collections.Generic;

    /// <summary>
    /// Interface that defines the Shared state that all nodes of the Dependency Graph share.
    /// </summary>
    public interface ISharedState
    {
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
        /// Ends an untracked part of a derivation by restoring the current derivation.
        /// </summary>
        /// <param name="derivation">The derivation to restore.</param>
        void EndUntracked(IDerivation derivation);

        /// <summary>
        /// Start of a section where allowedStateReads is modified.
        /// </summary>
        /// <param name="allowStateReads">Whether to allow State reads.</param>
        /// <returns>The previous value.</returns>
        bool StartAllowStateReads(bool allowStateReads);

        /// <summary>
        /// Increments the RunId and returns the new value.
        /// </summary>
        /// <returns>The new RunId.</returns>
        int IncrementRunId();
        object StartTracking(IDerivation derivation);
    }
}
