// <copyright file="ObservableCollection.cs" company="Michel Weststrate, Jan-Willem Spuij">
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
    using Cortex.Net.Core;
    using Cortex.Net.Properties;
    using Cortex.Net.Spy;

    /// <summary>
    /// Implements an Observable collection of Items.
    /// </summary>
    /// <typeparam name="T">The type parameter to work on.</typeparam>
    public class ObservableCollection<T> : ICollection<T>, IEnumerable<T>, IEnumerable, IList<T>, IReadOnlyCollection<T>, IReadOnlyList<T>, ICollection, IList
    {
        private readonly List<T> innerList;
        private readonly IAtom atom;
        private readonly IEnhancer enhancer;

        /// <summary>
        /// A set of event handlers for the change event.
        /// </summary>
        private readonly HashSet<EventHandler<ObservableCollectionCancellableEventArgs>> changeEventHandlers = new HashSet<EventHandler<ObservableCollectionCancellableEventArgs>>();

        /// <summary>
        /// A set of event handlers for the changed event.
        /// </summary>
        private readonly HashSet<EventHandler<ObservableCollectionEventArgs>> changedEventHandlers = new HashSet<EventHandler<ObservableCollectionEventArgs>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableCollection{T}"/> class.
        /// </summary>
        /// <param name="sharedState">The <see cref="ISharedState"/> instance this observableCollection belongs to.</param>
        /// <param name="enhancer">The <see cref="IEnhancer"/> implementation to use.</param>
        /// <param name="name">The name of the ObservableCollection.</param>
        public ObservableCollection(ISharedState sharedState, IEnhancer enhancer, string name = null)
        {
            this.SharedState = sharedState ?? throw new ArgumentNullException(nameof(sharedState));
            this.enhancer = enhancer ?? throw new ArgumentNullException(nameof(enhancer));

            if (string.IsNullOrEmpty(name))
            {
                name = $"{nameof(ObservableCollection<T>)}@{this.SharedState.GetUniqueId()}";
            }

            this.Name = name;
            this.innerList = new List<T>();
            this.atom = new Atom(sharedState, name);
        }

        /// <summary>
        /// Event that fires before the value will change.
        /// </summary>
        public event EventHandler<ObservableCollectionCancellableEventArgs> Change
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
        public event EventHandler<ObservableCollectionEventArgs> Changed
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
        /// Gets the shared state this <see cref="ObservableCollection{T}"/> operates on.
        /// </summary>
        public ISharedState SharedState { get; }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="ObservableCollection{T}"/>.
        /// </summary>
        public int Count
        {
            get
            {
                this.atom.ReportObserved();
                return this.innerList.Count;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="ObservableCollection{T}"/> is read-only.
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        ///  Gets a value indicating whether access to the <see cref="ObservableCollection{T}"/> is synchronized (thread safe).
        /// </summary>
        public bool IsSynchronized => false;

        /// <summary>
        /// Gets an object that can be used to synchronize access to the <see cref="ObservableCollection{T}"/>.
        /// </summary>
        public object SyncRoot { get; } = new object();

        /// <summary>
        /// Gets a value indicating whether the <see cref="ObservableCollection{T}"/> has a fixed size.
        /// </summary>
        public bool IsFixedSize => false;

        /// <summary>
        /// Gets the name of the <see cref="ObservableCollection{T}"/>.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <returns>The element at the specified index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">index is not a valid index in the <see cref="ObservableCollection{T}"/>.</exception>
        /// <exception cref="NotSupportedException">The property is set and the <see cref="ObservableCollection{T}"/> is read-only.</exception>
        public T this[int index]
        {
            get
            {
                if (index >= 0 && index < this.innerList.Count)
                {
                    this.atom.ReportObserved();
                }

                return this.innerList[index];
            }

            set
            {
                if (index >= 0 && index < this.innerList.Count && !this.IsReadOnly)
                {
                    this.atom.CheckIfStateModificationsAreAllowed();
                    var oldValue = this.innerList[index];

                    var changeEventArgs = new ObservableCollectionChangeEventArgs<T>()
                    {
                        Cancel = false,
                        Context = this,
                        NewValue = value,
                        OldValue = oldValue,
                    };

                    this.InterceptChange(changeEventArgs);

                    if (changeEventArgs.Cancel || !changeEventArgs.Changed)
                    {
                        return;
                    }

                    var newValue = this.enhancer.Enhance(changeEventArgs.NewValue, oldValue, this.Name);

                    if (!Equals(oldValue, newValue))
                    {
                        if (newValue is IReactiveObject reactiveObject)
                        {
                            if (reactiveObject.SharedState != this.SharedState)
                            {
                                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.DifferentSharedStates, this.Name));
                            }
                        }

                        this.innerList[index] = newValue;

                        this.NotifyArrayChildUpdate(index, newValue, oldValue);
                    }
                }
                else
                {
                    // this will trap
                    this.innerList[index] = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <returns>The element at the specified index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">index is not a valid index in the <see cref="ObservableCollection{T}"/>.</exception>
        /// <exception cref="NotSupportedException">The property is set and the <see cref="ObservableCollection{T}"/> is read-only.</exception>
        object IList.this[int index]
        {
            get => this[index];
            set => this[index] = (T)value;
        }

        /// <summary>
        /// Adds an item to the <see cref="ObservableCollection{T}"/>.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="ObservableCollection{T}"/>.</param>
        public void Add(T item)
        {
                // add is insert at the index past the last item.
                this.Insert(this.innerList.Count, item);
        }

        /// <summary>
        ///  Removes all elements from the <see cref="ObservableCollection{T}"/>.
        /// </summary>
        public void Clear()
        {
            this.RemoveRange(0, this.innerList.Count);
        }

        /// <summary>
        /// Determines whether the <see cref="ObservableCollection{T}"/> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="ObservableCollection{T}"/>.</param>
        /// <returns>true if item is found in the <see cref="ObservableCollection{T}"/>; otherwise, false.</returns>
        public bool Contains(T item)
        {
            this.atom.ReportObserved();
            return this.innerList.Contains(item);
        }

        /// <summary>
        /// Copies the elements of the <see cref="ObservableCollection{T}"/> to an System.Array,
        /// starting at a particular System.Array index.
        /// </summary>
        /// <param name="array">The one-dimensional System.Array that is the destination of the elements copied
        /// from <see cref="ObservableCollection{T}"/>. The System.Array must have zero-based indexing.
        /// </param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        /// <exception cref="ArgumentNullException">array is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">arrayIndex is less than 0.</exception>
        /// <exception cref="ArgumentException">
        /// The number of elements in the source System.Collections.Generic.ICollection`1
        /// is greater than the available space from arrayIndex to the end of the destination array.
        /// </exception>
        public void CopyTo(T[] array, int arrayIndex)
        {
            this.innerList.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="ObservableCollection{T}"/>.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the <see cref="ObservableCollection{T}"/>.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            this.atom.ReportObserved();
            return this.innerList.GetEnumerator();
        }

        /// <summary>
        /// Searches for the specified object and returns the zero-based index of the first
        /// occurrence within the entire <see cref="ObservableCollection{T}"/>.
        /// </summary>
        /// <param name="item">The object to locate in the System.Collections.Generic.List`1. The value can
        /// be null for reference types.
        /// </param>
        /// <returns>
        /// The zero-based index of the first occurrence of item within the entire <see cref="ObservableCollection{T}"/>, if found; otherwise, –1.
        /// </returns>
        public int IndexOf(T item)
        {
            this.atom.ReportObserved();
            return this.innerList.IndexOf(item);
        }

        /// <summary>
        ///  Inserts an item to the <see cref="ObservableCollection{T}"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        /// <param name="item">The object to insert into the <see cref="ObservableCollection{T}"/>.</param>
        public void Insert(int index, T item)
        {
            this.InsertRange(index, new[] { item });
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="ObservableCollection{T}"/>.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="ObservableCollection{T}"/>.
        /// The value can be null for reference types.
        /// </param>
        /// <returns>True if item is successfully removed; otherwise, false. This method also returns
        /// false if item was not found in the <see cref="ObservableCollection{T}"/>.
        /// </returns>
        public bool Remove(T item)
        {
            int index = this.innerList.IndexOf(item);
            if (index < 0)
            {
                return false;
            }

            return this.RemoveRange(index, 1);
        }

        /// <summary>
        /// Removes the <see cref="ObservableCollection{T}"/> item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException">Index is not a valid index in the <see cref="ObservableCollection{T}"/>.</exception>
        /// <exception cref="NotSupportedException">The <see cref="ObservableCollection{T}"/> is read-only.</exception>
        public void RemoveAt(int index)
        {
            this.RemoveRange(index, 1);
        }

        /// <summary>
        /// Adds an item to the <see cref="ObservableCollection{T}"/>.
        /// </summary>
        /// <param name="value">The object to add to the <see cref="ObservableCollection{T}"/>.</param>
        /// <returns>The index where the item was added.</returns>
        int IList.Add(object value)
        {
            int result = this.innerList.Count;
            this.Add((T)value);
            return result;
        }

        /// <summary>
        /// Determines whether the <see cref="ObservableCollection{T}"/> contains a specific value.
        /// </summary>
        /// <param name="value">The object to locate in the <see cref="ObservableCollection{T}"/>.</param>
        /// <returns>true if item is found in the <see cref="ObservableCollection{T}"/>; otherwise, false.</returns>
        bool IList.Contains(object value)
        {
            return this.Contains((T)value);
        }

        /// <summary>
        /// Copies the elements of the <see cref="ObservableCollection{T}"/> to an System.Array,
        /// starting at a particular System.Array index.
        /// </summary>
        /// <param name="array">The one-dimensional System.Array that is the destination of the elements copied
        /// from <see cref="ObservableCollection{T}"/>. The System.Array must have zero-based indexing.
        /// </param>
        /// <param name="index">The zero-based index in array at which copying begins.</param>
        /// <exception cref="ArgumentNullException">array is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">arrayIndex is less than 0.</exception>
        /// <exception cref="ArgumentException">
        /// The number of elements in the source System.Collections.Generic.ICollection`1
        /// is greater than the available space from arrayIndex to the end of the destination array.
        /// </exception>
        void ICollection.CopyTo(Array array, int index)
        {
            ((ICollection)this.innerList).CopyTo(array, index);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="ObservableCollection{T}"/>.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the <see cref="ObservableCollection{T}"/>.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            this.atom.ReportObserved();
            return this.innerList.GetEnumerator();
        }

        /// <summary>
        /// Searches for the specified object and returns the zero-based index of the first
        /// occurrence within the entire <see cref="ObservableCollection{T}"/>.
        /// </summary>
        /// <param name="value">The object to locate in the System.Collections.Generic.List`1. The value can
        /// be null for reference types.
        /// </param>
        /// <returns>
        /// The zero-based index of the first occurrence of item within the entire <see cref="ObservableCollection{T}"/>, if found; otherwise, –1.
        /// </returns>
        int IList.IndexOf(object value)
        {
            return this.IndexOf((T)value);
        }

        /// <summary>
        ///  Inserts an item to the <see cref="ObservableCollection{T}"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        /// <param name="value">The object to insert into the <see cref="ObservableCollection{T}"/>.</param>
        void IList.Insert(int index, object value)
        {
            this.Insert(index, (T)value);
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="ObservableCollection{T}"/>.
        /// </summary>
        /// <param name="value">The object to remove from the <see cref="ObservableCollection{T}"/>.
        /// The value can be null for reference types.
        /// </param>
        void IList.Remove(object value)
        {
            this.Remove((T)value);
        }

        /// <summary>
        /// Removes the <see cref="ObservableCollection{T}"/> item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException">Index is not a valid index in the <see cref="ObservableCollection{T}"/>.</exception>
        /// <exception cref="NotSupportedException">The <see cref="ObservableCollection{T}"/> is read-only.</exception>
        void IList.RemoveAt(int index)
        {
            this.RemoveAt(index);
        }

        /// <summary>
        /// Insert a range of items into the collection.
        /// </summary>
        /// <param name="index">The index to insert at.</param>
        /// <param name="items">The items to remove.</param>
        private int InsertRange(int index, IEnumerable<T> items)
        {
            if (index >= 0 && index <= this.innerList.Count && !this.IsReadOnly)
            {
                this.atom.CheckIfStateModificationsAreAllowed();

                var addEventArgs = new ObservableCollectionAddEventArgs<T>(items.ToList())
                {
                    Index = index,
                    Cancel = false,
                    Context = this,
                };

                this.InterceptChange(addEventArgs);

                if (addEventArgs.Cancel)
                {
                    return -1;
                }

                var newItems = addEventArgs.AddedValues.Select(x => this.enhancer.Enhance(x, default, this.Name));

                foreach (var item in newItems)
                {
                    if (item is IReactiveObject reactiveObject)
                    {
                        if (reactiveObject.SharedState != this.SharedState)
                        {
                            throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.DifferentSharedStates, this.Name));
                        }
                    }
                }

                this.innerList.InsertRange(index, newItems);

                this.NotifyArrayChildAdd(index, newItems);
            }
            else
            {
                // this will trap
                this.innerList.InsertRange(index, items);
            }

            return index;
        }

        /// <summary>
        /// Removes a range of objects from the collection.
        /// </summary>
        /// <param name="index">The index to start from.</param>
        /// <param name="count">The number of items to remove.</param>
        private bool RemoveRange(int index, int count)
        {
            if (index >= 0 && (index + count) <= this.innerList.Count && !this.IsReadOnly)
            {
                this.atom.CheckIfStateModificationsAreAllowed();

                var items = this.innerList.GetRange(index, count);

                var removeEventArgs = new ObservableCollectionRemoveEventArgs<T>(items)
                {
                    Index = index,
                    Cancel = false,
                    Context = this,
                };

                this.InterceptChange(removeEventArgs);

                if (removeEventArgs.Cancel)
                {
                    return false;
                }

                this.innerList.RemoveRange(index, count);

                this.NotifyArrayChildRemove(index, items);
            }
            else
            {
                // this will trap
                this.innerList.RemoveRange(index, count);
            }

            return true;
        }

        /// <summary>
        /// Fires a Change event that can be intercepted and or canceled.
        /// </summary>
        /// <param name="changeEventArgs">The change event args.</param>
        private void InterceptChange(ObservableCollectionCancellableEventArgs changeEventArgs)
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
        /// <param name="index">The index of the item that got updated.</param>
        /// <param name="newValue">The new value.</param>
        /// <param name="oldValue">The old value.</param>
        private void NotifyArrayChildUpdate(int index, T newValue, T oldValue)
        {
            this.SharedState.OnSpy(this, new ObservableCollectionChangedStartSpyEventArgs()
            {
                Name = this.Name,
                Context = this,
                Index = index,
                OldValue = oldValue,
                NewValue = newValue,
                StartTime = DateTime.UtcNow,
            });

            var eventArgs = new ObservableCollectionChangedEventArgs<T>()
            {
                Context = this,
                Index = index,
                OldValue = oldValue,
                NewValue = newValue,
            };

            this.atom.ReportChanged();
            this.NotifyListeners(eventArgs);

            this.SharedState.OnSpy(this, new ObservableCollectionChangedEndSpyEventArgs()
            {
                Name = this.Name,
                Context = this,
                Index = index,
                EndTime = DateTime.UtcNow,
            });
        }

        /// <summary>
        ///  Notify spy and listeners of the add.
        /// </summary>
        /// <param name="index">The index at which the items were added.</param>
        /// <param name="newItems">The items that were added.</param>
        private void NotifyArrayChildAdd(int index, IEnumerable<T> newItems)
        {
            this.SharedState.OnSpy(this, new ObservableCollectionAddedStartSpyEventArgs(newItems)
            {
                Name = this.Name,
                Context = this,
                Index = index,
                StartTime = DateTime.UtcNow,
            });

            var eventArgs = new ObservableCollectionAddedEventArgs<T>(newItems)
            {
                Context = this,
                Index = index,
            };

            this.atom.ReportChanged();
            this.NotifyListeners(eventArgs);

            this.SharedState.OnSpy(this, new ObservableCollectionAddedEndSpyEventArgs()
            {
                Name = this.Name,
                Context = this,
                Index = index,
                EndTime = DateTime.UtcNow,
            });
        }

        /// <summary>
        ///  Notify spy and listeners of the remove.
        /// </summary>
        /// <param name="index">The index at which the items were removed.</param>
        /// <param name="removedItems">The items that were removed.</param>
        private void NotifyArrayChildRemove(int index, IEnumerable<T> removedItems)
        {
            this.SharedState.OnSpy(this, new ObservableCollectionRemovedStartSpyEventArgs(removedItems)
            {
                Name = this.Name,
                Context = this,
                Index = index,
                StartTime = DateTime.UtcNow,
            });

            var eventArgs = new ObservableCollectionRemovedEventArgs<T>(removedItems)
            {
                Context = this,
                Index = index,
            };

            this.atom.ReportChanged();
            this.NotifyListeners(eventArgs);

            this.SharedState.OnSpy(this, new ObservableCollectionRemovedEndSpyEventArgs()
            {
                Name = this.Name,
                Context = this,
                Index = index,
                EndTime = DateTime.UtcNow,
            });
        }

        /// <summary>
        /// Notifies Listeners on the <see cref="Changed"/> event.
        /// </summary>
        /// <param name="eventArgs">The event arguments.</param>
        private void NotifyListeners(ObservableCollectionEventArgs eventArgs)
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
