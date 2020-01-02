// <copyright file="RxObserver.cs" company="Michel Weststrate, Jan-Willem Spuij">
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
    using System.Collections.Generic;
    using System.Text;
    using Cortex.Net.Api;
    using Cortex.Net.Core;
    using Cortex.Net.Types;

    /// <summary>
    /// An RxObserver class that observes an System.Reactive observable and
    /// subsequently triggers an <see cref="IObservableValue{T}" /> instance.
    /// </summary>
    /// <typeparam name="T">The type of the observable.</typeparam>
    public sealed class RxObserver<T> : IObserver<T>, IDisposable, IObservableValue<T>
    {
        private readonly ISharedState sharedState;
        private readonly Action<Exception> exceptionHandler;
        private IObservableValue<T> observableValue;
        private IDisposable subscription;

        /// <summary>
        /// Initializes a new instance of the <see cref="RxObserver{T}"/> class.
        /// </summary>
        /// <param name="sharedState">The shared state.</param>
        /// <param name="observable">The observable.</param>
        /// <param name="exceptionHandler">The exception handler.</param>
        /// <param name="initialValue">The initial value.</param>
        internal RxObserver(ISharedState sharedState, IObservable<T> observable, T initialValue, Action<Exception> exceptionHandler)
        {
            if (observable is null)
            {
                throw new ArgumentNullException(nameof(observable));
            }

            this.sharedState = sharedState ?? throw new ArgumentNullException(nameof(sharedState));
            this.exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
            this.sharedState.RunInAction(() =>
            {
                this.observableValue = new ObservableValue<T>(this.sharedState, $"RxObserver<{typeof(T)}>", this.sharedState.ReferenceEnhancer());
                this.observableValue.Value = initialValue;
                observable.Subscribe(this);
            });
        }

        /// <summary>
        /// Event that fires before the value will change.
        /// </summary>
        public event EventHandler<ValueChangeEventArgs<T>> Change
        {
            add
            {
                this.observableValue.Change += value;
            }

            remove
            {
                this.observableValue.Change -= value;
            }
        }

        /// <summary>
        /// Event that fires after the value has changed.
        /// </summary>
        public event EventHandler<ValueChangedEventArgs<T>> Changed
        {
            add
            {
                this.observableValue.Changed += value;
            }

            remove
            {
                this.observableValue.Changed -= value;
            }
        }

        /// <summary>
        /// Gets or sets the underlying value.
        /// </summary>
        public T Value { get => this.observableValue.Value; set => this.observableValue.Value = value; }

        /// <summary>
        /// Gets or sets the underlying value.
        /// </summary>
        object IValue.Value { get => this.observableValue.Value; set => this.observableValue.Value = (T)value; }

        /// <summary>
        /// Cleans up the subscription when the observer is disposed.
        /// </summary>
        public void Dispose()
        {
            if (this.subscription != null)
            {
                this.subscription.Dispose();
                this.subscription = null;
            }
        }

        /// <summary>
        /// Registers the secified event handler, and optionally fires it first.
        /// </summary>
        /// <param name="changedEventHandler">The event handler to register.</param>
        /// <param name="fireImmediately">Whether to fire the event handler immediately.</param>
        public void Observe(EventHandler<ValueChangedEventArgs<T>> changedEventHandler, bool fireImmediately)
        {
            this.observableValue.Observe(changedEventHandler, fireImmediately);
        }

        /// <summary>
        /// Notifies the observer that the provider has finished sending push-based notifications.
        /// </summary>
        public void OnCompleted()
        {
            this.sharedState.RunInAction(() =>
            {
                this.Dispose();
            });
        }

        /// <summary>
        /// Notifies the observer that the provider has experienced an error condition.
        /// </summary>
        /// <param name="error">An object that provides additional information about the error.</param>
        public void OnError(Exception error)
        {
            this.sharedState.RunInAction(() =>
            {
                this.exceptionHandler?.Invoke(error);
                this.Dispose();
            });
        }

        /// <summary>
        /// Provides the observer with new data.
        /// </summary>
        /// <param name="value">The current notification information.</param>
        public void OnNext(T value)
        {
            this.sharedState.RunInAction(() =>
            {
                this.observableValue.Value = value;
            });
        }
    }
}
