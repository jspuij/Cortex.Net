// <copyright file="ObservableCollectionAddEventArgs.cs" company="Michel Weststrate, Jan-Willem Spuij">
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
    using System.Text;

    /// <summary>
    /// Event arguments for an element that was added to a collection.
    /// </summary>
    /// <typeparam name="T">The type of the value that will change.</typeparam>
    public class ObservableCollectionAddEventArgs<T> : ObservableCollectionCancellableEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableCollectionAddEventArgs{T}"/> class.
        /// </summary>
        /// <param name="addedValues">Added values.</param>
        public ObservableCollectionAddEventArgs(ICollection<T> addedValues)
        {
            this.AddedValues = addedValues ?? throw new ArgumentNullException(nameof(addedValues));
        }

        /// <summary>
        /// Gets the added values.
        /// </summary>
        public ICollection<T> AddedValues { get; private set; }
    }
}
