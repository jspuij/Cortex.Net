// <copyright file="CortexWeaver.cs" company="Jan-Willem Spuij">
// Copyright 2019 Jan-Willem Spuij
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

namespace Cortex.Net.Fody
{
    using System;
    using System.Collections.Generic;
    using global::Fody;

    /// <summary>
    /// Orchestrates weaving of classes with Cortex.Net Observables, Actions and Reactions.
    /// </summary>
    public class CortexWeaver : BaseModuleWeaver
    {
        /// <summary>
        /// Executes the <see cref="CortexWeaver"/>.
        /// </summary>
        public override void Execute()
        {
            var observableObjectWeaver = new ObservableObjectInterfaceWeaver(this);
            var enumerableWeaver = new EnumerableInterfaceWeaver(this, observableObjectWeaver);
            var actionWeaver = new ActionWeaver(this, observableObjectWeaver);
            var observableWeaver = new ObservableWeaver(this, enumerableWeaver, observableObjectWeaver);
            var computedWeaver = new ComputedWeaver(this, observableObjectWeaver);
            actionWeaver.Execute();
            observableWeaver.Execute();
            computedWeaver.Execute();
            observableObjectWeaver.Execute();
        }

        /// <summary>
        /// Return a list of assembly names for scanning. Used as a list for Fody.BaseModuleWeaver.FindType.
        /// </summary>
        /// <returns>All types in the references assembly.</returns>
        public override IEnumerable<string> GetAssembliesForScanning()
        {
            return new string[] { "System.Runtime" };
        }
    }
}
