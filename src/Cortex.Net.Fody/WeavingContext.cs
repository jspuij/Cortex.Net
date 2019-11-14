// <copyright file="WeavingContext.cs" company="Jan-Willem Spuij">
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
    using System.Globalization;
    using System.Linq;
    using Cortex.Net.Fody.Properties;
    using global::Fody;
    using Mono.Cecil;

    /// <summary>
    /// Context class for Weaving.
    /// </summary>
    public class WeavingContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WeavingContext"/> class.
        /// </summary>
        /// <param name="moduleWeaver">Moduleweaver to use.</param>
        public WeavingContext(CortexWeaver moduleWeaver)
        {
            this.CortexNetISharedState = this.TryResolveFromReference(moduleWeaver, "Cortex.Net.ISharedState", "Cortex.Net");
            this.CortexNetApiActionAttribute = this.TryResolveFromReference(moduleWeaver, "Cortex.Net.Api.ActionAttribute", "Cortex.Net");
            this.CortexNetIReactiveObject = this.TryResolveFromReference(moduleWeaver, "Cortex.Net.IReactiveObject", "Cortex.Net");
            this.CortexNetApiActionExtensions = this.TryResolveFromReference(moduleWeaver, "Cortex.Net.Api.ActionExtensions", "Cortex.Net");
            this.CortexNetApiComputedAttribute = this.TryResolveFromReference(moduleWeaver, "Cortex.Net.Api.ComputedAttribute", "Cortex.Net");
            this.CortexNetTypesDeepEnhancer = this.TryResolveFromReference(moduleWeaver, "Cortex.Net.Types.DeepEnhancer", "Cortex.Net");
            this.CortexNetTypesObservableObject = this.TryResolveFromReference(moduleWeaver, "Cortex.Net.Types.ObservableObject", "Cortex.Net");
            this.CortexNetComputedValueOptions = this.TryResolveFromReference(moduleWeaver, "Cortex.Net.ComputedValueOptions`1", "Cortex.Net");
            this.CortexNetTypesObservableCollection = this.TryResolveFromReference(moduleWeaver, "Cortex.Net.Types.ObservableCollection`1", "Cortex.Net");
            this.CortexNetApiObservableAttribute = this.TryResolveFromReference(moduleWeaver, "Cortex.Net.Api.ObservableAttribute", "Cortex.Net");
            this.CortexNetCoreActionExtensions = this.TryResolveFromReference(moduleWeaver, "Cortex.Net.Core.ActionExtensions", "Cortex.Net");
            this.SystemRuntimeCompilerServicesCompilerGeneratedAttribute = this.TryResolveFromScannedAssemblies(moduleWeaver, "System.Runtime.CompilerServices.CompilerGeneratedAttribute");
            this.SystemDiagnosticsDebuggerBrowsableAttribute = this.TryResolveFromScannedAssemblies(moduleWeaver, "System.Diagnostics.DebuggerBrowsableAttribute");
        }

        /// <summary>
        /// Gets type reference to Cortex.Net.ISharedState.
        /// </summary>
        internal TypeReference CortexNetISharedState { get; private set; }

        /// <summary>
        /// Gets type reference to Cortex.Net.Api.ActionAttribute.
        /// </summary>
        internal TypeReference CortexNetApiActionAttribute { get; private set; }

        /// <summary>
        /// Gets type reference to Cortex.Net.IReactiveObject.
        /// </summary>
        internal TypeReference CortexNetIReactiveObject { get; private set; }

        /// <summary>
        /// Gets type reference to Cortex.Net.Api.ActionExtensions.
        /// </summary>
        internal TypeReference CortexNetApiActionExtensions { get; private set; }

        /// <summary>
        /// Gets type reference to Cortex.Net.Api.ComputedAttribute.
        /// </summary>
        internal TypeReference CortexNetApiComputedAttribute { get; private set; }

        /// <summary>
        /// Gets type reference to Cortex.Net.Types.DeepEnhancer.
        /// </summary>
        internal TypeReference CortexNetTypesDeepEnhancer { get; private set; }

        /// <summary>
        /// Gets type reference to Cortex.Net.Types.ObservableObject.
        /// </summary>
        internal TypeReference CortexNetTypesObservableObject { get; private set; }

        /// <summary>
        /// Gets type reference to Cortex.Net.ComputedValueOptions`1.
        /// </summary>
        internal TypeReference CortexNetComputedValueOptions { get; private set; }

        /// <summary>
        /// Gets type reference to Cortex.Net.Types.ObservableCollection`1.
        /// </summary>
        internal TypeReference CortexNetTypesObservableCollection { get; private set; }

        /// <summary>
        /// Gets type reference to Cortex.Net.Api.ObservableAttribute.
        /// </summary>
        internal TypeReference CortexNetApiObservableAttribute { get; private set; }

        /// <summary>
        /// Gets type reference to Cortex.Net.Core.ActionExtensions.
        /// </summary>
        internal TypeReference CortexNetCoreActionExtensions { get; private set; }

        /// <summary>
        /// Gets type reference to System.Runtime.CompilerServices.CompilerGeneratedAttribute.
        /// </summary>
        internal TypeReference SystemRuntimeCompilerServicesCompilerGeneratedAttribute { get; private set; }

        /// <summary>
        /// Gets type reference to System.Diagnostics.DebuggerBrowsableAttribute.
        /// </summary>
        internal TypeReference SystemDiagnosticsDebuggerBrowsableAttribute { get; private set; }

        /// <summary>
        /// Tries to resolve a type from a preference.
        /// </summary>
        /// <param name="moduleWeaver">The module weaver to use.</param>
        /// <param name="fullName">The fullname of the type.</param>
        /// <param name="assemblyName">The assembly name.</param>
        /// <returns>A type reference.</returns>
        protected TypeReference TryResolveFromReference(CortexWeaver moduleWeaver, string fullName, string assemblyName)
        {
            try
            {
                var assembly = moduleWeaver.ModuleDefinition.AssemblyResolver.Resolve(moduleWeaver.ModuleDefinition.AssemblyReferences.FirstOrDefault(asm => asm.Name == assemblyName));
                return moduleWeaver.ModuleDefinition.ImportReference(assembly.MainModule.GetType(fullName));
            }
            catch
            {
                throw new WeavingException(string.Format(CultureInfo.CurrentCulture, Resources.AssemblyOrTypeNotFound, fullName));
            }
        }

        /// <summary>
        /// Tries to resolve a type from a preference.
        /// </summary>
        /// <param name="moduleWeaver">The module weaver to use.</param>
        /// <param name="fullName">The fullname of the type.</param>
        /// <returns>A type reference.</returns>
        protected TypeReference TryResolveFromScannedAssemblies(CortexWeaver moduleWeaver, string fullName)
        {
            try
            {
                return moduleWeaver.ModuleDefinition.ImportReference(moduleWeaver.FindStandardType(fullName));
            }
            catch
            {
                throw new WeavingException(string.Format(CultureInfo.CurrentCulture, Resources.AssemblyOrTypeNotFound, fullName));
            }
        }
    }
}
