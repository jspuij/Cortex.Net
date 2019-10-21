// <copyright file="SharedStateExtensions.cs" company="Michel Weststrate, Jan-Willem Spuij">
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

namespace Cortex.Net.Core
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Extensions class for <see cref="ISharedState"/> instances.
    /// </summary>
    public static class SharedStateExtensions
    {
        /// <summary>
        /// Executes a function without tracking derivations.
        /// </summary>
        /// <typeparam name="T">The type of the return value of the function.</typeparam>
        /// <param name="sharedState">The <see cref="ISharedState"/> instance to use to temporarily stop tracking derivations.</param>
        /// <param name="func">The function to execute.</param>
        /// <returns>The return value.</returns>
        public static T Untracked<T>(this ISharedState sharedState, Func<T> func)
        {
            if (sharedState is null)
            {
                throw new ArgumentNullException(nameof(sharedState));
            }

            if (func is null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            var previousDerivation = sharedState.StartUntracked();
            try
            {
                return func();
            }
            finally
            {
                sharedState.EndTracking(previousDerivation);
            }
        }
    }
}
