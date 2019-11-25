// <copyright file="ActionExtensions.cs" company="Michel Weststrate, Jan-Willem Spuij">
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

    /// <summary>
    /// Extensions class for <see cref="ISharedState"/> instances.
    /// </summary>
    public static partial class ActionExtensions
    {
        /// <summary>
        /// Creates and runs an Action that triggers reaction in all observables in the shared state.
        /// </summary>
        /// <param name="sharedState">The name of the shared state to use to create this action.</param>
        /// <param name="action">The action itself.</param>
        public static void RunInAction(this ISharedState sharedState, Action action)
        {
            CreateAction(sharedState, "<unnamed action>", null, action)();
        }

        /// <summary>
        /// Creates and runs an Action that triggers reaction in all observables in the shared state.
        /// </summary>
        /// <param name="sharedState">The name of the shared state to use to create this action.</param>
        /// <param name="actionName">The name of this action.</param>
        /// <param name="action">The action itself.</param>
        public static void RunInAction(this ISharedState sharedState, string actionName, Action action)
        {
            CreateAction(sharedState, actionName, null, action)();
        }

        /// <summary>
        /// Creates and runs an Action that triggers reaction in all observables in the shared state.
        /// </summary>
        /// <param name="sharedState">The name of the shared state to use to create this action.</param>
        /// <param name="actionName">The name of this action.</param>
        /// <param name="scope">The scope of this action.</param>
        /// <param name="action">The action itself.</param>
        public static void RunInAction(this ISharedState sharedState, string actionName, object scope, Action action)
        {
            CreateAction(sharedState, actionName, scope, action)();
        }
    }
}