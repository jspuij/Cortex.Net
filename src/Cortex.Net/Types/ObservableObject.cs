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
    public class ObservableObject : IReactiveObject
    {
        /// <summary>
        /// A set of event handlers for the change event.
        /// </summary>
        private readonly HashSet<EventHandler<ObjectCancellableEventArgs>> changeEventHandlers = new HashSet<EventHandler<ObjectCancellableEventArgs>>();

        /// <summary>
        /// A set of event handlers for the changed event.
        /// </summary>
        private readonly HashSet<EventHandler<ObjectEventArgs>> changedEventHandlers = new HashSet<EventHandler<ObjectEventArgs>>();

        /// <summary>
        /// The dictionary of <see cref="ComputedValue{T}"/> and <see cref="ObservableValue{T}"/> instances.
        /// </summary>
        private readonly IDictionary<string, IValue> values;

        /// <summary>
        /// A dictionary of keys that are probably pending for addition.
        /// </summary>
        private readonly IDictionary<string, IObservableValue<bool>> pendingKeys = new Dictionary<string, IObservableValue<bool>>();

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
        /// <param name="sharedState">The shared state for this ObservableObject.</param>
        /// <param name="name">The name of the objservable ovject.</param>
        /// <param name="defaultEnhancer">The default enhancer to use for newly created values.</param>
        /// <param name="values">A dictionary with values.</param>
        public ObservableObject(string name, IEnhancer defaultEnhancer, ISharedState sharedState, IDictionary<string, IValue> values = null)
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
        public event EventHandler<ObjectCancellableEventArgs> Change
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
        public event EventHandler<ObjectEventArgs> Changed
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
                    this.pendingKeys.Clear();
                    this.values.Clear();
                    this.changeEventHandlers.Clear();
                    this.changedEventHandlers.Clear();
                }
            }
        }

        /// <summary>
        /// Gets the IValue item at the specified key.
        /// </summary>
        /// <param name="key">The key to fetch.</param>
        /// <returns>The <see cref="IValue"/> instance for the key.</returns>
        internal IValue this[string key] => this.values[key];

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

            return ((IValue<T>)this.values[key]).Value;
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
            var changeEventArgs = new ObjectChangeEventArgs<T>()
            {
                Cancel = false,
                Context = this,
                Key = key,
                NewValue = value,
                OldValue = oldValue,
            };

            this.InterceptChange(changeEventArgs);

            if (changeEventArgs.Cancel || !changeEventArgs.Changed)
            {
                return;
            }

            (T newValue, bool changed) = observable.PrepareNewValue(value);

            if (changed)
            {
                this.SharedState.OnSpy(this, new ObservableObjectChangedStartSpyEventArgs()
                {
                    Name = this.Name,
                    Context = this,
                    Key = key,
                    OldValue = oldValue,
                    NewValue = newValue,
                    StartTime = DateTime.UtcNow,
                });

                observable.SetNewValue(newValue);

                var eventArgs = new ObjectChangedEventArgs<T>()
                {
                    Context = this,
                    Key = key,
                    OldValue = oldValue,
                    NewValue = newValue,
                };

                this.NotifyListeners(eventArgs);

                this.SharedState.OnSpy(this, new ObservableObjectChangedEndSpyEventArgs()
                {
                    Name = this.Name,
                    Context = this,
                    Key = key,
                    EndTime = DateTime.UtcNow,
                });
            }
        }

        /// <summary>
        /// Returns whether this <see cref="ObservableObject"/> instance has a property of method with the name <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>True when this object contains the key, false otherwise.</returns>
        public bool Has(string key)
        {
            if (this.pendingKeys.TryGetValue(key, out var observableValue))
            {
                return observableValue.Value;
            }

            var referenceEnhancer = this.SharedState.ReferenceEnhancer();

            var exists = this.values.ContainsKey(key);
            observableValue = new ObservableValue<bool>(this.SharedState, $"{this.Name}.{key}?", referenceEnhancer, exists);
            this.pendingKeys.Add(key, observableValue);
            return observableValue.Value; // read to subscribe
        }

        /// <summary>
        /// Adds an Observable property to this <see cref="ObservableObject"/> instnace.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="initialValue">The initial value.</param>
        /// <param name="enhancer">The enhancer to use.</param>
        public void AddObservableProperty<T>(string propertyName, T initialValue = default, IEnhancer enhancer = null)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            if (enhancer == null)
            {
                enhancer = this.defaultEnhancer;
            }

            if (this.values.ContainsKey(propertyName))
            {
                throw new ArgumentOutOfRangeException(nameof(propertyName), string.Format(CultureInfo.CurrentCulture, Resources.PropertyOrMethodAlreadyExistOnObservableObject, propertyName, this.Name));
            }

            var keyAddEventArgs = new ObjectKeyAddEventArgs<T>()
            {
                Cancel = false,
                Context = this,
                NewValue = initialValue,
                Key = propertyName,
            };

            this.InterceptChange(keyAddEventArgs);

            if (keyAddEventArgs.Cancel)
            {
                return;
            }

            var newValue = keyAddEventArgs.NewValue;
            var observableValue = new ObservableValue<T>(this.SharedState, $"{this.Name}.{propertyName}", enhancer, newValue);

            this.values.Add(propertyName, observableValue);
            newValue = observableValue.Value; // observableValue might have changed it
            this.NotifyPropertyAddition(propertyName, newValue);
        }

        /// <summary>
        /// Adds a Computed Value.
        /// </summary>
        /// <typeparam name="T">The return type of the member.</typeparam>
        /// <param name="key">The key of the member.</param>
        /// <param name="computedValueOptions">The computed value options.</param>
        public void AddComputedMember<T>(string key, ComputedValueOptions<T> computedValueOptions)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (computedValueOptions is null)
            {
                throw new ArgumentNullException(nameof(computedValueOptions));
            }

            if (this.values.ContainsKey(key))
            {
                throw new ArgumentOutOfRangeException(nameof(key), string.Format(CultureInfo.CurrentCulture, Resources.PropertyOrMethodAlreadyExistOnObservableObject, key, this.Name));
            }

            this.values.Add(key, new ComputedValue<T>(this.SharedState, computedValueOptions));
        }

        /// <summary>
        /// Removes a property or computed value.
        /// </summary>
        /// <param name="key">The key to remove.</param>
        public void Remove(string key)
        {
            if (!this.values.ContainsKey(key))
            {
                return;
            }

            var keyRemoveEventArgs = new ObjectKeyRemoveEventArgs()
            {
                Cancel = false,
                Context = this,
                Key = key,
            };

            this.InterceptChange(keyRemoveEventArgs);

            if (keyRemoveEventArgs.Cancel)
            {
                return;
            }

            try
            {
                this.SharedState.StartBatch();

                var oldObservable = this.values[key];

                var oldValue = oldObservable.Value;
                oldObservable.Value = null;

                // notify key and keyset listeners
                this.keys.ReportChanged();

                this.values.Remove(key);

                if (this.pendingKeys.TryGetValue(key, out var observablePendingKey))
                {
                    observablePendingKey.Value = false;
                }

                this.SharedState.OnSpy(this, new ObservableObjectRemovedStartSpyEventArgs()
                {
                    Name = this.Name,
                    Context = this,
                    Key = key,
                    OldValue = oldValue,
                    StartTime = DateTime.UtcNow,
                });

                var eventArgs = new ObjectKeyRemovedEventArgs()
                {
                    Context = this,
                    Key = key,
                    OldValue = oldValue,
                };

                this.NotifyListeners(eventArgs);

                this.SharedState.OnSpy(this, new ObservableObjectRemovedEndSpyEventArgs()
                {
                    Name = this.Name,
                    Context = this,
                    Key = key,
                    EndTime = DateTime.UtcNow,
                });
            }
            finally
            {
                this.SharedState.EndBatch();
          }
        }

        /// <summary>
        /// Tries to get an <see cref="ObservableObject"/> from the provided instance.
        /// </summary>
        /// <param name="instance">The instance to get the ObservableObject from.</param>
        /// <returns>The observable object found on the object. Null otherwise.</returns>
        internal static ObservableObject GetFromObject(object instance)
        {
            if (instance is ObservableObject result)
            {
                return result;
            }

            foreach (var field in instance.GetType().GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic))
            {
                if (typeof(ObservableObject).IsAssignableFrom(field.FieldType))
                {
                    return (ObservableObject)field.GetValue(instance);
                }
            }

            foreach (var property in instance.GetType().GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic))
            {
                if (typeof(ObservableObject).IsAssignableFrom(property.PropertyType))
                {
                    return (ObservableObject)property.GetValue(instance);
                }
            }

            return null;
        }

        /// <summary>
        /// Notifies listeners of property addition.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="key">The name of the property.</param>
        /// <param name="newValue">The new value.</param>
        private void NotifyPropertyAddition<T>(string key, T newValue)
        {
            this.SharedState.OnSpy(this, new ObservableObjectAddedStartSpyEventArgs()
            {
                Name = this.Name,
                Context = this,
                Key = key,
                NewValue = newValue,
                StartTime = DateTime.UtcNow,
            });

            var eventArgs = new ObjectKeyAddedEventArgs<T>()
            {
                Context = this,
                Key = key,
                NewValue = newValue,
            };

            this.NotifyListeners(eventArgs);

            this.SharedState.OnSpy(this, new ObservableObjectAddedEndSpyEventArgs()
            {
                Name = this.Name,
                Context = this,
                Key = key,
                EndTime = DateTime.UtcNow,
            });

            if (this.pendingKeys.TryGetValue(key, out var pendingKey))
            {
                pendingKey.Value = true;
            }

            this.keys.ReportChanged();
        }

        /// <summary>
        /// Fires a Change event that can be intercepted and or canceled.
        /// </summary>
        /// <param name="changeEventArgs">The change event args.</param>
        private void InterceptChange(ObjectCancellableEventArgs changeEventArgs)
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
        private void NotifyListeners(ObjectEventArgs eventArgs)
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
