// <copyright file="SharedStateReactionExtensions.cs" company="Michel Weststrate, Jan-Willem Spuij">
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

namespace Cortex.Net.Api
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Cortex.Net.Core;
    using Cortex.Net.Properties;

    /// <summary>
    /// Extension methods that deal with Reactions on an ISharedState instance.
    /// </summary>
    public static class SharedStateReactionExtensions
    {
        /// <summary>
        /// Creates a new Autorun reaction.
        /// </summary>
        /// <param name="sharedState">The shared state to operate on.</param>
        /// <param name="expression">The expression that should autorun.</param>
        /// <param name="autorunOptions">An <see cref="AutorunOptions"/> instance with options.</param>
        /// <returns>An <see cref="IDisposable"/> instance that can be used to tear down the Autorun reaction.</returns>
        public static IDisposable Autorun(this ISharedState sharedState, Action<Reaction> expression, AutorunOptions autorunOptions = null)
        {
            if (sharedState is null)
            {
                throw new ArgumentNullException(nameof(sharedState));
            }

            if (expression is null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            if (autorunOptions is null)
            {
                autorunOptions = new AutorunOptions();
            }

            var name = autorunOptions.Name ?? $"Autorun@{sharedState.GetUniqueId()}";

            var runSync = autorunOptions.Scheduler == null && autorunOptions.Delay == 0;

            Reaction reaction = null;

#pragma warning disable CA2000 // Dispose objects before losing scope
#pragma warning disable IDE0067 // Dispose objects before losing scope
            if (runSync)
            {
                // normal autorun
                reaction = new Reaction(
                sharedState,
                name,
                ReactionRunner,
                autorunOptions.ErrorHandler);
            }
            else
            {
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
                var scheduler = CreateSchedulerFromOptions(autorunOptions, async () =>
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
                {
                    ReactionRunner();
                });

                // debounced autorun
                var isScheduled = false;

                reaction = reaction = new Reaction(
                sharedState,
                name,
                () =>
                {
                    if (!isScheduled)
                    {
                        var taskScheduler = sharedState.GetTaskScheduler();
                        isScheduled = true;
                        Task.Factory.StartNew(
                            scheduler,
                            CancellationToken.None,
                            TaskCreationOptions.DenyChildAttach,
                            taskScheduler).Unwrap().ContinueWith(
                                t =>
                                {
                                    if (t.Exception != null)
                                    {
                                        reaction.ReportExceptionInReaction(t.Exception);
                                    }
                                }, taskScheduler);
                    }
                },
                autorunOptions.ErrorHandler);
            }
#pragma warning restore IDE0067 // Dispose objects before losing scope
#pragma warning restore CA2000 // Dispose objects before losing scope

            void ReactionRunner()
            {
                if (reaction.IsDisposed)
                {
                    return;
                }

                reaction.Track(() => expression(reaction));
            }

            reaction.Schedule();
            return new DisposableDelegate(() => reaction.Dispose());
        }

        /// <summary>
        /// Creates a reaction that operates on data of type T.
        /// </summary>
        /// <typeparam name="T">The type the reaction operates on.</typeparam>
        /// <param name="sharedState">The shared state to use.</param>
        /// <param name="expression">The expression that delivers a value.</param>
        /// <param name="effect">The effect that is executed when the value changes.</param>
        /// <param name="reactionOptions">The options to use for the reaction.</param>
        /// <returns>An <see cref="IDisposable"/> instance that can be used to stop the reaction.</returns>
        public static IDisposable Reaction<T>(this ISharedState sharedState, Func<Reaction, T> expression, Action<T, Reaction> effect, ReactionOptions<T> reactionOptions = null)
        {
            if (sharedState is null)
            {
                throw new ArgumentNullException(nameof(sharedState));
            }

            if (expression is null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            if (effect is null)
            {
                throw new ArgumentNullException(nameof(effect));
            }

            if (reactionOptions is null)
            {
                reactionOptions = new ReactionOptions<T>();
            }

            var name = reactionOptions.Name ?? $"Reaction@{sharedState.GetUniqueId()}";
            var action = sharedState.CreateAction(
                name + "effect",
                reactionOptions.Context,
                reactionOptions.ErrorHandler != null ? WrapErrorHandler(reactionOptions.ErrorHandler, effect) : effect);
            var runSync = reactionOptions.Scheduler == null && reactionOptions.Delay == 0;

            var firstTime = true;
            var isScheduled = false;
            T value = default;
            Reaction reaction = null;

            var equals =
                reactionOptions.EqualityComparer != null ?
                new Func<T, T, bool>(reactionOptions.EqualityComparer.Equals) :
                new Func<T, T, bool>((x, y) => Equals(x, y));

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
            var scheduler = CreateSchedulerFromOptions(reactionOptions, async () =>
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            {
                ReactionRunner();
            });

#pragma warning disable CA2000 // Dispose objects before losing scope
#pragma warning disable IDE0067 // Dispose objects before losing scope
            reaction = new Reaction(
                sharedState,
                name,
                () =>
                {
                    if (firstTime || runSync)
                    {
                        ReactionRunner();
                    }
                    else if (!isScheduled)
                    {
                        var taskScheduler = sharedState.GetTaskScheduler();
                        isScheduled = true;
                        Task.Factory.StartNew(
                            scheduler,
                            CancellationToken.None,
                            TaskCreationOptions.DenyChildAttach,
                            taskScheduler).Unwrap().ContinueWith(
                                t =>
                                {
                                    if (t.Exception != null)
                                    {
                                        reaction.ReportExceptionInReaction(t.Exception);
                                    }
                                }, taskScheduler);
                    }
                }, reactionOptions.ErrorHandler);
#pragma warning restore IDE0067 // Dispose objects before losing scope
#pragma warning restore CA2000 // Dispose objects before losing scope

            void ReactionRunner()
            {
                isScheduled = false; // Q: move into reaction runner?
                if (reaction.IsDisposed)
                {
                    return;
                }

                var changed = false;

                reaction.Track(() =>
                {
                    var nextValue = expression(reaction);
                    changed = firstTime || !equals(value, nextValue);
                    value = nextValue;
                });
                if (firstTime && reactionOptions.FireImmediately)
                {
                    action(value, reaction);
                }

                if (!firstTime && changed)
                {
                    action(value, reaction);
                }

                if (firstTime)
                {
                    firstTime = false;
                }
            }

            reaction.Schedule();
            return new DisposableDelegate(() => reaction.Dispose());
        }

        /// <summary>
        /// Creates a reaction that operates on data of type T.
        /// </summary>
        /// <typeparam name="T">The type the reaction operates on.</typeparam>
        /// <param name="sharedState">The shared state to use.</param>
        /// <param name="expression">The expression that delivers a value.</param>
        /// <param name="effect">The effect that is executed when the value changes.</param>
        /// <param name="reactionOptions">The options to use for the reaction.</param>
        /// <returns>An <see cref="IDisposable"/> instance that can be used to stop the reaction.</returns>
        /// <remarks>Only pass asynchronous effect functions that wrap state modifications in actions.</remarks>
        public static IDisposable Reaction<T>(this ISharedState sharedState, Func<Reaction, T> expression, Func<T, Reaction, Task> effect, ReactionOptions<T> reactionOptions = null)
        {
            if (sharedState is null)
            {
                throw new ArgumentNullException(nameof(sharedState));
            }

            if (expression is null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            if (effect is null)
            {
                throw new ArgumentNullException(nameof(effect));
            }

            if (reactionOptions is null)
            {
                reactionOptions = new ReactionOptions<T>();
            }

            var name = reactionOptions.Name ?? $"Reaction@{sharedState.GetUniqueId()}";
            var action = reactionOptions.ErrorHandler != null ? WrapErrorHandler(reactionOptions.ErrorHandler, effect) : effect;
            var runSync = reactionOptions.Scheduler == null && reactionOptions.Delay == 0;

            var firstTime = true;
            var isScheduled = false;
            T value = default;
            Reaction reaction = null;

            var equals =
                reactionOptions.EqualityComparer != null ?
                new Func<T, T, bool>(reactionOptions.EqualityComparer.Equals) :
                new Func<T, T, bool>((x, y) => Equals(x, y));

            var scheduler = CreateSchedulerFromOptions(reactionOptions, async () =>
            {
                await ReactionRunner().ConfigureAwait(true);
            });

#pragma warning disable CA2000 // Dispose objects before losing scope
#pragma warning disable IDE0067 // Dispose objects before losing scope
            reaction = new Reaction(
                sharedState,
                name,
                () =>
                {
                    if (firstTime || runSync)
                    {
                        var taskScheduler = sharedState.GetTaskScheduler();
                        isScheduled = true;
                        Task.Factory.StartNew(
                            ReactionRunner,
                            CancellationToken.None,
                            TaskCreationOptions.DenyChildAttach,
                            taskScheduler).Unwrap().ContinueWith(
                                t =>
                                {
                                    if (t.Exception != null)
                                    {
                                        reaction.ReportExceptionInReaction(t.Exception);
                                    }
                                }, taskScheduler);
                    }
                    else if (!isScheduled)
                    {
                        var taskScheduler = sharedState.GetTaskScheduler();
                        isScheduled = true;
                        Task.Factory.StartNew(
                            scheduler,
                            CancellationToken.None,
                            TaskCreationOptions.DenyChildAttach,
                            taskScheduler).Unwrap().ContinueWith(
                                t =>
                                {
                                    if (t.Exception != null)
                                    {
                                        reaction.ReportExceptionInReaction(t.Exception);
                                    }
                                }, taskScheduler);
                    }
                }, reactionOptions.ErrorHandler);
#pragma warning restore IDE0067 // Dispose objects before losing scope
#pragma warning restore CA2000 // Dispose objects before losing scope

            async Task ReactionRunner()
            {
                isScheduled = false; // Q: move into reaction runner?
                if (reaction.IsDisposed)
                {
                    return;
                }

                var changed = false;

                reaction.Track(() =>
                {
                    var nextValue = expression(reaction);
                    changed = firstTime || !equals(value, nextValue);
                    value = nextValue;
                });
                if (firstTime && reactionOptions.FireImmediately)
                {
                    await action(value, reaction).ConfigureAwait(true);
                }

                if (!firstTime && changed)
                {
                    await action(value, reaction).ConfigureAwait(true);
                }

                if (firstTime)
                {
                    firstTime = false;
                }
            }

            reaction.Schedule();
            return new DisposableDelegate(() => reaction.Dispose());
        }

        /// <summary>
        /// Tries to get a Task scheduler or throws an exception.
        /// </summary>
        /// <param name="sharedState">The shared state to use.</param>
        /// <returns>The task scheduler.</returns>
        /// <exception cref="InvalidOperationException">When a task scheduler was not specified or could not be inferred from a SynchronizationContext.</exception>
        internal static TaskScheduler GetTaskScheduler(this ISharedState sharedState)
        {
            if (sharedState is null)
            {
                throw new ArgumentNullException(nameof(sharedState));
            }

            if (sharedState.Configuration.SynchronizationContext != null)
            {
                return sharedState.Configuration.TaskScheduler;
            }

            throw new InvalidOperationException(Resources.TaskSchedulerNull);
        }

        /// <summary>
        /// Wraps the error handler function around an action.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="errorHandler">The errorhandler to use.</param>
        /// <param name="action">The action to wrap.</param>
        /// <returns>The wrapped action.</returns>
        private static Action<T, Reaction> WrapErrorHandler<T>(Action<Reaction, Exception> errorHandler, Action<T, Reaction> action)
        {
            return (value, reaction) =>
            {
                try
                {
                    action(value, reaction);
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception exception)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    errorHandler(reaction, exception);
                }
            };
        }

        /// <summary>
        /// Wraps the error handler function around an action.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="errorHandler">The errorhandler to use.</param>
        /// <param name="asyncAction">The asynchronous action action to wrap.</param>
        /// <returns>The wrapped action.</returns>
        private static Func<T, Reaction, Task> WrapErrorHandler<T>(Action<Reaction, Exception> errorHandler, Func<T, Reaction, Task> asyncAction)
        {
            return async (value, reaction) =>
            {
                try
                {
                    await asyncAction(value, reaction).ConfigureAwait(true);
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception exception)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    errorHandler(reaction, exception);
                }
            };
        }

        /// <summary>
        /// Creates a default scheduler function for reactions.
        /// </summary>
        /// <param name="options">The options to run the scheduler with.</param>
        /// <param name="action">The action to execute.</param>
        /// <returns>A Scheduler function.</returns>
        private static Func<Task> CreateSchedulerFromOptions(AutorunOptions options, Func<Task> action)
        {
            return async () =>
                {
                    await Task.Delay(options.Delay).ConfigureAwait(true);
                    await action().ConfigureAwait(true);
                };
        }
    }
}
