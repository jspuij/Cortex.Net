// <copyright file="DisposableDelegate.cs" company="Michel Weststrate, Jan-Willem Spuij">
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
    /// Class that executes a delegate on disposal.
    /// </summary>
    public sealed class DisposableDelegate : IDisposable
    {
        private readonly Action action;

        /// <summary>
        /// Initializes a new instance of the <see cref="DisposableDelegate"/> class.
        /// </summary>
        /// <param name="action">The delegate to execute on disposal.</param>
        public DisposableDelegate(Action action)
        {
            this.action = action ?? throw new ArgumentNullException(nameof(action));
        }

        /// <summary>
        /// Executes the delegate that was encapsulated by this <see cref="DisposableDelegate"/>.
        /// </summary>
        public void Dispose()
        {
            this.action();
        }
    }
}
