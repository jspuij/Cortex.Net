// <copyright file="DelegateDisposable.cs" company="Michel Weststrate, Jan-Willem Spuij">
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
    /// Implements <see cref="IDisposable"/> by executing a deletgate.
    /// </summary>
    internal sealed class DelegateDisposable : IDisposable
    {
        /// <summary>
        /// The delegate that is executed on disposal.
        /// </summary>
        private readonly Action disposeDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateDisposable"/> class.
        /// </summary>
        /// <param name="disposeDelegate">Action to execute when this instance is disposed.</param>
        public DelegateDisposable(Action disposeDelegate)
        {
            this.disposeDelegate = disposeDelegate ?? throw new ArgumentNullException(nameof(disposeDelegate));
        }

        /// <summary>
        /// Disposes this instance by executing the delegate.
        /// </summary>
        public void Dispose()
        {
            this.disposeDelegate();
        }
    }
}
