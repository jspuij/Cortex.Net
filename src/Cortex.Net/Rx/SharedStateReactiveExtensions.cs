// <copyright file="SharedStateReactiveExtensions.cs" company="Michel Weststrate, Jan-Willem Spuij">
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

namespace Cortex.Net.Rx
{
    using System;
    using Cortex.Net.Api;

    /// <summary>
    /// Extension methods on ISharedState for conversion from and to
    /// System.Reactive observables.
    /// </summary>
    public static class SharedStateReactiveExtensions
    {
        /// <summary>
        /// Converts an expression to observe into an <see cref="System.IObservable{T}" />.
        /// </summary>
        /// <remarks>
        /// The provided expression is tracked by Cortex.Net as long as there are subscribers, automatically
        /// emitting when new values become available. The expressions respect(trans)actions.</remarks>
        /// <typeparam name="T">The type to observe.</typeparam>
        /// <param name="sharedState">The shared state to use.</param>
        /// <param name="expressionToObserve">The expression to observe.</param>
        /// <param name="fireImmediately">Whether to fire immediately.</param>
        /// <returns>An <see cref="System.IObservable{T}" /> instance that can be used to observe.</returns>
        public static IObservable<T> AsObservable<T>(this ISharedState sharedState, Func<T> expressionToObserve, bool fireImmediately = false)
        {
            if (sharedState is null)
            {
                throw new ArgumentNullException(nameof(sharedState));
            }

            if (expressionToObserve is null)
            {
                throw new ArgumentNullException(nameof(expressionToObserve));
            }

            var computedValue = sharedState.Computed(expressionToObserve);
            return new DelegateObservable<T>(observer =>
            {
                return computedValue.Observe(
                    (sender, args) =>
                {
                    observer.OnNext(args.NewValue);
                }, fireImmediately);
            });
        }

        /// <summary>
        ///  Converts a <see cref="System.IObservable{T}" /> into an <see cref="IObservableValue{T}"></see> which stores the current value(as `Current`).
        ///  The subscription can be canceled through the `Dispose` method. Takes an initial value as second optional argument.
        /// </summary>
        /// <typeparam name="T">The type to observe.</typeparam>
        /// <param name="sharedState">The shared state to use.</param>
        /// <param name="observable">The observable to "convert".</param>
        /// <param name="initialValue">The initial value for the <see cref="IObservableValue{T}" />.</param>
        /// <param name="exceptionHandler">The exception handler to use.</param>
        /// <returns>An observable value.</returns>
        public static RxObserver<T> FromObservable<T>(this ISharedState sharedState, IObservable<T> observable, T initialValue = default, Action<Exception> exceptionHandler = null)
        {
            if (sharedState is null)
            {
                throw new ArgumentNullException(nameof(sharedState));
            }

            if (observable is null)
            {
                throw new ArgumentNullException(nameof(observable));
            }

            return new RxObserver<T>(sharedState, observable, initialValue, exceptionHandler);
        }
    }
}
