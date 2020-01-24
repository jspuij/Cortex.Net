// <copyright file="CollectionExtensions.cs" company="Michel Weststrate, Jan-Willem Spuij">
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

namespace Cortex.Net.Api
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Cortex.Net.Types;

    /// <summary>
    /// Extension methods for collection instances.
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        /// Refill the collection with new items.
        /// </summary>
        /// <typeparam name="T">The type of items in the collection.</typeparam>
        /// <param name="collection">The observable collection to fill.</param>
        /// <param name="items">The items to fill the collection with.</param>
        public static void Refill<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            RefillInternal(collection, items);
        }

        /// <summary>
        /// Refill the collection with new items.
        /// </summary>
        /// <typeparam name="T">The type of items in the collection.</typeparam>
        /// <param name="collection">The observable collection to fill.</param>
        /// <param name="items">The items to fill the collection with.</param>
        public static void Refill<T>(this IReadOnlyCollection<T> collection, IEnumerable<T> items)
        {
            RefillInternal(collection, items);
        }

        private static void RefillInternal<T>(IEnumerable<T> collection, IEnumerable<T> items)
        {
            if (collection is null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            if (items is null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            var observableCollection = collection as ObservableCollection<T>;

            if (observableCollection is null)
            {
                throw new ArgumentOutOfRangeException(nameof(collection), $"The collection is not an Observable<{typeof(T).Name}> collection.");
            }

            observableCollection.SharedState.RunInAction($"Observable<{typeof(T).Name}>.Refill", () =>
            {
                observableCollection.Clear();
                observableCollection.InsertRange(0, items);
            });
        }
    }
}
