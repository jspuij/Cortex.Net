﻿// <copyright file="Reaction.cs" company="Michel Weststrate, Jan-Willem Spuij">
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
    using Cortex.Net.Spy;

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
        /// Handler that is executed when this reaction is invalidated.
        /// </summary>
        private readonly Action onInvalidate;

        /// <summary>
        /// The error handler to use for this reaction.
        /// </summary>
        private readonly Action<Reaction, Exception> errorHandler;

        /// <summary>
        /// Indicates whether this reaction is scheduled.
        /// </summary>
        private bool isScheduled;

        /// <summary>
        /// Indicates whether this reaction is disposed.
        /// </summary>
        private bool isDisposed;

        /// <summary>
        /// Indicates whether tracking is pending.
        /// </summary>
        private bool isTrackPending;

        /// <summary>
        /// Indicaes whether tracking is running.
        /// </summary>
        private bool isRunning;

        /// <summary>
        /// Initializes a new instance of the <see cref="Reaction"/> class.
        /// </summary>
        /// <param name="sharedState">The shared state to use.</param>
        /// <param name="name">The name to use.</param>
        /// <param name="onInvalidate">Handler to run when this reaction is invalidated. This handler should call <see cref="Track"/>.</param>
        /// <param name="errorHandler">The error handler for this reaction.</param>
        internal Reaction(ISharedState sharedState, string name, Action onInvalidate, Action<Reaction, Exception> errorHandler)
        {
            // resolve state state from context if necessary.
            this.SharedState = Net.SharedState.ResolveState(sharedState);
            this.onInvalidate = onInvalidate ?? throw new ArgumentNullException(nameof(onInvalidate));
            this.errorHandler = errorHandler;
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
        /// Gets a value indicating whether this reaction is disposed.
        /// </summary>
        public bool IsDisposed { get => this.isDisposed; }

        /// <summary>
        /// Disposes the reaction by clearing the observables it is observing.
        /// </summary>
        public void Dispose()
        {
            if (!this.isDisposed)
            {
                this.isDisposed = true;
                if (!this.isRunning)
                {
                    // if disposed while running, clean up later. Maybe not optimal, but rare case
                    this.SharedState.StartBatch();
                    this.ClearObserving();
                    this.SharedState.EndBatch();
                }
            }
        }

        /// <summary>
        /// Propagates confirmation of a possible change to all observers of
        /// this <see cref="Reaction"/> for delayed computation.
        /// </summary>
        public void OnBecomeStale()
        {
            this.Schedule();
        }

        /// <summary>
        /// Tracks this reaction using the specified action.
        /// </summary>
        /// <param name="action">The action to use to track.</param>
        public void Track(Action action)
        {
            if (this.isDisposed)
            {
                return;
            }

            this.SharedState.StartBatch();

            this.SharedState.OnSpy(this, new ReactionStartSpyEventArgs()
            {
                Name = this.Name,
                StartTime = DateTime.UtcNow,
            });

            this.isRunning = true;

            object result = null;
            Exception exception = null;

            (result, exception) = this.TrackDerivedFunction(() =>
            {
                action();
                return result;
            });

            this.isRunning = false;
            this.isTrackPending = false;
            if (this.isDisposed)
            {
                // disposed during last run. Clean up everything that was bound after the dispose call.
                this.ClearObserving();
            }

            if (exception != null)
            {
                this.ReportExceptionInReaction(exception);
            }

            this.SharedState.OnSpy(this, new ReactionEndSpyEventArgs()
            {
                Name = this.Name,
                EndTime = DateTime.UtcNow,
            });

            this.SharedState.EndBatch();
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

            if (this.ShouldCompute())
            {
                this.isTrackPending = true;

                try
                {
                    this.onInvalidate();
                    if (this.isTrackPending)
                    {
                        this.SharedState.OnSpy(this, new ReactionScheduledSpyEventArgs()
                        {
                            Name = this.Name,
                        });
                    }
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception exception)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    this.ReportExceptionInReaction(exception);
                }
            }

            this.SharedState.EndBatch();
        }

        /// <summary>
        /// Schedules the reaction for execution.
        /// </summary>
        internal void Schedule()
        {
            if (!this.isScheduled)
            {
                this.isScheduled = true;
                this.SharedState.PendingReactions.Enqueue(this);
                this.SharedState.RunReactions();
            }
        }

        /// <summary>
        /// Reports an Exception in the reaction.
        /// </summary>
        /// <param name="exception">The exception to report.</param>
        internal void ReportExceptionInReaction(Exception exception)
        {
            if (this.errorHandler != null)
            {
                this.errorHandler(this, exception);
                return;
            }

            if (this.SharedState.Configuration.DisableErrorBoundaries)
            {
                throw exception;
            }

            var message = string.Format(CultureInfo.CurrentCulture, Resources.UncaughtExceptionInsideReaction, this.Name);

            if (this.SharedState.SuppressReactionErrors)
            {
                Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, Resources.UncaughtExceptionInsideReactionSuppressed, this.Name));
            }
            else
            {
                Trace.WriteLine(message);
                Trace.WriteLine(exception.ToString());
                Debugger.Break();
                /* If debugging brought you here, please, read the above message :-). Tnx! */
            }

            this.SharedState.OnSpy(this, new ReactionExceptionSpyEventArgs()
            {
                Name = this.Name,
                Message = message,
                Exception = exception,
            });

            this.SharedState.OnUnhandledReactionException(this, new UnhandledExceptionEventArgs(exception, false));
        }
    }
}
