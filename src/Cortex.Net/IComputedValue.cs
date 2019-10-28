// <copyright file="IComputedValue.cs" company="Michel Weststrate, Jan-Willem Spuij">
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
    /// <summary>
    /// Interface for Computed values.
    /// </summary>
    /// <typeparam name="T">The type of the computed value.</typeparam>
    public interface IComputedValue<T> : IComputedValue
    {
        /// <summary>
        /// Gets or sets the underlying value.
        /// </summary>
        new T Value { get; set; }
    }

    /// <summary>
    /// Interface for Computed values.
    /// </summary>
    public interface IComputedValue
    {
        /// <summary>
        /// Gets or sets the underlying value.
        /// Best to be implemented specifically.
        /// </summary>
        object Value { get; set; }

        /// <summary>
        /// Suspends computation of this computed value when the last observer leaves.
        /// Computed values are automatically teared down when the last observer leaves.
        /// This process happens recursively, this computed might be the last observabe of another, etc.
        /// </summary>
        void Suspend();
    }
}
