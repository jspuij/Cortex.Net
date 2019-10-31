// <copyright file="ObservableObject.cs" company="Michel Weststrate, Jan-Willem Spuij">
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
    using Cortex.Net.Core;

    /// <summary>
    /// Base or inner class for observable objects.
    /// </summary>
    public class ObservableObject : IObservableObject
    {
        /// <summary>
        /// The default enhancer that possibly makes new values observable as well.
        /// </summary>
        private readonly IEnhancer defaultEnhancer;

        /// <summary>
        /// An atom for managing addition or removal of property / method keys.
        /// </summary>
        private IAtom keys;

        /// <summary>
        /// A reference to the shared state.
        /// </summary>
        private ISharedState sharedState;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableObject"/> class.
        /// </summary>
        /// <param name="name">The name of the objservable ovject.</param>
        /// <param name="sharedState">The shared state for this ObservableObject.</param>
        /// <param name="values">A dictionary with values.</param>
        public ObservableObject(string name, ISharedState sharedState = null, IDictionary<string, object> values)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            this.Name = name;
            this.SharedState = sharedState;
        }

        /// <summary>
        /// Gets the name of this ObservableObject.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets or sets the Shared State on this object.
        /// </summary>
        public ISharedState SharedState
        {
            get => this.sharedState;

            set
            {
                this.sharedState = value;
                if (this.sharedState != null)
                {
                    this.keys = new Atom(this.sharedState, $"{this.Name}.keys");
                }
            }
        }
    }
}
