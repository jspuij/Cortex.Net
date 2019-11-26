// <copyright file="ObservableSet.cs" company="Michel Weststrate, Jan-Willem Spuij">
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
    using System.Runtime.Serialization;
    using Cortex.Net.Api;
    using Cortex.Net.Core;
    using Cortex.Net.Properties;
    using Cortex.Net.Spy;

    /// <summary>
    /// Implements an Observable set of Items.
    /// </summary>
    /// <typeparam name="T">The type parameter to work on.</typeparam>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix", Justification = "A Set is a collection.")]
    public class ObservableSet<T> : ICollection<T>, IEnumerable<T>, IEnumerable, IReadOnlyCollection<T>, ISet<T>
    {
        private readonly HashSet<T> innerSet;
        private readonly IAtom atom;
        private readonly IEnhancer enhancer;

        /// <summary>
        /// A set of event handlers for the change event.
        /// </summary>
        private readonly HashSet<EventHandler<ObservableSetCancellableEventArgs>> changeEventHandlers = new HashSet<EventHandler<ObservableSetCancellableEventArgs>>();

        /// <summary>
        /// A set of event handlers for the changed event.
        /// </summary>
        private readonly HashSet<EventHandler<ObservableSetEventArgs>> changedEventHandlers = new HashSet<EventHandler<ObservableSetEventArgs>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableSet{T}"/> class.
        /// </summary>
        /// <param name="sharedState">The <see cref="ISharedState"/> instance this ObservableSet belongs to.</param>
        /// <param name="enhancer">The <see cref="IEnhancer"/> implementation to use.</param>
        /// <param name="name">The name of the ObservableSet.</param>
        public ObservableSet(ISharedState sharedState, IEnhancer enhancer, string name = null)
            : this(sharedState, enhancer, null, name)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableSet{T}"/> class.
        /// </summary>
        /// <param name="sharedState">The <see cref="ISharedState"/> instance this ObservableSet belongs to.</param>
        /// <param name="enhancer">The <see cref="IEnhancer"/> implementation to use.</param>
        /// <param name="name">The name of the ObservableSet.</param>
        /// <param name="initialItems">The initialItems to use.</param>
        public ObservableSet(ISharedState sharedState, IEnhancer enhancer, IEnumerable<T> initialItems, string name = null)
        {
            this.SharedState = sharedState ?? throw new ArgumentNullException(nameof(sharedState));
            this.enhancer = enhancer ?? throw new ArgumentNullException(nameof(enhancer));

            if (string.IsNullOrEmpty(name))
            {
                name = $"{nameof(ObservableSet<T>)}@{this.SharedState.GetUniqueId()}";
            }

            this.Name = name;
            this.innerSet = initialItems != null ? new HashSet<T>(initialItems) : new HashSet<T>();
            this.atom = new Atom(sharedState, name);
        }

        /// <summary>
        /// Event that fires before the value will change.
        /// </summary>
        public event EventHandler<ObservableSetCancellableEventArgs> Change
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
        public event EventHandler<ObservableSetEventArgs> Changed
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
        /// Gets the shared state this <see cref="ObservableSet{T}"/> operates on.
        /// </summary>
        public ISharedState SharedState { get; }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="ObservableSet{T}"/>.
        /// </summary>
        public int Count
        {
            get
            {
                this.atom.ReportObserved();
                return this.innerSet.Count;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="ObservableSet{T}"/> is read-only.
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        ///  Gets a value indicating whether access to the <see cref="ObservableSet{T}"/> is synchronized (thread safe).
        /// </summary>
        public bool IsSynchronized => false;

        /// <summary>
        /// Gets an object that can be used to synchronize access to the <see cref="ObservableSet{T}"/>.
        /// </summary>
        public object SyncRoot { get; } = new object();

        /// <summary>
        /// Gets a value indicating whether the <see cref="ObservableSet{T}"/> has a fixed size.
        /// </summary>
        public bool IsFixedSize => false;

        /// <summary>
        /// Gets the name of the <see cref="ObservableSet{T}"/>.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Adds an item to the <see cref="ObservableSet{T}"/>.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="ObservableSet{T}"/>.</param>
        /// <returns>True if the item was added, false otherwise.</returns>
        public bool Add(T item)
        {
            if (!this.IsReadOnly && !this.innerSet.Contains(item))
            {
                this.atom.CheckIfStateModificationsAreAllowed();

                var addEventArgs = new ObservableSetAddEventArgs<T>()
                {
                    Item = item,
                    Cancel = false,
                    Context = this,
                };

                this.InterceptChange(addEventArgs);

                if (addEventArgs.Cancel)
                {
                    return false;
                }

                var newItem = addEventArgs.Item;

                if (newItem is IReactiveObject reactiveObject)
                {
                    if (reactiveObject.SharedState != this.SharedState)
                    {
                        throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.DifferentSharedStates, this.Name));
                    }
                }

                this.innerSet.Add(newItem);

                this.NotifySetChildAdd(newItem);
            }
            else
            {
                // this will trap
                return this.innerSet.Add(item);
            }

            return true;
        }

        /// <summary>
        ///  Removes all elements from the <see cref="ObservableSet{T}"/>.
        /// </summary>
        public void Clear()
        {
            this.SharedState.Transaction(() =>
            {
                this.SharedState.Untracked(() =>
                {
                    foreach (var item in this.innerSet.ToArray())
                    {
                        this.Remove(item);
                    }
                });
            });
        }

        /// <summary>
        /// Determines whether the <see cref="ObservableSet{T}"/> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="ObservableSet{T}"/>.</param>
        /// <returns>true if item is found in the <see cref="ObservableSet{T}"/>; otherwise, false.</returns>
        public bool Contains(T item)
        {
            this.atom.ReportObserved();
            return this.innerSet.Contains(item);
        }

        /// <summary>
        /// Copies the elements of the <see cref="ObservableSet{T}"/> to an System.Set,
        /// starting at a particular System.Set index.
        /// </summary>
        /// <param name="array">The one-dimensional System.Set that is the destination of the elements copied
        /// from <see cref="ObservableSet{T}"/>. The System.Set must have zero-based indexing.
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
            this.innerSet.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="ObservableSet{T}"/>.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the <see cref="ObservableSet{T}"/>.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            this.atom.ReportObserved();
            return this.innerSet.GetEnumerator();
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="ObservableSet{T}"/>.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="ObservableSet{T}"/>.
        /// The value can be null for reference types.
        /// </param>
        /// <returns>True if item is successfully removed; otherwise, false. This method also returns
        /// false if item was not found in the <see cref="ObservableSet{T}"/>.
        /// </returns>
        public bool Remove(T item)
        {
            if (!this.IsReadOnly && this.innerSet.Contains(item))
            {
                this.atom.CheckIfStateModificationsAreAllowed();

                var removeEventArgs = new ObservableSetRemoveEventArgs<T>()
                {
                    Item = item,
                    Cancel = false,
                    Context = this,
                };

                this.InterceptChange(removeEventArgs);

                if (removeEventArgs.Cancel)
                {
                    return false;
                }

                this.innerSet.Remove(item);

                this.NotifySetChildRemove(item);
            }
            else
            {
                // this will trap
                return this.innerSet.Remove(item);
            }

            return true;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="ObservableSet{T}"/>.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the <see cref="ObservableSet{T}"/>.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            this.atom.ReportObserved();
            return this.innerSet.GetEnumerator();
        }

        /// <summary>
        /// Adds an item to the <see cref="ObservableSet{T}"/>.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="ObservableSet{T}"/>.</param>
        void ICollection<T>.Add(T item)
        {
            this.Add(item);
        }

        /// <summary>
        /// Removes all elements in the specified collection from the current <see cref="ObservableSet{T}"/> object.
        /// </summary>
        /// <param name="other">The collection of items to remove from the System.Collections.Generic.HashSet`1 object.</param>
        public void ExceptWith(IEnumerable<T> other)
        {
            if (other is null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            this.SharedState.Transaction(() =>
            {
                this.SharedState.Untracked(() =>
                {
                    foreach (var item in other)
                    {
                        if (this.innerSet.Contains(item))
                        {
                            this.Remove(item);
                        }
                    }
                });
            });
        }

        /// <summary>
        ///   Modifies the current System.Collections.Generic.HashSet`1 object to contain only elements that are present in that object and in the specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current System.Collections.Generic.HashSet`1 object.</param>
        public void IntersectWith(IEnumerable<T> other)
        {
            if (other is null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            this.SharedState.Transaction(() =>
            {
                this.SharedState.Untracked(() =>
                {
                    foreach (var item in this.innerSet)
                    {
                        if (!other.Contains(item))
                        {
                            this.Remove(item);
                        }
                    }
                });
            });
        }

        /// <summary>
        /// Determines whether a System.Collections.Generic.HashSet`1 object is a proper subset of the specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current System.Collections.Generic.HashSet`1 object.</param>
        /// <returns>true if the System.Collections.Generic.HashSet`1 object is a proper subset of other; otherwise, false.</returns>
        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            this.atom.ReportObserved();
            return this.innerSet.IsProperSubsetOf(other);
        }

        /// <summary>
        /// Determines whether a System.Collections.Generic.HashSet`1 object is a proper superset of the specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current System.Collections.Generic.HashSet`1 object.</param>
        /// <returns> true if the System.Collections.Generic.HashSet`1 object is a proper superset of other; otherwise, false.</returns>
        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            this.atom.ReportObserved();
            return this.innerSet.IsProperSupersetOf(other);
        }

        /// <summary>
        /// Determines whether a System.Collections.Generic.HashSet`1 object is a subset of the specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current System.Collections.Generic.HashSet`1 object.</param>
        /// <returns>true if the System.Collections.Generic.HashSet`1 object is a subset of other; otherwise, false.</returns>
        public bool IsSubsetOf(IEnumerable<T> other)
        {
            this.atom.ReportObserved();
            return this.innerSet.IsSubsetOf(other);
        }

        /// <summary>
        /// Determines whether a System.Collections.Generic.HashSet`1 object is a superset of the specified collection.
        /// </summary>
        /// <param name="other">The collection to compare to the current System.Collections.Generic.HashSet`1 object.</param>
        /// <returns> true if the System.Collections.Generic.HashSet`1 object is a superset of other; otherwise, false.</returns>
        public bool IsSupersetOf(IEnumerable<T> other)
        {
            this.atom.ReportObserved();
            return this.innerSet.IsSupersetOf(other);
        }

        /// <summary>
        /// Determines whether the current System.Collections.Generic.HashSet`1 object and a specified collection share common elements.
        /// </summary>
        /// <param name="other">The collection to compare to the current System.Collections.Generic.HashSet`1 object.</param>
        /// <returns>true if the System.Collections.Generic.HashSet`1 object and other share at least one common element; otherwise, false.</returns>
        public bool Overlaps(IEnumerable<T> other)
        {
            this.atom.ReportObserved();
            return this.innerSet.Overlaps(other);
        }

        /// <summary>
        /// Determines whether a System.Collections.Generic.HashSet`1 object and the specified collection contain the same elements.
        /// </summary>
        /// <param name="other">The collection to compare to the current System.Collections.Generic.HashSet`1 object.</param>
        /// <returns>true if the System.Collections.Generic.HashSet`1 object is equal to other; otherwise, false.</returns>
        public bool SetEquals(IEnumerable<T> other)
        {
            this.atom.ReportObserved();
            return this.innerSet.SetEquals(other);
        }

        /// <summary>
        /// Modifies the current System.Collections.Generic.HashSet`1 object to contain only
        ///  elements that are present either in that object or in the specified collection,
        ///  but not both.
        /// </summary>
        /// <param name="other">The collection to compare to the current System.Collections.Generic.HashSet`1 object.</param>
        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            if (other is null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            this.SharedState.Transaction(() =>
            {
                foreach (T item in other)
                {
                    if (!this.Remove(item))
                    {
                        if (!this.Contains(item))
                        {
                            this.Add(item);
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Modifies the current System.Collections.Generic.HashSet`1 object to contain all
        ///  elements that are present in itself, the specified collection, or both.
        /// </summary>
        /// <param name="other">The collection to compare to the current System.Collections.Generic.HashSet`1 object.</param>
        public void UnionWith(IEnumerable<T> other)
        {
            if (other is null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            this.SharedState.Transaction(() =>
            {
                foreach (T item in other)
                {
                    this.Add(item);
                }
            });
        }

        /// <summary>
        /// Fires a Change event that can be intercepted and or canceled.
        /// </summary>
        /// <param name="changeEventArgs">The change event args.</param>
        private void InterceptChange(ObservableSetCancellableEventArgs changeEventArgs)
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
        ///  Notify spy and listeners of the add.
        /// </summary>
        private void NotifySetChildAdd(T newItem)
        {
            this.SharedState.OnSpy(this, new ObservableSetAddedStartSpyEventArgs()
            {
                Name = this.Name,
                Context = this,
                Item = newItem,
                StartTime = DateTime.UtcNow,
            });

            var eventArgs = new ObservableSetAddedEventArgs<T>()
            {
                Context = this,
                Item = newItem,
            };

            this.atom.ReportChanged();
            this.NotifyListeners(eventArgs);

            this.SharedState.OnSpy(this, new ObservableSetAddedEndSpyEventArgs()
            {
                Name = this.Name,
                Context = this,
                EndTime = DateTime.UtcNow,
            });
        }

        /// <summary>
        ///  Notify spy and listeners of the remove.
        /// </summary>
        /// <param name="removedItem">The items that was removed.</param>
        private void NotifySetChildRemove(T removedItem)
        {
            this.SharedState.OnSpy(this, new ObservableSetRemovedStartSpyEventArgs()
            {
                Name = this.Name,
                Context = this,
                Item = removedItem,
                StartTime = DateTime.UtcNow,
            });

            var eventArgs = new ObservableSetRemovedEventArgs<T>()
            {
                Context = this,
                Item = removedItem,
            };

            this.atom.ReportChanged();
            this.NotifyListeners(eventArgs);

            this.SharedState.OnSpy(this, new ObservableSetRemovedEndSpyEventArgs()
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
        private void NotifyListeners(ObservableSetEventArgs eventArgs)
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
