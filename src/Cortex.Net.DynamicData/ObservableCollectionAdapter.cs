// <copyright file="ObservableCollectionAdapter.cs" company="Jan-Willem Spuij">
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
    using Cortex.Net.Api;
    using Cortex.Net.Types;
    using global::DynamicData;

    /// <summary>
    /// An adapter class that binds an observable stream of changesets to
    /// a Cortex.Net observable collection.
    /// </summary>
    /// <typeparam name="T">The type of the collection.</typeparam>
    public sealed class ObservableCollectionAdapter<T> : IChangeSetAdaptor<T>
    {
        /// <summary>
        /// The observable collection to update.
        /// </summary>
        private readonly ObservableCollection<T> observableCollection;

        /// <summary>
        /// The equality comparer to use to compare instances of T.
        /// </summary>
        private readonly IEqualityComparer<T> equalityComparer;

        /// <summary>
        /// The threshold to use to refresh the entire list.
        /// </summary>
        private readonly int resetThreshold;

        /// <summary>
        /// Collection loaded.
        /// </summary>
        private bool loaded;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableCollectionAdapter{T}"/> class.
        /// </summary>
        /// <param name="observableCollection">The Cortex.Net observable Collection to bind to.</param>
        /// <param name="equalityComparer">The equality comparer to use to compare items.</param>
        /// <param name="resetThreshold">The reset threshold to just reset the entire collection.</param>
        internal ObservableCollectionAdapter(ObservableCollection<T> observableCollection, IEqualityComparer<T> equalityComparer, int resetThreshold)
        {
            this.observableCollection = observableCollection ?? throw new ArgumentNullException(nameof(observableCollection));
            this.equalityComparer = equalityComparer;
            this.resetThreshold = resetThreshold;
        }

        /// <summary>
        /// Adapt the Cortex.Net collection based on a single changeset.
        /// </summary>
        /// <param name="change">The changeset to process.</param>
        public void Adapt(IChangeSet<T> change)
        {
            if (change == null)
            {
                throw new ArgumentNullException(nameof(change));
            }

            this.observableCollection.SharedState.RunInAction(() =>
            {
                /*if (change.TotalChanges - change.Refreshes > this.resetThreshold || !this.loaded)
                {
                    this.observableCollection.Clear();
                    this.loaded = true;
                }*/

                this.observableCollection.Clone(change, this.equalityComparer);
            });
        }
    }
}
