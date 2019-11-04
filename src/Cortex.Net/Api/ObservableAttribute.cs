// <copyright file="ObservableAttribute.cs" company="Michel Weststrate, Jan-Willem Spuij">
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
    /// Attribute that signals that the property or class. it is applied to, should be interpreted as an observable.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class ObservableAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableAttribute"/> class.
        /// </summary>
        public ObservableAttribute()
        {
            this.EnhancerType = typeof(DeepEnhancer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableAttribute"/> class.
        /// </summary>
        /// <param name="name">The name of the action.</param>
        public ObservableAttribute(string name)
        {
            this.Name = name;
            this.EnhancerType = typeof(DeepEnhancer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableAttribute"/> class.
        /// </summary>
        /// <param name="enhancerType">The type of the enhancer.</param>
        public ObservableAttribute(Type enhancerType)
        {
            this.EnhancerType = enhancerType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableAttribute"/> class.
        /// </summary>
        /// <param name="name">The name of the action.</param>
        /// <param name="enhancerType">The type of the enhancer.</param>
        public ObservableAttribute(string name, Type enhancerType)
        {
            this.Name = name;
            this.EnhancerType = enhancerType;
        }

        /// <summary>
        /// Gets the Name of the action.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the Name of the action.
        /// </summary>
        public Type EnhancerType { get; }
    }
}
