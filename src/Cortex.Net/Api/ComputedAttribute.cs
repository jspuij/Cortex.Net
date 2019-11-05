// <copyright file="ComputedAttribute.cs" company="Michel Weststrate, Jan-Willem Spuij">
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
    using System.Collections;
    using System.Text;

    /// <summary>
    /// Attribute that signals that the property or method it is applied to, should be interpreted as a computed value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
    public class ComputedAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ComputedAttribute"/> class.
        /// </summary>
        /// <param name="requiresReaction">Whether the computed has to be calculated inside a reactive context.</param>
        /// <param name="keepAlive">whether to keep the computed value alive when it's not observed.</param>
        public ComputedAttribute(bool requiresReaction = false, bool keepAlive = false)
        {
            this.RequiresReaction = requiresReaction;
            this.KeepAlive = keepAlive;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComputedAttribute"/> class.
        /// </summary>
        /// <param name="name">The name of the action.</param>
        /// <param name="requiresReaction">Whether the computed has to be calculated inside a reactive context.</param>
        /// <param name="keepAlive">whether to keep the computed value alive when it's not observed.</param>
        public ComputedAttribute(string name, bool requiresReaction = false, bool keepAlive = false)
            : this(requiresReaction, keepAlive)
        {
            this.Name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComputedAttribute"/> class.
        /// </summary>
        /// <param name="equalityComparer">The comparer used to see whether the value has changed.</param>
        /// <param name="requiresReaction">Whether the computed has to be calculated inside a reactive context.</param>
        /// <param name="keepAlive">whether to keep the computed value alive when it's not observed.</param>
        public ComputedAttribute(IEqualityComparer equalityComparer, bool requiresReaction = false, bool keepAlive = false)
            : this(requiresReaction, keepAlive)
        {
            this.EqualityComparer = equalityComparer;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComputedAttribute"/> class.
        /// </summary>
        /// <param name="name">The name of the action.</param>
        /// <param name="equalityComparer">The comparer used to see whether the value has changed.</param>
        /// <param name="requiresReaction">Whether the computed has to be calculated inside a reactive context.</param>
        /// <param name="keepAlive">whether to keep the computed value alive when it's not observed.</param>
        public ComputedAttribute(string name, IEqualityComparer equalityComparer, bool requiresReaction = false, bool keepAlive = false)
            : this(requiresReaction, keepAlive)
        {
            this.Name = name;
            this.EqualityComparer = equalityComparer;
        }

        /// <summary>
        /// Gets the Name of the action.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///  Gets a value indicating whether a reactive context is required.
        ///  it is recommended to set this one to true on very expensive computed values.
        ///  If you try to read it's value, but the value is not being tracked by some observe, it will cause the computed to throw,
        ///  instead of doing an expensive re-evalution.
        /// </summary>
        public bool RequiresReaction { get; }

        /// <summary>
        /// Gets a value indicating whether to keep this computed value alive if it is not observed by anybody.
        /// Be aware, this can easily lead to memory leaks as it will result in every observable used by this computed value, keeping the computed value in memory.
        /// </summary>
        public bool KeepAlive { get; }

        /// <summary>
        /// Gets the Comparer used for equality.
        /// </summary>
        public IEqualityComparer EqualityComparer { get; }
    }
}
