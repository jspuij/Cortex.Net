// <copyright file="SharedStateWhenExtensions.cs" company="Michel Weststrate, Jan-Willem Spuij">
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
    using System.Globalization;
    using System.Threading;
    using System.Threading.Tasks;
    using Cortex.Net.Core;
    using Cortex.Net.Properties;

    /// <summary>
    /// Extension methods on the shared state than create an Autorun that observes and runs the given predicate until it returns true.
    /// Once that happens, the given effect is executed and the autorunner is disposed.
    /// The function returns a disposer to cancel the autorunner prematurely.
    /// </summary>
    public static class SharedStateWhenExtensions
    {
        /// <summary>
        ///  Creates an autorun that observes and runs the given predicate until it returns true.
        ///  Once that happens, the given effect is executed and the autorunner is disposed.
        ///  The function returns a disposer to cancel the autorunner prematurely.
        /// </summary>
        /// <param name="sharedState">The shared state to use.</param>
        /// <param name="predicate">The predicate to match.</param>
        /// <param name="effect">The effect to run.</param>
        /// <param name="whenOptions">The options for this When.</param>
        /// <returns>A disposer to cancel the autorunner prematurely.</returns>
        public static IDisposable When(this ISharedState sharedState, Func<bool> predicate, Action effect, WhenOptions whenOptions = null)
        {
            if (sharedState is null)
            {
                throw new ArgumentNullException(nameof(sharedState));
            }

            if (predicate is null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            if (effect is null)
            {
                throw new ArgumentNullException(nameof(effect));
            }

            if (whenOptions is null)
            {
                whenOptions = new WhenOptions();
            }

            IDisposable result = null;
            Reaction reaction = null;
            CancellationTokenSource cancellationTokenSource = null;

            whenOptions.Name = !string.IsNullOrEmpty(whenOptions.Name) ? whenOptions.Name : $"When@{sharedState.GetUniqueId()}";

            if (whenOptions.TimeOut.HasValue)
            {
#pragma warning disable CA2000 // Dispose objects before losing scope
                cancellationTokenSource = new CancellationTokenSource();
#pragma warning restore CA2000 // Dispose objects before losing scope

                var taskScheduler = sharedState.GetTaskScheduler();
                Task.Factory.StartNew(
                            async () =>
                            {
                                var token = cancellationTokenSource.Token;
                                await Task.Delay(whenOptions.TimeOut.Value, token).ConfigureAwait(true);
                                if (!token.IsCancellationRequested)
                                {
                                    throw new TimeoutException(
                                        string.Format(CultureInfo.CurrentCulture, Resources.TimeoutOccuredInWhen, whenOptions.Name, whenOptions.TimeOut.Value));
                                }
                            },
                            CancellationToken.None,
                            TaskCreationOptions.DenyChildAttach,
                            taskScheduler).Unwrap().ContinueWith(
                                t =>
                                {
                                    try
                                    {
                                        if (t.Exception != null)
                                        {
                                            reaction.ReportExceptionInReaction(t.Exception);
                                        }
                                    }
                                    finally
                                    {
                                        cancellationTokenSource.Dispose();
                                        result.Dispose();
                                    }
                                }, taskScheduler);
            }

            var autorunOptions = new AutorunOptions()
            {
                Name = $"{whenOptions.Name}-effect",
                RequiresObservable = whenOptions.RequiresObservable,
            };

            if (whenOptions.ErrorHandler != null)
            {
                autorunOptions.ErrorHandler = (r, e) => whenOptions.ErrorHandler(e);
            }

#pragma warning disable CA2000 // Dispose objects before losing scope
            result = sharedState.Autorun(
                r =>
                {
                    reaction = r;
                    if (predicate())
                    {
                        r.Dispose();
                        cancellationTokenSource?.Cancel();
                        effect();
                    }
                }, autorunOptions);
#pragma warning restore CA2000 // Dispose objects before losing scope

            return new DelegateDisposable(() =>
            {
                result.Dispose();
                cancellationTokenSource?.Cancel();
            });
        }

        /// <summary>
        ///  Creates an autorun that observes and runs the given predicate until it returns true.
        ///  Once that happens, the given effect is executed and the autorunner is disposed.
        ///  The function returns a disposer to cancel the autorunner prematurely.
        /// </summary>
        /// <param name="sharedState">The shared state to use.</param>
        /// <param name="predicate">The predicate to match.</param>
        /// <param name="whenOptions">The options for this When.</param>
        /// <returns>A task to await in asynchronous code..</returns>
        public static Task When(this ISharedState sharedState, Func<bool> predicate, WhenOptions whenOptions = null)
        {
            return sharedState.When(predicate, CancellationToken.None, whenOptions);
        }

        /// <summary>
        ///  Creates an autorun that observes and runs the given predicate until it returns true.
        ///  Once that happens, the given effect is executed and the autorunner is disposed.
        ///  The function returns a disposer to cancel the autorunner prematurely.
        /// </summary>
        /// <param name="sharedState">The shared state to use.</param>
        /// <param name="predicate">The predicate to match.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
        /// <param name="whenOptions">The options for this When.</param>
        /// <returns>A task to await in asynchronous code..</returns>
        public static Task When(this ISharedState sharedState, Func<bool> predicate, CancellationToken cancellationToken, WhenOptions whenOptions = null)
        {
            if (whenOptions != null && whenOptions.ErrorHandler != null)
            {
                throw new InvalidOperationException(Resources.NoErrorHandlerWithWhen);
            }
            else if (whenOptions is null)
            {
                whenOptions = new WhenOptions();
            }

            var taskScheduler = sharedState.GetTaskScheduler();
            var taskCompletionSource = new TaskCompletionSource<bool>();
            whenOptions.ErrorHandler = (ex) => taskCompletionSource.SetException(ex);

#pragma warning disable CA2000 // Dispose objects before losing scope
            IDisposable when = When(sharedState, predicate, () => taskCompletionSource.SetResult(true), whenOptions);
#pragma warning restore CA2000 // Dispose objects before losing scope

            var cancellationTokenRegistration = cancellationToken.Register(() =>
            {
                when.Dispose();
                taskCompletionSource.TrySetCanceled();
            });

            return taskCompletionSource.Task.ContinueWith(
                t =>
                {
                    cancellationTokenRegistration.Dispose();
                }, taskScheduler);
        }
    }
}
