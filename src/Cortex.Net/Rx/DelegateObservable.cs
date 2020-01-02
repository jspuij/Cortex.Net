// <copyright file="DelegateObservable.cs" company="Michel Weststrate, Jan-Willem Spuij">
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

    /// <summary>
    /// An observable object that uses a delegate as a subscribe function.
    /// </summary>
    /// <typeparam name="T">The type of the values to observe.</typeparam>
    internal class DelegateObservable<T> : IObservable<T>
    {
        private readonly Func<IObserver<T>, IDisposable> subscribeDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateObservable{T}"/> class.
        /// </summary>
        /// <param name="subscribeDelegate">The delegate to be used when an observer subscribes.</param>
        public DelegateObservable(Func<IObserver<T>, IDisposable> subscribeDelegate)
        {
            this.subscribeDelegate = subscribeDelegate ?? throw new ArgumentNullException(nameof(subscribeDelegate));
        }

        /// <summary>
        /// Notifies the provider that an observer is to receive notifications.
        /// </summary>
        /// <param name="observer">The object that is to receive notifications.</param>
        /// <returns>
        /// A reference to an interface that allows observers to stop receiving notifications
        /// before the provider has finished sending them.
        /// </returns>
        public IDisposable Subscribe(IObserver<T> observer)
        {
            return this.subscribeDelegate(observer);
        }
    }
}
