// <copyright file="ObservableValue.cs" company="Michel Weststrate, Jan-Willem Spuij">
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

namespace Cortex.Net.Types
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Cortex.Net.Core;
    using Cortex.Net.Spy;

    /// <summary>
    /// Observable value. Can be changed and can be observed.
    /// </summary>
    /// <typeparam name="T">The type of the Observable value.</typeparam>
    public class ObservableValue<T> :
        Atom, IObservableValue<T>
    {
        /// <summary>
        /// A set of event handlers for the change event.
        /// </summary>
        private readonly HashSet<EventHandler<ValueChangeEventArgs<T>>> changeEventHandlers = new HashSet<EventHandler<ValueChangeEventArgs<T>>>();

        /// <summary>
        /// A set of event handlers for the changed event.
        /// </summary>
        private readonly HashSet<EventHandler<ValueChangedEventArgs<T>>> changedEventHandlers = new HashSet<EventHandler<ValueChangedEventArgs<T>>>();

        /// <summary>
        /// An enhancer that possibly makes new values observable as well.
        /// </summary>
        private readonly IEnhancer enhancer;

        /// <summary>
        /// The value of the observable.
        /// </summary>
        private T value;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableValue{T}"/> class.
        /// </summary>
        /// <param name="sharedState">The shared state this Observable will be attached to.</param>
        /// <param name="name">The name of this observable value.</param>
        /// <param name="enhancer">The enhancer to use on the type.</param>
        /// <param name="value">The initial value of the Observable.</param>
        public ObservableValue(ISharedState sharedState, string name, IEnhancer enhancer, T value = default)
            : base(sharedState, name)
        {
            this.enhancer = enhancer ?? throw new ArgumentNullException(nameof(enhancer));
            this.value = this.enhancer.Enhance(value, default, this.Name);
            this.SharedState.OnSpy(this, new ObservableValueCreateSpyEventArgs() { Name = this.Name, NewValue = value });
        }

        /// <summary>
        /// Event that fires before the value will change.
        /// </summary>
        public event EventHandler<ValueChangeEventArgs<T>> Change
        {
            add
            {
                this.changeEventHandlers.Add(value);
            }

            remove
            {
                this.changeEventHandlers.Remove(value);
            }
        }

        /// <summary>
        /// Event that fires after the value has changed.
        /// </summary>
        public event EventHandler<ValueChangedEventArgs<T>> Changed
        {
            add
            {
                this.changedEventHandlers.Add(value);
            }

            remove
            {
                this.changedEventHandlers.Remove(value);
            }
        }

        /// <summary>
        /// Gets or sets the underlying value.
        /// </summary>
        public T Value
        {
            get
            {
                this.ReportObserved();
                return this.value;
            }

            set
            {
                T oldValue = this.value;

                (T newValue, bool changed) = this.PrepareNewValue(value);

                if (changed)
                {
                    this.SharedState.OnSpy(this, new ObservableValueStartSpyEventArgs()
                    {
                        Name = this.Name,
                        OldValue = oldValue,
                        NewValue = newValue,
                        StartTime = DateTime.UtcNow,
                    });

                    this.SetNewValue(newValue);

                    this.SharedState.OnSpy(this, new ObservableValueEndSpyEventArgs()
                    {
                        Name = this.Name,
                        EndTime = DateTime.UtcNow,
                    });
                }
            }
        }

        /// <summary>
        /// Gets or sets the underlying value.
        /// </summary>
        object IValue.Value
        {
            get => this.Value;

            set
            {
                if (value is null)
                {
                    this.Value = default;
                }
                else
                {
                    this.Value = (T)value;
                }
            }
        }

        /// <summary>
        /// Gets the untracked value of this observable.
        /// </summary>
        internal T UntrackedValue
        {
            get
            {
                return this.value;
            }
        }

        /// <summary>
        /// Registers the secified event handler, and optionally fires it first.
        /// </summary>
        /// <param name="changedEventHandler">The event handler to register.</param>
        /// <param name="fireImmediately">Whether to fire the event handler immediately.</param>
        public void Observe(EventHandler<ValueChangedEventArgs<T>> changedEventHandler, bool fireImmediately)
        {
            if (changedEventHandler is null)
            {
                throw new ArgumentNullException(nameof(changedEventHandler));
            }

            if (fireImmediately)
            {
                changedEventHandler(this, new ValueChangedEventArgs<T>()
                {
                    Context = this,
                    OldValue = default,
                    NewValue = this.Value,
                });
            }

            this.Changed += changedEventHandler;
        }

        /// <summary>
        /// Prepares setting of a new Value.
        /// </summary>
        /// <param name="newValue">The new value.</param>
        /// <returns>The value plus a boolean indicating whether the observable value has changed.</returns>
        internal (T, bool hasChanged) PrepareNewValue(T newValue)
        {
            this.CheckIfStateModificationsAreAllowed();

            var changeEventArgs = new ValueChangeEventArgs<T>()
            {
                Cancel = false,
                Context = this,
                NewValue = newValue,
                OldValue = this.value,
            };

            this.InterceptChange(changeEventArgs);

            if (changeEventArgs.Cancel || !changeEventArgs.Changed)
            {
                return (this.value, true);
            }

            newValue = this.enhancer.Enhance(changeEventArgs.NewValue, this.value, this.Name);

            return (newValue, !Equals(this.value, newValue));
        }

        /// <summary>
        /// Sets a new value.
        /// </summary>
        /// <param name="newValue">The new value to set.</param>
        internal void SetNewValue(T newValue)
        {
            var oldValue = this.value;
            this.value = newValue;
            this.ReportChanged();

            var eventArgs = new ValueChangedEventArgs<T>()
            {
                Context = this,
                OldValue = oldValue,
                NewValue = newValue,
            };

            this.NotifyListeners(eventArgs);
        }

        /// <summary>
        /// Fires a Change event that can be intercepted and or canceled.
        /// </summary>
        /// <param name="changeEventArgs">The change event args.</param>
        private void InterceptChange(ValueChangeEventArgs<T> changeEventArgs)
        {
            var previousDerivation = this.SharedState.StartUntracked();
            try
            {
                foreach (var handler in this.changeEventHandlers)
                {
                    handler(this, changeEventArgs);
                    if (changeEventArgs.Cancel)
                    {
                        break;
                    }
                }
            }
            finally
            {
                this.SharedState.EndTracking(previousDerivation);
            }
        }

        /// <summary>
        /// Notifies Listeners on the <see cref="Changed"/> event.
        /// </summary>
        /// <param name="eventArgs">The event arguments.</param>
        private void NotifyListeners(ValueChangedEventArgs<T> eventArgs)
        {
            var previousDerivation = this.SharedState.StartUntracked();
            try
            {
                foreach (var handler in this.changedEventHandlers)
                {
                    handler(this, eventArgs);
                }
            }
            finally
            {
                this.SharedState.EndTracking(previousDerivation);
            }
        }
    }
}
