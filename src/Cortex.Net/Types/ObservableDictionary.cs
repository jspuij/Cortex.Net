// <copyright file="ObservableDictionary.cs" company="Michel Weststrate, Jan-Willem Spuij">
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
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using Cortex.Net.Api;
    using Cortex.Net.Core;
    using Cortex.Net.Properties;
    using Cortex.Net.Spy;

    /// <summary>
    /// Implements an Observable dictionary of key value pairs..
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    public class ObservableDictionary<TKey, TValue> :
        ICollection<KeyValuePair<TKey, TValue>>,
        IEnumerable<KeyValuePair<TKey, TValue>>,
        IEnumerable,
        IDictionary<TKey, TValue>,
        IReadOnlyCollection<KeyValuePair<TKey, TValue>>,
        IReadOnlyDictionary<TKey, TValue>,
        ICollection,
        IDictionary
    {
        /// <summary>
        /// The inner dictionary to store the keys and values.
        /// </summary>
        private readonly Dictionary<TKey, TValue> innerDictionary;

        /// <summary>
        /// Dictionary with observables that makes Contains and ContaisKey methods reactive.
        /// </summary>
        private readonly Dictionary<TKey, ObservableValue<bool>> hasDictionary;

        /// <summary>
        /// Atom to signal that the dictionary has changed.
        /// </summary>
        private readonly IAtom atom;

        /// <summary>
        /// The enhancer for this dictionary.
        /// </summary>
        private readonly IEnhancer enhancer;

        /// <summary>
        /// A set of event handlers for the change event.
        /// </summary>
        private readonly HashSet<EventHandler<ObservableDictionaryCancellableEventArgs>> changeEventHandlers = new HashSet<EventHandler<ObservableDictionaryCancellableEventArgs>>();

        /// <summary>
        /// A set of event handlers for the changed event.
        /// </summary>
        private readonly HashSet<EventHandler<ObservableDictionaryEventArgs>> changedEventHandlers = new HashSet<EventHandler<ObservableDictionaryEventArgs>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableDictionary{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="sharedState">The <see cref="ISharedState"/> instance this observableDictionary belongs to.</param>
        /// <param name="enhancer">The <see cref="IEnhancer"/> implementation to use.</param>
        /// <param name="name">The name of the ObservableDictionary.</param>
        public ObservableDictionary(ISharedState sharedState, IEnhancer enhancer, string name = null)
        {
            this.SharedState = sharedState ?? throw new ArgumentNullException(nameof(sharedState));
            this.enhancer = enhancer ?? throw new ArgumentNullException(nameof(enhancer));

            if (string.IsNullOrEmpty(name))
            {
                name = $"{nameof(ObservableDictionary<TKey, TValue>)}@{this.SharedState.GetUniqueId()}";
            }

            this.Name = name;
            this.innerDictionary = new Dictionary<TKey, TValue>();
            this.hasDictionary = new Dictionary<TKey, ObservableValue<bool>>();

            this.atom = new Atom(sharedState, name);
        }

        /// <summary>
        /// Event that fires before the value will change.
        /// </summary>
        public event EventHandler<ObservableDictionaryCancellableEventArgs> Change
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
        public event EventHandler<ObservableDictionaryEventArgs> Changed
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
        /// Gets the shared state this <see cref="ObservableDictionary{TKey, TValue}"/> operates on.
        /// </summary>
        public ISharedState SharedState { get; }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="ObservableDictionary{TKey, TValue}"/>.
        /// </summary>
        public int Count
        {
            get
            {
                this.atom.ReportObserved();
                return this.innerDictionary.Count;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="ObservableDictionary{TKey, TValue}"/> is read-only.
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        ///  Gets a value indicating whether access to the <see cref="ObservableDictionary{TKey, TValue}"/> is synchronized (thread safe).
        /// </summary>
        public bool IsSynchronized => false;

        /// <summary>
        /// Gets an object that can be used to synchronize access to the <see cref="ObservableDictionary{TKey, TValue}"/>.
        /// </summary>
        public object SyncRoot { get; } = new object();

        /// <summary>
        /// Gets a value indicating whether the <see cref="ObservableDictionary{TKey, TValue}"/> has a fixed size.
        /// </summary>
        public bool IsFixedSize => false;

        /// <summary>
        /// Gets the name of the <see cref="ObservableDictionary{TKey, TValue}"/>.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets a collection containing the keys in the <see cref="ObservableDictionary{TKey, TValue}"/>.
        /// </summary>
        public ICollection<TKey> Keys
        {
            get
            {
                this.atom.ReportObserved();
                return this.innerDictionary.Keys;
            }
        }

        /// <summary>
        /// Gets a collection containing the values in the <see cref="ObservableDictionary{TKey, TValue}"/>.
        /// </summary>
        public ICollection<TValue> Values
        {
            get
            {
                this.atom.ReportObserved();
                return this.innerDictionary.Values;
            }
        }

        /// <summary>
        /// Gets a readonly enumeration containing the keys in the <see cref="ObservableDictionary{TKey, TValue}"/>.
        /// </summary>
        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => this.Keys;

        /// <summary>
        /// Gets a readonly enumeration containing the values in the <see cref="ObservableDictionary{TKey, TValue}"/>.
        /// </summary>
        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => this.Values;

        /// <summary>
        /// Gets the number of elements contained in the <see cref="ObservableDictionary{TKey, TValue}"/>.
        /// </summary>
        int ICollection.Count => this.Count;

        /// <summary>
        ///  Gets a value indicating whether access to the <see cref="ObservableDictionary{TKey, TValue}"/> is synchronized (thread safe).
        /// </summary>
        bool ICollection.IsSynchronized => this.IsSynchronized;

        /// <summary>
        /// Gets an object that can be used to synchronize access to the <see cref="ObservableDictionary{TKey, TValue}"/>.
        /// </summary>
        object ICollection.SyncRoot => this.SyncRoot;

        /// <summary>
        /// Gets a value indicating whether the <see cref="ObservableDictionary{TKey, TValue}"/> has a fixed size.
        /// </summary>
        bool IDictionary.IsFixedSize => this.IsFixedSize;

        /// <summary>
        /// Gets a value indicating whether the <see cref="ObservableDictionary{TKey, TValue}"/> is read-only.
        /// </summary>
        bool IDictionary.IsReadOnly => this.IsReadOnly;

        /// <summary>
        /// Gets a collection containing the keys in the <see cref="ObservableDictionary{TKey, TValue}"/>.
        /// </summary>
        ICollection IDictionary.Keys => (ICollection)this.Keys;

        /// <summary>
        /// Gets a collection containing the values in the <see cref="ObservableDictionary{TKey, TValue}"/>.
        /// </summary>
        ICollection IDictionary.Values => (ICollection)this.Values;

        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get or set.</param>
        /// <returns>
        /// The value associated with the specified key. If the specified key is not found,
        /// a get operation throws a System.Collections.Generic.KeyNotFoundException, and
        /// a set operation creates a new element with the specified key.
        /// </returns>
        object IDictionary.this[object key]
        {
            get => this[(TKey)key];
            set => this[(TKey)key] = (TValue)value;
        }

        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get or set.</param>
        /// <returns>
        /// The value associated with the specified key. If the specified key is not found,
        /// a get operation throws a System.Collections.Generic.KeyNotFoundException, and
        /// a set operation creates a new element with the specified key.
        /// </returns>
        public TValue this[TKey key]
        {
            get
            {
                if (this.ContainsKey(key))
                {
                    return this.innerDictionary[key];
                }
                else
                {
                    // this will throw.
                    return this.innerDictionary[key];
                }
            }

            set
            {
                bool hasKey = this.innerDictionary.ContainsKey(key);

                if (!this.IsReadOnly)
                {
                    TValue newValue = value;

                    if (hasKey)
                    {
                        this.UpdateValue(key, newValue);
                    }
                    else
                    {
                        this.Add(key, value);
                    }
                }
                else
                {
                    // this will trap
                    this.innerDictionary[key] = value;
                }
            }
        }

        /// <summary>
        ///  Removes all elements from the <see cref="ObservableDictionary{TKey, TValue}"/>.
        /// </summary>
        public void Clear()
        {
            this.SharedState.Transaction(() =>
            {
                this.SharedState.Untracked(() =>
                {
                    foreach (var key in this.innerDictionary.Keys.ToList())
                    {
                        this.Remove(key);
                    }
                });
            });
        }

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="ObservableDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the <see cref="ObservableDictionary{TKey, TValue}"/>.</returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            this.atom.ReportObserved();
            return this.innerDictionary.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="ObservableDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the <see cref="ObservableDictionary{TKey, TValue}"/>.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            this.atom.ReportObserved();
            return this.innerDictionary.GetEnumerator();
        }

        /// <summary>
        /// Adds the specified <see cref="KeyValuePair{TKey, TValue}"/> to the dictionary.
        /// </summary>
        /// <param name="item">The pair to add.</param>
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            this.Add(item.Key, item.Value);
        }

        /// <summary>
        /// Determines whether the System.Collections.Generic.Dictionary`2 contains the specified key value pair.
        /// </summary>
        /// <param name="item">The <see cref="KeyValuePair{TKey, TValue}"/> to locate in the System.Collections.Generic.Dictionary`2.</param>
        /// <returns>true if the System.Collections.Generic.Dictionary`2 contains the key value pair; otherwise, false.</returns>
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            if (!this.ContainsKey(item.Key))
            {
                return false;
            }

            return Equals(this.innerDictionary[item.Key], item.Value);
        }

        /// <summary>
        /// Copies the elements of the <see cref="ObservableDictionary{TKey, TValue}"/> to an System.Array,
        /// starting at a particular System.Array index.
        /// </summary>
        /// <param name="array">The one-dimensional System.Array that is the destination of the elements copied
        /// from <see cref="ObservableDictionary{TKey, TValue}"/>. The System.Array must have zero-based indexing.
        /// </param>
        /// <param name="index">The zero-based index in array at which copying begins.</param>
        /// <exception cref="ArgumentNullException">array is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">arrayIndex is less than 0.</exception>
        /// <exception cref="ArgumentException">
        /// The number of elements in the source System.Dictionarys.Generic.IDictionary`1
        /// is greater than the available space from arrayIndex to the end of the destination array.
        /// </exception>
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
        {
           ((ICollection<KeyValuePair<TKey, TValue>>)this.innerDictionary).CopyTo(array, index);
        }

        /// <summary>
        /// Removes the value with the specified <see cref="KeyValuePair{TKey, TValue}"/> from the System.Collections.Generic.Dictionary`2.
        /// </summary>
        /// <param name="item">The <see cref="KeyValuePair{TKey, TValue}"/> of the element to remove.</param>
        /// <returns>
        /// true if the element is successfully found and removed; otherwise, false. This
        /// method returns false if key is not found in the System.Collections.Generic.Dictionary`2.
        /// </returns>
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (this.innerDictionary.ContainsKey(item.Key) && Equals(this.innerDictionary[item.Key], item.Value))
            {
                return this.Remove(item.Key);
            }

            return false;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="ObservableDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the <see cref="ObservableDictionary{TKey, TValue}"/>.</returns>
        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Adds the specified key and value to the dictionary.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add. The value can be null for reference types.</param>
        public void Add(TKey key, TValue value)
        {
            if (this.innerDictionary.ContainsKey(key))
            {
                // this will throw
                this.innerDictionary.Add(key, value);
            }
            else
            {
                this.atom.CheckIfStateModificationsAreAllowed();

                var addEventArgs = new ObservableDictionaryAddEventArgs<TKey, TValue>(key)
                {
                    Cancel = false,
                    Context = this,
                    AddedValue = value,
                };

                this.InterceptChange(addEventArgs);

                if (addEventArgs.Cancel)
                {
                    return;
                }

                var newValue = this.enhancer.Enhance(addEventArgs.AddedValue, default, this.Name);

                if (newValue is IReactiveObject reactiveObject)
                {
                    if (reactiveObject.SharedState != this.SharedState)
                    {
                        throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.DifferentSharedStates, this.Name));
                    }
                }

                this.SharedState.Transaction(() =>
                {
                    this.atom.ReportChanged();
                    var referenceEnhancer = this.SharedState.ReferenceEnhancer();

                    if (!this.hasDictionary.ContainsKey(key))
                    {
                        // todo: replace with atom (breaking change)
                        var newEntry = new ObservableValue<bool>(
                            this.SharedState,
                            $"{this.Name}.{key.ToString()}",
                            referenceEnhancer,
                            true);

                        this.hasDictionary[key] = newEntry;

                        newEntry.BecomeUnobserved += (s, e) => this.hasDictionary.Remove(key);
                    }

                    this.hasDictionary[key].Value = true;
                    this.innerDictionary.Add(key, newValue);

                    this.NotifyDictionaryChildAdd(key, newValue);
                });
            }
        }

        /// <summary>
        /// Determines whether the System.Collections.Generic.Dictionary`2 contains the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the System.Collections.Generic.Dictionary`2.</param>
        /// <returns>true if the System.Collections.Generic.Dictionary`2 contains an element with the specified key; otherwise, false.</returns>
        public bool ContainsKey(TKey key)
        {
            if (this.SharedState.TrackingDerivation != null)
            {
                return this.innerDictionary.ContainsKey(key);
            }

            if (!this.hasDictionary.TryGetValue(key, out var entry))
            {
                var referenceEnhancer = this.SharedState.ReferenceEnhancer();

                // todo: replace with atom (breaking change)
                var newEntry = entry = new ObservableValue<bool>(
                    this.SharedState,
                    $"{this.Name}.{key.ToString()}",
                    referenceEnhancer,
                    this.innerDictionary.ContainsKey(key));
                this.hasDictionary[key] = newEntry;

                newEntry.BecomeUnobserved += (s, e) => this.hasDictionary.Remove(key);
            }

            return entry.Value;
        }

        /// <summary>
        /// Removes the value with the specified key from the System.Collections.Generic.Dictionary`2.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns>
        /// true if the element is successfully found and removed; otherwise, false. This
        /// method returns false if key is not found in the System.Collections.Generic.Dictionary`2.
        /// </returns>
        public bool Remove(TKey key)
        {
            if (!this.IsReadOnly && this.innerDictionary.ContainsKey(key))
            {
                this.atom.CheckIfStateModificationsAreAllowed();

                var oldValue = this.innerDictionary[key];

                var removeEventArgs = new ObservableDictionaryRemoveEventArgs<TKey, TValue>(key)
                {
                    Cancel = false,
                    Context = this,
                    RemovedValue = oldValue,
                };

                this.InterceptChange(removeEventArgs);

                if (removeEventArgs.Cancel)
                {
                    return false;
                }

                this.SharedState.Transaction(() =>
                {
                    this.atom.ReportChanged();
                    this.hasDictionary[key].Value = false;
                    this.innerDictionary.Remove(key);
                    this.NotifyDictionaryChildRemove(key, oldValue);
                });

                return true;
            }
            else
            {
                // this will trap or return false.
                return this.innerDictionary.Remove(key);
            }
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="value">When this method returns, the value associated with the specified key, if the
        /// key is found; otherwise, the default value for the type of the value parameter.
        /// This parameter is passed uninitialized.</param>
        /// <returns>true if the object that implements System.Collections.Generic.IDictionary`2 contains
        /// an element with the specified key; otherwise, false.</returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            if (this.ContainsKey(key))
            {
                value = this.innerDictionary[key];
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Copies the elements of the System.Collections.ICollection to an System.Array,
        /// starting at a particular System.Array index.
        /// </summary>
        /// <param name="array">The one-dimensional System.Array that is the destination of the elements copied
        /// from System.Collections.ICollection. The System.Array must have zero-based indexing.</param>
        /// <param name="index">The zero-based index in array at which copying begins.</param>
        void ICollection.CopyTo(Array array, int index)
        {
            ((ICollection)this.innerDictionary).CopyTo(array, index);
        }

        /// <summary>
        /// Adds the specified key and value to the dictionary.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add. The value can be null for reference types.</param>
        void IDictionary.Add(object key, object value)
        {
            this.Add((TKey)key, (TValue)value);
        }

        /// <summary>
        ///  Removes all elements from the <see cref="ObservableDictionary{TKey, TValue}"/>.
        /// </summary>
        void IDictionary.Clear()
        {
            this.Clear();
        }

        /// <summary>
        /// Determines whether the System.Collections.Generic.Dictionary`2 contains the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the System.Collections.Generic.Dictionary`2.</param>
        /// <returns>true if the System.Collections.Generic.Dictionary`2 contains an element with the specified key; otherwise, false.</returns>
        bool IDictionary.Contains(object key)
        {
            return this.ContainsKey((TKey)key);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="ObservableDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the <see cref="ObservableDictionary{TKey, TValue}"/>.</returns>
        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            this.atom.ReportObserved();
            return ((IDictionary)this.innerDictionary).GetEnumerator();
        }

        /// <summary>
        /// Removes the value with the specified key from the System.Collections.Generic.Dictionary`2.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        void IDictionary.Remove(object key)
        {
            this.Remove((TKey)key);
        }

        /// <summary>
        /// Updates a value in the dictionary.
        /// </summary>
        /// <param name="key">The key for the item to update.</param>
        /// <param name="newValue">The value for the item to update.</param>
        private void UpdateValue(TKey key, TValue newValue)
        {
            this.atom.CheckIfStateModificationsAreAllowed();

            var oldValue = this.innerDictionary[key];

            var addEventArgs = new ObservableDictionaryChangeEventArgs<TKey, TValue>(key)
            {
                Cancel = false,
                Context = this,
                OldValue = oldValue,
                NewValue = newValue,
            };

            this.InterceptChange(addEventArgs);

            if (addEventArgs.Cancel)
            {
                return;
            }

            newValue = this.enhancer.Enhance(addEventArgs.NewValue, oldValue, this.Name);

            if (newValue is IReactiveObject reactiveObject)
            {
                if (reactiveObject.SharedState != this.SharedState)
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.DifferentSharedStates, this.Name));
                }
            }

            this.innerDictionary.Add(key, newValue);
            this.NotifyDictionaryChildUpdate(key, newValue, oldValue);
        }

        /// <summary>
        /// Fires a Change event that can be intercepted and or canceled.
        /// </summary>
        /// <param name="changeEventArgs">The change event args.</param>
        private void InterceptChange(ObservableDictionaryCancellableEventArgs changeEventArgs)
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
        /// Notify spy and listeners of the update.
        /// </summary>
        /// <param name="key">The key of the item that got updated.</param>
        /// <param name="newValue">The new value.</param>
        /// <param name="oldValue">The old value.</param>
        private void NotifyDictionaryChildUpdate(TKey key, TValue newValue, TValue oldValue)
        {
            this.SharedState.OnSpy(this, new ObservableDictionaryChangedStartSpyEventArgs(key)
            {
                Name = this.Name,
                Context = this,
                OldValue = oldValue,
                NewValue = newValue,
                StartTime = DateTime.UtcNow,
            });

            var eventArgs = new ObservableDictionaryChangedEventArgs<TKey, TValue>(key)
            {
                Context = this,
                OldValue = oldValue,
                NewValue = newValue,
            };

            this.atom.ReportChanged();
            this.NotifyListeners(eventArgs);

            this.SharedState.OnSpy(this, new ObservableDictionaryChangedEndSpyEventArgs()
            {
                Name = this.Name,
                Context = this,
                EndTime = DateTime.UtcNow,
            });
        }

        /// <summary>
        ///  Notify spy and listeners of the add.
        /// </summary>
        /// <param name="key">The key to add.</param>
        /// <param name="value">The value to add.</param>
        private void NotifyDictionaryChildAdd(TKey key, TValue value)
        {
            this.SharedState.OnSpy(this, new ObservableDictionaryAddedStartSpyEventArgs(key)
            {
                Name = this.Name,
                Context = this,
                Value = value,
                StartTime = DateTime.UtcNow,
            });

            var eventArgs = new ObservableDictionaryAddedEventArgs<TKey, TValue>(key)
            {
                Context = this,
                AddedValue = value,
            };

            this.NotifyListeners(eventArgs);

            this.SharedState.OnSpy(this, new ObservableDictionaryAddedEndSpyEventArgs()
            {
                Name = this.Name,
                Context = this,
                EndTime = DateTime.UtcNow,
            });
        }

        /// <summary>
        ///  Notify spy and listeners of the remove.
        /// </summary>
        /// <param name="key">The key of the value that was removed.</param>
        /// <param name="value">The value that was removed.</param>
        private void NotifyDictionaryChildRemove(TKey key, TValue value)
        {
            this.SharedState.OnSpy(this, new ObservableDictionaryRemovedStartSpyEventArgs(key)
            {
                Name = this.Name,
                Context = this,
                RemovedValue = value,
                StartTime = DateTime.UtcNow,
            });

            var eventArgs = new ObservableDictionaryRemovedEventArgs<TKey, TValue>(key)
            {
                Context = this,
                RemovedValue = value,
            };

            this.NotifyListeners(eventArgs);

            this.SharedState.OnSpy(this, new ObservableDictionaryRemovedEndSpyEventArgs()
            {
                Name = this.Name,
                Context = this,
                EndTime = DateTime.UtcNow,
            });
        }

        /// <summary>
        /// Notifies Listeners on the <see cref="Changed"/> event.
        /// </summary>
        /// <param name="eventArgs">The event arguments.</param>
        private void NotifyListeners(ObservableDictionaryEventArgs eventArgs)
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
