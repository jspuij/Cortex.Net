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
    using System.Globalization;
    using System.Linq;
    using Cortex.Net.Fody.Properties;
    using global::Fody;
    using Mono.Cecil;

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
            IDictionary<string, TypeReference> cortexNetTypes;
            IDictionary<string, TypeReference> cortexNetBlazorTypes;

            try
            {
                cortexNetTypes = new TypeResolverDictionary(this.ModuleDefinition, new Dictionary<string, string>()
                {
                    { "Cortex.Net.ISharedState", "Cortex.Net" },
                    { "Cortex.Net.Api.ActionAttribute", "Cortex.Net" },
                    { "Cortex.Net.IReactiveObject", "Cortex.Net" },
                    { "Cortex.Net.Api.ActionExtensions", "Cortex.Net" },
                    { "Cortex.Net.Api.ComputedAttribute", "Cortex.Net" },
                    { "Cortex.Net.Types.DeepEnhancer", "Cortex.Net" },
                    { "Cortex.Net.Types.ObservableObject", "Cortex.Net" },
                    { "Cortex.Net.ComputedValueOptions`1", "Cortex.Net" },
                    { "Cortex.Net.Types.ObservableCollection`1", "Cortex.Net" },
                    { "Cortex.Net.Api.ObservableAttribute", "Cortex.Net" },
                    { "Cortex.Net.Core.ActionExtensions", "Cortex.Net" },
                });
            }
            catch
            {
                throw new WeavingException(string.Format(CultureInfo.CurrentCulture, Resources.AssemblyOrTypeNotFound, "Cortex.Net"));
            }

            var reactiveObjectInterfaceWeaver = new ReactiveObjectInterfaceWeaver(this, cortexNetTypes);

            var enumerableWeaver = new EnumerableInterfaceWeaver(this, reactiveObjectInterfaceWeaver, cortexNetTypes);
            var actionWeaver = new ActionWeaver(this, reactiveObjectInterfaceWeaver, cortexNetTypes);
            var observableWeaver = new ObservableWeaver(this, enumerableWeaver, reactiveObjectInterfaceWeaver, cortexNetTypes);
            var computedWeaver = new ComputedWeaver(this, reactiveObjectInterfaceWeaver, cortexNetTypes);

            actionWeaver.Execute();
            observableWeaver.Execute();
            computedWeaver.Execute();

            if (this.ModuleDefinition.AssemblyReferences.Any(x => x.Name == "Cortex.Net.Blazor"))
            {
                try
                {
                    cortexNetBlazorTypes = new TypeResolverDictionary(this.ModuleDefinition, new Dictionary<string, string>()
                {
                    { "Cortex.Net.Blazor.ObserverAttribute", "Cortex.Net.Blazor" },
                    { "Cortex.Net.Blazor.ObserverObject", "Cortex.Net.Blazor" },
                });
                }
                catch
                {
                    throw new WeavingException(string.Format(CultureInfo.CurrentCulture, Resources.AssemblyOrTypeNotFound, "Cortex.Net.Blazor"));
                }

                var blazorObserverWeaver = new BlazorObserverWeaver(this, reactiveObjectInterfaceWeaver, cortexNetBlazorTypes);
                blazorObserverWeaver.Execute();
            }

            reactiveObjectInterfaceWeaver.Execute();
        }

        /// <summary>
        /// Return a list of assembly names for scanning. Used as a list for Fody.BaseModuleWeaver.FindType.
        /// </summary>
        /// <returns>All types in the references assembly.</returns>
        public override IEnumerable<string> GetAssembliesForScanning()
        {
            return new string[] { "System.Runtime", "Microsoft.AspnetCore.Components" };
        }
    }
}
