// <copyright file="SharedStateExtensions.cs" company="Michel Weststrate, Jan-Willem Spuij">
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
    using Cortex.Net.Api;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Extensions class for <see cref="ISharedState"/> instances.
    /// </summary>
    public static partial class ActionExtensions
    {
        /// <summary>
        /// Executes a function without tracking derivations.
        /// </summary>
        /// <typeparam name="T">The type of the return value of the function.</typeparam>
        /// <param name="sharedState">The <see cref="ISharedState"/> instance to use to temporarily stop tracking derivations.</param>
        /// <param name="function">The function to execute.</param>
        /// <returns>The return value.</returns>
        public static T Untracked<T>(this ISharedState sharedState, Func<T> function)
        {
            if (sharedState is null)
            {
                throw new ArgumentNullException(nameof(sharedState));
            }

            if (function is null)
            {
                throw new ArgumentNullException(nameof(function));
            }

            var previousDerivation = sharedState.StartUntracked();
            try
            {
                return function();
            }
            finally
            {
                sharedState.EndTracking(previousDerivation);
            }
        }

        /// <summary>
        /// Executes a function while specifying <see cref="ISharedState.AllowStateChanges"/>. The previous value of
        /// <see cref="ISharedState.AllowStateChanges"/> is automatically restored.
        /// </summary>
        /// <typeparam name="T">The result type of the function.</typeparam>
        /// <param name="sharedState">The shared state to operate on.</param>
        /// <param name="allowStateChanges">The value for AllStateChanges to use while executing the function.</param>
        /// <param name="function">The function to execute.</param>
        /// <returns>The return value of the function.</returns>
        public static T AllowStateChanges<T>(this ISharedState sharedState, bool allowStateChanges, Func<T> function)
        {
            if (sharedState is null)
            {
                throw new ArgumentNullException(nameof(sharedState));
            }

            if (function is null)
            {
                throw new ArgumentNullException(nameof(function));
            }

            var previousAllowStateChanges = sharedState.StartAllowStateChanges(allowStateChanges);

            try
            {
                return function();
            }
            finally
            {
                sharedState.EndAllowStateChanges(previousAllowStateChanges);
            }
        }

        /// <summary>
        /// Creates a reaction that operates on data of type T.
        /// </summary>
        /// <typeparam name="T">The type the reaction operates on.</typeparam>
        /// <param name="sharedState">The shared state to use.</param>
        /// <param name="expression">The expression that delivers a value.</param>
        /// <param name="effect">The effect that is executed when the value changes.</param>
        /// <param name="options">The options to use for the reaction.</param>
        /// <returns>An <see cref="IDisposable"/> instance that can be used to stop the reaction.</returns>
        public static IDisposable Reaction<T>(this ISharedState sharedState, Func<Reaction, T> expression, Action<T, Reaction> effect, ReactionOptions<T> options = null)
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

            if (options is null)
            {
                options = new ReactionOptions<T>();
            }

            var name = options.Name ?? $"Reaction@{sharedState.GetUniqueId()}";
            var action = sharedState.CreateAction(
                name + "effect",
                options.Context,
                options.ErrorHandler != null ? WrapErrorHandler(options.ErrorHandler, effect) : effect);
            var runSync = options.Scheduler != null && options.Delay == 0;

            var firstTime = true;
            var isScheduled = false;
            T value = default;
            Reaction reaction = null;

            var equals =
                options.EqualityComparer != null ?
                new Func<T, T, bool>(options.EqualityComparer.Equals) :
                new Func<T, T, bool>((x, y) => Equals(x, y));

            var scheduler = CreateSchedulerFromOptions(options, ReactionRunner);

#pragma warning disable CA2000 // Dispose objects before losing scope
            reaction = new Reaction(
                sharedState,
                name,
                () =>
                {
                    if (firstTime || runSync)
                    {
                        ReactionRunner().RunSynchronously();
                    }
                    else if (!isScheduled)
                    {
                        isScheduled = true;
                        scheduler().RunSynchronously();
                    }
                });
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
                if (firstTime && options.FireImmediately)
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
        private static Action<T, Reaction> WrapErrorHandler<T>(Action<Exception> errorHandler, Action<T, Reaction> action)
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
                    errorHandler(exception);
                }
            });
        }

        private static Func<Task> CreateSchedulerFromOptions<T>(ReactionOptions<T> options, Func<Task> action)
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
