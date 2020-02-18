// <copyright file="ObservableChangeSetExtensions.cs" company="Jan-Willem Spuij">
// Copyright 2019 Jan-Willem Spuij
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

namespace Cortex.Net.DynamicData
{
    using System;
    using System.Collections.Generic;
    using Cortex.Net.Types;
    using global::DynamicData;

    /// <summary>
    /// Extension methods for binding to observable changeset instances.
    /// </summary>
    public static class ObservableChangeSetExtensions
    {
        /// <summary>
        /// Binds the observable changeset to a Cortex.Net observable collection.
        /// </summary>
        /// <typeparam name="T">The type of the collection.</typeparam>
        /// <param name="source">The source observable changeset.</param>
        /// <param name="observableCollection">The observable collection.</param>
        /// <param name="equalityComparer">The equality comparer.</param>
        /// <param name="resetThreshold">The threshold to use to just refresh the entire collection.</param>
        /// <returns>The observable changeset.</returns>
        public static IObservable<IChangeSet<T>> CortexBind<T>(this IObservable<IChangeSet<T>> source, ObservableCollection<T> observableCollection, IEqualityComparer<T> equalityComparer = null, int resetThreshold = 25)
        {
            var adapter = new ObservableCollectionAdapter<T>(observableCollection, equalityComparer, resetThreshold);
            return source.Adapt(adapter);
        }

        /// <summary>
        /// Binds the observable changeset to a Cortex.Net observable dictionary.
        /// </summary>
        /// <typeparam name="TValue">The value type of the dictionary.</typeparam>
        /// <typeparam name="TKey">The key type of the dictionary.</typeparam>
        /// <param name="source">The source observable changeset.</param>
        /// <param name="observableDictionary">The observable collection.</param>
        /// <param name="resetThreshold">The threshold to use to just refresh the entire collection.</param>
        /// <returns>The observable changeset.</returns>
        public static IObservable<IChangeSet<TValue, TKey>> CortexBind<TValue, TKey>(this IObservable<IChangeSet<TValue, TKey>> source, ObservableDictionary<TKey, TValue> observableDictionary, int resetThreshold = 25)
        {
            var adapter = new ObservableDictionaryAdapter<TKey, TValue>(observableDictionary, resetThreshold);
            return source.Adapt(adapter);
        }
    }
}
