// <copyright file="AutorunOptions.cs" company="Michel Weststrate, Jan-Willem Spuij">
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
    using System.Threading.Tasks;
    using Cortex.Net.Core;

    /// <summary>
    /// Specifies the options for an Autorun instance.
    /// </summary>
    public class AutorunOptions
    {
        /// <summary>
        /// Gets or sets the delay in miliseconds.
        /// </summary>
        public int Delay { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether observables are required in the view function.
        /// </summary>
        public bool RequiresObservable { get; set; }

        /// <summary>
        /// Gets or sets the scheduler function to use. The function is async
        /// so that other code can be executed in the mean time.
        /// </summary>
        public Func<Task> Scheduler { get; set; }

        /// <summary>
        /// Gets or sets error handler function that is called in case of an error. Otherwise the error is propagated.
        /// </summary>
        public Action<Reaction, Exception> ErrorHandler { get; set; }

        /// <summary>
        /// Gets or sets the context.
        /// </summary>
        public object Context { get; set; }
    }
}
