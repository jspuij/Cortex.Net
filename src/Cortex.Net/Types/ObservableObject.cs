// <copyright file="ObservableObject.cs" company="Michel Weststrate, Jan-Willem Spuij">
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
    using System.Globalization;
    using System.Text;
    using Cortex.Net.Core;
    using Cortex.Net.Properties;
    using Cortex.Net.Spy;

    /// <summary>
    /// Base or inner class for observable objects.
    /// </summary>
    internal class ObservableObject : IObservableObject
    {
        /// <summary>
        /// A set of event handlers for the change event.
        /// </summary>
        private readonly HashSet<EventHandler<ObjectChangeEventArgs>> changeEventHandlers = new HashSet<EventHandler<ObjectChangeEventArgs>>();

        /// <summary>
        /// A set of event handlers for the changed event.
        /// </summary>
        private readonly HashSet<EventHandler<ObjectChangedEventArgs>> changedEventHandlers = new HashSet<EventHandler<ObjectChangedEventArgs>>();

        /// <summary>
        /// The dictionary of <see cref="ComputedValue{T}"/> and <see cref="ObservableValue{T}"/> instances.
        /// </summary>
        private readonly IDictionary<string, IValue> values;

        /// <summary>
        /// The default enhancer that possibly makes new values observable as well.
        /// </summary>
        private readonly IEnhancer defaultEnhancer;

        /// <summary>
        /// An atom for managing addition or removal of property / method keys.
        /// </summary>
        private IAtom keys;

        /// <summary>
        /// A reference to the shared state.
        /// </summary>
        private ISharedState sharedState;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableObject"/> class.
        /// </summary>
        /// <param name="name">The name of the objservable ovject.</param>
        /// <param name="defaultEnhancer">The default enhancer to use for newly created values.</param>
        /// <param name="sharedState">The shared state for this ObservableObject.</param>
        /// <param name="values">A dictionary with values.</param>
        public ObservableObject(string name, IEnhancer defaultEnhancer, ISharedState sharedState = null, IDictionary<string, IValue> values = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (values is null)
            {
               this.values = new Dictionary<string, IValue>();
            }
            else
            {
                this.values = values;
            }

            this.Name = name;
            this.defaultEnhancer = defaultEnhancer ?? throw new ArgumentNullException(nameof(defaultEnhancer));
            this.SharedState = sharedState;
        }

        /// <summary>
        /// Event that fires before a value on the object will change.
        /// </summary>
        public event EventHandler<ObjectChangeEventArgs> Change
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
        /// Event that fires after a value on the object has changed.
        /// </summary>
        public event EventHandler<ObjectChangedEventArgs> Changed
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
        /// Gets the name of this ObservableObject.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets or sets the Shared State on this object.
        /// </summary>
        public ISharedState SharedState
        {
            get => this.sharedState;

            set
            {
                this.sharedState = value;
                if (this.sharedState != null)
                {
                    this.keys = new Atom(this.sharedState, $"{this.Name}.keys");
                }
            }
        }

        /// <summary>
        /// Gets the value for property or method with specified key.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="key">The key.</param>
        /// <returns>The value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the key does not exist in the values.</exception>
        public T Read<T>(string key)
        {
            if (!this.values.ContainsKey(key))
            {
                throw new ArgumentOutOfRangeException(nameof(key), string.Format(CultureInfo.CurrentCulture, Resources.PropertyOrMethodNotFoundOnObservableObject, key, this.Name));
            }

            return ((IValue<T>)this.values).Value;
        }

        /// <summary>
        /// Sets the value for property or method with the specified value.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the key does not exist in the values.</exception>
        public void Write<T>(string key, T value)
        {
            if (!this.values.ContainsKey(key))
            {
                throw new ArgumentOutOfRangeException(nameof(key), string.Format(CultureInfo.CurrentCulture, Resources.PropertyOrMethodNotFoundOnObservableObject, key, this.Name));
            }

            var ivalue = this.values[key];

            if (ivalue is IComputedValue<T>)
            {
                var computedValue = ivalue as IComputedValue<T>;
                computedValue.Value = value;
                return;
            }

            var observable = ivalue as ObservableValue<T>;
            T oldValue = observable.UntrackedValue;
            var changeEventArgs = new ObjectChangeEventArgs()
            {
                Cancel = false,
                Context = this,
                NewValue = value,
                OldValue = oldValue,
            };

            foreach (var handler in this.changeEventHandlers)
            {
                handler(this, changeEventArgs);
            }

            if (changeEventArgs.Cancel || !changeEventArgs.Changed)
            {
                return;
            }

            (T newValue, bool changed) = observable.PrepareNewValue(value);

            if (changed)
            {
                this.SharedState.OnSpy(this, new ObservableObjectStartEventArgs()
                {
                    Name = this.Name,
                    Context = this,
                    Key = key,
                    OldValue = oldValue,
                    NewValue = newValue,
                    StartTime = DateTime.UtcNow,
                });

                observable.SetNewValue(newValue);

                var eventArgs = new ObjectChangedEventArgs()
                {
                    Context = this,
                    Key = key,
                    OldValue = oldValue,
                    NewValue = newValue,
                };

                foreach (var handler in this.changedEventHandlers)
                {
                    handler(this, eventArgs);
                }

                this.SharedState.OnSpy(this, new ObservableObjectEndEventArgs()
                {
                    Name = this.Name,
                    Context = this,
                    Key = key,
                    EndTime = DateTime.UtcNow,
                });
            }
        }
    }
}
