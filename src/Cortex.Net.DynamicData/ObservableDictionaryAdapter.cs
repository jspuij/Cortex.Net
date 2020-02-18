// <copyright file="ObservableDictionaryAdapter.cs" company="Jan-Willem Spuij">
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
    /// a Cortex.Net observable dictionary.
    /// </summary>
    /// <typeparam name="TKey">The key type of the dictionary.</typeparam>
    /// <typeparam name="TValue">The value type of the dictionary.</typeparam>
    public sealed class ObservableDictionaryAdapter<TKey, TValue> : IChangeSetAdaptor<TValue, TKey>
    {
        /// <summary>
        /// The observable dictionary to update.
        /// </summary>
        private readonly ObservableDictionary<TKey, TValue> observableDictionary;

        /// <summary>
        /// The threshold to use to refresh the entire list.
        /// </summary>
        private readonly int resetThreshold;

        /// <summary>
        /// Collection loaded.
        /// </summary>
        private bool loaded;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableDictionaryAdapter{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="observableDictionary">The Cortex.Net observable dictionary to bind to.</param>
        /// <param name="resetThreshold">The reset threshold to just reset the entire collection.</param>
        internal ObservableDictionaryAdapter(ObservableDictionary<TKey, TValue> observableDictionary, int resetThreshold)
        {
            this.observableDictionary = observableDictionary ?? throw new ArgumentNullException(nameof(observableDictionary));
            this.resetThreshold = resetThreshold;
        }

        /// <summary>
        /// Adapt the Cortex.Net collection based on a single changeset.
        /// </summary>
        /// <param name="changes">The changeset to process.</param>
        public void Adapt(IChangeSet<TValue, TKey> changes)
        {
            if (changes == null)
            {
                throw new ArgumentNullException(nameof(changes));
            }

            this.observableDictionary.SharedState.RunInAction(() =>
            {
                if (changes.Count - changes.Refreshes > this.resetThreshold || !this.loaded)
                {
                    this.observableDictionary.Clear();
                    this.loaded = true;
                    foreach (var update in changes)
                    {
                        switch (update.Reason)
                        {
                            case ChangeReason.Add:
                            case ChangeReason.Update:
                                this.observableDictionary.Add(update.Key, update.Current);
                                break;
                        }
                    }
                }
                else
                {
                    foreach (var update in changes)
                    {
                        switch (update.Reason)
                        {
                            case ChangeReason.Add:
                                this.observableDictionary.Add(update.Key, update.Current);
                                break;
                            case ChangeReason.Remove:
                                this.observableDictionary.Remove(update.Key);
                                break;
                            case ChangeReason.Update:
                                this.observableDictionary[update.Key] = update.Current;
                                break;
                        }
                    }
                }
            });
        }
    }
}
