// <copyright file="CortexConfiguration.cs" company="Michel Weststrate, Jan-Willem Spuij">
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
    /// Configuration parameters for an <see cref="ISharedState"/> instance.
    /// </summary>
    public class CortexConfiguration
    {
        /// <summary>
        /// Gets or sets a value indicating whether to catch and rethrow exceptions.
        /// This is useful for inspecting the state of the stack when an exception occurs while debugging.
        /// </summary>
        /// <remarks>
        /// Enabling this setting makes it possible for the graph to be left in
        /// an inconsistent state. Do not enable this in production.
        /// </remarks>
        public bool DisableErrorBoundaries { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to warn if observables are accessed outside a reactive context.
        /// </summary>
        public bool ObservableRequiresReaction { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to warn if reactions are required to visit at least one observable.
        /// </summary>
        public bool ReactionRequiresObservable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a <see cref="ComputedValue{T}"/> instance requires a reactive context.
        /// </summary>
        public bool ComputedRequiresReaction { get; set; }

        /// <summary>
        /// Gets or sets the Maximum number of reaction iterations that is allowed.
        /// </summary>
        public int MaxReactionIteractions { get; set; } = 100;

        /// <summary>
        /// Gets or sets a value that defines how strict modification of state should be enforced.
        /// </summary>
        public EnforceAction EnforceActions { get; set; }
    }
}
