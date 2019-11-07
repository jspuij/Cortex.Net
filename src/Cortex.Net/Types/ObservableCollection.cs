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
    using System.Collections.ObjectModel;
    using System.Text;
    using Cortex.Net.Core;

    /// <summary>
    /// Implements an Observable collection of Items.
    /// </summary>
    /// <typeparam name="T">The type parameter to work on.</typeparam>
    public class ObservableCollection<T> : ICollection<T>, IEnumerable<T>, IEnumerable, IList<T>, IReadOnlyCollection<T>, IReadOnlyList<T>, ICollection, IList
    {
        private readonly List<T> innerList;
        private readonly IAtom atom;
        private readonly ISharedState sharedState;
        private readonly IEnhancer enhancer;
        private readonly string name;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableCollection{T}"/> class.
        /// </summary>
        /// <param name="sharedState">The <see cref="ISharedState"/> instance this observableCollection belongs to.</param>
        /// <param name="enhancer">The <see cref="IEnhancer"/> implementation to use.</param>
        /// <param name="name">The name of the ObservableCollection.</param>
        public ObservableCollection(ISharedState sharedState, IEnhancer enhancer, string name = null)
        {
            this.sharedState = sharedState ?? throw new ArgumentNullException(nameof(sharedState));
            this.enhancer = enhancer ?? throw new ArgumentNullException(nameof(enhancer));

            if (string.IsNullOrEmpty(name))
            {
                name = $"{nameof(ObservableCollection<T>)}@{this.sharedState.GetUniqueId()}";
            }

            this.name = name;
        }

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

        public int Count => throw new NotImplementedException();

        public bool IsReadOnly => throw new NotImplementedException();

        public bool IsSynchronized => throw new NotImplementedException();

        public object SyncRoot => throw new NotImplementedException();

        public bool IsFixedSize => throw new NotImplementedException();

        public void Add(T item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(T item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public int IndexOf(T item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, T item)
        {
            throw new NotImplementedException();
        }

        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        int IList.Add(object value)
        {
            throw new NotImplementedException();
        }

        bool IList.Contains(object value)
        {
            throw new NotImplementedException();
        }

        void ICollection.CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        int IList.IndexOf(object value)
        {
            throw new NotImplementedException();
        }

        void IList.Insert(int index, object value)
        {
            throw new NotImplementedException();
        }

        void IList.Remove(object value)
        {
            throw new NotImplementedException();
        }

        void IList.RemoveAt(int index)
        {
            throw new NotImplementedException();
        }
    }
}
