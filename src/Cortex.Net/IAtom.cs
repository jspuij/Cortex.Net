// <copyright file="IAtom.cs" company="Michel Weststrate, Jan-Willem Spuij">
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

    /// <summary>
    /// Interface that defines an Atom.
    /// Atoms can be used to signal Cortex.Net that some observable data source has been observed or changed.
    /// And Cortex.Net will signal the atom whenever it is used or no longer in use.
    /// </summary>
    public interface IAtom : IObservable
    {
        /// <summary>
        /// Invoke this method to notify Cortex.Net that your atom has been used somehow.
        /// </summary>
        /// <returns>Returns true if there is currently a reactive context.</returns>
        bool ReportObserved();

        /// <summary>
        /// Invoke this method after this atom has changed to signal Cortex.Net that all its observers should invalidate.
        /// </summary>
        void ReportChanged();
    }
}
