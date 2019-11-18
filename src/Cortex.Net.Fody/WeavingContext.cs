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
    using System.Collections.Generic;
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
        public WeavingContext(ModuleWeaver moduleWeaver)
        {
            this.CortexNetISharedState = TryResolveFromReference(moduleWeaver, "Cortex.Net.ISharedState", "Cortex.Net");
            this.CortexNetSharedState = TryResolveFromReference(moduleWeaver, "Cortex.Net.SharedState", "Cortex.Net");
            this.CortexNetApiActionAttribute = TryResolveFromReference(moduleWeaver, "Cortex.Net.Api.ActionAttribute", "Cortex.Net");
            this.CortexNetIReactiveObject = TryResolveFromReference(moduleWeaver, "Cortex.Net.IReactiveObject", "Cortex.Net");
            this.CortexNetApiActionExtensions = TryResolveFromReference(moduleWeaver, "Cortex.Net.Api.ActionExtensions", "Cortex.Net");
            this.CortexNetApiComputedAttribute = TryResolveFromReference(moduleWeaver, "Cortex.Net.Api.ComputedAttribute", "Cortex.Net");
            this.CortexNetTypesDeepEnhancer = TryResolveFromReference(moduleWeaver, "Cortex.Net.Types.DeepEnhancer", "Cortex.Net");
            this.CortexNetTypesObservableObject = TryResolveFromReference(moduleWeaver, "Cortex.Net.Types.ObservableObject", "Cortex.Net");
            this.CortexNetComputedValueOptions = TryResolveFromReference(moduleWeaver, "Cortex.Net.ComputedValueOptions`1", "Cortex.Net");
            this.CortexNetTypesObservableCollection = TryResolveFromReference(moduleWeaver, "Cortex.Net.Types.ObservableCollection`1", "Cortex.Net");
            this.CortexNetApiObservableAttribute = TryResolveFromReference(moduleWeaver, "Cortex.Net.Api.ObservableAttribute", "Cortex.Net");
            this.CortexNetCoreActionExtensions = TryResolveFromReference(moduleWeaver, "Cortex.Net.Core.ActionExtensions", "Cortex.Net");
            this.SystemRuntimeCompilerServicesCompilerGeneratedAttribute = TryResolveFromScannedAssemblies(moduleWeaver, "System.Runtime.CompilerServices.CompilerGeneratedAttribute");
            this.SystemDiagnosticsDebuggerBrowsableAttribute = TryResolveFromScannedAssemblies(moduleWeaver, "System.Diagnostics.DebuggerBrowsableAttribute");
            this.SystemAction = Enumerable.Range(0, 16).Select(x => TryResolveFromScannedAssemblies(moduleWeaver, x == 0 ? "System.Action" : $"System.Action`{x}")).ToList().AsReadOnly();
            this.SystemFunc = Enumerable.Range(0, 16).Select(x => TryResolveFromScannedAssemblies(moduleWeaver, $"System.Func`{x + 1}")).ToList().AsReadOnly();
        }

        /// <summary>
        /// Gets type reference to Cortex.Net.ISharedState.
        /// </summary>
        public TypeReference CortexNetISharedState { get; private set; }

        /// <summary>
        /// Gets type reference to Cortex.Net.ISharedState.
        /// </summary>
        public TypeReference CortexNetSharedState { get; private set; }

        /// <summary>
        /// Gets type reference to Cortex.Net.Api.ActionAttribute.
        /// </summary>
        public TypeReference CortexNetApiActionAttribute { get; private set; }

        /// <summary>
        /// Gets type reference to Cortex.Net.IReactiveObject.
        /// </summary>
        public TypeReference CortexNetIReactiveObject { get; private set; }

        /// <summary>
        /// Gets type reference to Cortex.Net.Api.ActionExtensions.
        /// </summary>
        public TypeReference CortexNetApiActionExtensions { get; private set; }

        /// <summary>
        /// Gets type reference to Cortex.Net.Api.ComputedAttribute.
        /// </summary>
        public TypeReference CortexNetApiComputedAttribute { get; private set; }

        /// <summary>
        /// Gets type reference to Cortex.Net.Types.DeepEnhancer.
        /// </summary>
        public TypeReference CortexNetTypesDeepEnhancer { get; private set; }

        /// <summary>
        /// Gets type reference to Cortex.Net.Types.ObservableObject.
        /// </summary>
        public TypeReference CortexNetTypesObservableObject { get; private set; }

        /// <summary>
        /// Gets type reference to Cortex.Net.ComputedValueOptions`1.
        /// </summary>
        public TypeReference CortexNetComputedValueOptions { get; private set; }

        /// <summary>
        /// Gets type reference to Cortex.Net.Types.ObservableCollection`1.
        /// </summary>
        public TypeReference CortexNetTypesObservableCollection { get; private set; }

        /// <summary>
        /// Gets type reference to Cortex.Net.Api.ObservableAttribute.
        /// </summary>
        public TypeReference CortexNetApiObservableAttribute { get; private set; }

        /// <summary>
        /// Gets type reference to Cortex.Net.Core.ActionExtensions.
        /// </summary>
        public TypeReference CortexNetCoreActionExtensions { get; private set; }

        /// <summary>
        /// Gets type reference to System.Runtime.CompilerServices.CompilerGeneratedAttribute.
        /// </summary>
        public TypeReference SystemRuntimeCompilerServicesCompilerGeneratedAttribute { get; private set; }

        /// <summary>
        /// Gets type reference to System.Diagnostics.DebuggerBrowsableAttribute.
        /// </summary>
        public TypeReference SystemDiagnosticsDebuggerBrowsableAttribute { get; private set; }

        /// <summary>
        /// Gets action type references.
        /// </summary>
        public IReadOnlyList<TypeReference> SystemAction { get; }

        /// <summary>
        /// Gets func type references.
        /// </summary>
        public IReadOnlyList<TypeReference> SystemFunc { get; }

        /// <summary>
        /// Tries to resolve a type from a preference.
        /// </summary>
        /// <param name="moduleWeaver">The module weaver to use.</param>
        /// <param name="fullName">The fullname of the type.</param>
        /// <param name="assemblyName">The assembly name.</param>
        /// <returns>A type reference.</returns>
        protected static TypeReference TryResolveFromReference(ModuleWeaver moduleWeaver, string fullName, string assemblyName)
        {
            if (moduleWeaver is null)
            {
                throw new System.ArgumentNullException(nameof(moduleWeaver));
            }

            try
            {
                var assembly = moduleWeaver.ModuleDefinition.AssemblyResolver.Resolve(moduleWeaver.ModuleDefinition.AssemblyReferences.FirstOrDefault(asm => asm.Name == assemblyName));
                return moduleWeaver.ModuleDefinition.ImportReference(assembly.MainModule.GetType(fullName));
            }
            catch
            {
                moduleWeaver.LogWarning(string.Format(CultureInfo.CurrentCulture, Resources.AssemblyOrTypeNotFoundReferences, fullName, assemblyName));
                throw;
            }
        }

        /// <summary>
        /// Tries to resolve a type from a preference.
        /// </summary>
        /// <param name="moduleWeaver">The module weaver to use.</param>
        /// <param name="fullName">The fullname of the type.</param>
        /// <returns>A type reference.</returns>
        protected static TypeReference TryResolveFromScannedAssemblies(ModuleWeaver moduleWeaver, string fullName)
        {
            if (moduleWeaver is null)
            {
                throw new System.ArgumentNullException(nameof(moduleWeaver));
            }

            try
            {
                var type = moduleWeaver.FindType(fullName);
                var result = moduleWeaver.ModuleDefinition.ImportReference(type);
                return result;
            }
            catch
            {
                moduleWeaver.LogWarning(string.Format(CultureInfo.CurrentCulture, Resources.AssemblyOrTypeNotFoundScan, fullName));
                throw;
            }
        }
    }
}
