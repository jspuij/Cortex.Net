﻿// <copyright file="SharedStateReactionExtensions.cs" company="Michel Weststrate, Jan-Willem Spuij">
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
    using System.Threading.Tasks;
    using Cortex.Net.Core;

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
                () =>
                {
                    ReactionRunner().GetAwaiter().GetResult();
                }, autorunOptions.ErrorHandler);
            }
            else
            {
                var scheduler = CreateSchedulerFromOptions(autorunOptions, ReactionRunner);

                // debounced autorun
                var isScheduled = false;

                reaction = reaction = new Reaction(
                sharedState,
                name,
                () =>
                {
                    if (!isScheduled)
                    {
                        isScheduled = true;
                        scheduler().GetAwaiter().GetResult();
                    }
                },
                autorunOptions.ErrorHandler);
            }
#pragma warning restore IDE0067 // Dispose objects before losing scope
#pragma warning restore CA2000 // Dispose objects before losing scope

            Task ReactionRunner()
            {
                if (reaction.IsDisposed)
                {
                    return Task.CompletedTask;
                }

                reaction.Track(() => expression(reaction));
                return Task.CompletedTask;
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

            var scheduler = CreateSchedulerFromOptions(reactionOptions, ReactionRunner);

#pragma warning disable CA2000 // Dispose objects before losing scope
#pragma warning disable IDE0067 // Dispose objects before losing scope
            reaction = new Reaction(
                sharedState,
                name,
                () =>
                {
                    if (firstTime || runSync)
                    {
                        ReactionRunner().GetAwaiter().GetResult();
                    }
                    else if (!isScheduled)
                    {
                        isScheduled = true;
                        scheduler().GetAwaiter().GetResult();
                    }
                }, reactionOptions.ErrorHandler);
#pragma warning restore IDE0067 // Dispose objects before losing scope
#pragma warning restore CA2000 // Dispose objects before losing scope

            Task ReactionRunner()
            {
                isScheduled = false; // Q: move into reaction runner?
                if (reaction.IsDisposed)
                {
                    return Task.CompletedTask;
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

                return Task.CompletedTask;
            }

            reaction.Schedule();
            return new DisposableDelegate(() => reaction.Dispose());
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
            return new Action<T, Reaction>((value, reaction) =>
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
            });
        }

        private static Func<Task> CreateSchedulerFromOptions(AutorunOptions options, Func<Task> action)
        {
            return options.Scheduler ?? (options.Delay > 0
                ? new Func<Task>(async () =>
                {
                    await Task.Delay(options.Delay).ConfigureAwait(true);
                    await action().ConfigureAwait(true);
                })
            : action);
        }
    }
}