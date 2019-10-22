// <copyright file="ComputedValueOptions.cs" company="Michel Weststrate, Jan-Willem Spuij">
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

namespace Cortex.Net
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Cortex.Net.Core;

    /// <summary>
    /// Options class for the Constructor of <see cref="ComputedValue{T}"/> class.
    /// </summary>
    /// <typeparam name="T">The type of the getter / setter.</typeparam>
    public class ComputedValueOptions<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ComputedValueOptions{T}"/> class.
        /// </summary>
        /// <param name="getter">The getter for the computed value.</param>
        /// <param name="name">The name of the computed value.</param>
        public ComputedValueOptions(Func<T> getter, string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            this.Getter = getter ?? throw new ArgumentNullException(nameof(getter));
            this.Name = name;
        }

        /// <summary>
        /// Gets the getter function.
        /// </summary>
        public Func<T> Getter { get; private set; }

        /// <summary>
        /// Gets or sets setter function.
        /// </summary>
        public Action<T> Setter { get; set; } = null;

        /// <summary>
        /// Gets or sets the name of the computed value.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets an optional equality comparer for type <typeparamref name="T"/>.
        /// </summary>
        public IEqualityComparer<T> EqualityComparer { get; set; }

        /// <summary>
        /// Gets or sets the context where the computed value operates on (If Any).
        /// </summary>
        public object Context { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="ComputedValue{T}"/> requires a reactive context.
        /// </summary>
        public bool RequiresReaction { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether the computed value keeps calculating, even when it is not observed.
        /// </summary>
        public bool KeepAlive { get; set; } = false;
    }
}
