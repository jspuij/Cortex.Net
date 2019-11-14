// <copyright file="ReactiveObjectInterfaceWeaver.cs" company="Jan-Willem Spuij">
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
    using System.Linq;
    using Mono.Cecil;
    using Mono.Cecil.Cil;

    /// <summary>
    /// Weaves the implementation for IReactiveObject on an object.
    /// </summary>
    public class ReactiveObjectInterfaceWeaver : ISharedStateAssignmentILProcessorQueue
    {
        /// <summary>
        /// A reference to the parent Cortex.Net weaver.
        /// </summary>
        private readonly CortexWeaver parentWeaver;

        /// <summary>
        /// A type reference to the Cortex.Net.ISharedState type.
        /// </summary>
        private readonly TypeReference iSharedStateReference;

        /// <summary>
        /// A type reference to the Cortex.Net.IReactiveObject type.
        /// </summary>
        private readonly TypeReference iReactiveObjectReference;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReactiveObjectInterfaceWeaver"/> class.
        /// </summary>
        /// <param name="parentWeaver">The parent weaver.</param>
        /// <param name="resolvedTypes">The resolved types necessary by this weaver.</param>
        public ReactiveObjectInterfaceWeaver(CortexWeaver parentWeaver, IDictionary<string, TypeReference> resolvedTypes)
        {
            if (resolvedTypes is null)
            {
                throw new ArgumentNullException(nameof(resolvedTypes));
            }

            this.parentWeaver = parentWeaver ?? throw new ArgumentNullException(nameof(parentWeaver));
            this.iSharedStateReference = resolvedTypes["Cortex.Net.ISharedState"];
            this.iReactiveObjectReference = resolvedTypes["Cortex.Net.IReactiveObject"];
        }

        /// <summary>
        /// Gets a Queue with delegates to be executed to emit the IL code on sharedStateAssignment.
        /// </summary>
        public Queue<(TypeDefinition, bool, Action<ILProcessor, FieldReference>)> SharedStateAssignmentQueue { get; } = new Queue<(TypeDefinition, bool, Action<ILProcessor, FieldReference>)>();

        /// <summary>
        /// Executes this weaver.
        /// </summary>
        internal void Execute()
        {
            var moduleDefinition = this.parentWeaver.ModuleDefinition;

            // sort queue contents by object, and group by type.
            var queueContent = from q in this.SharedStateAssignmentQueue.ToList()
                               group q by q.Item1 into g
                               select (g.Key, g.Any(x => x.Item2), g.Select(x => x.Item3));

            this.SharedStateAssignmentQueue.Clear();

            foreach ((TypeDefinition reactiveObjectTypeDefinition, bool addInjectAttribute, IEnumerable<Action<ILProcessor, FieldReference>> processorActions) in queueContent)
            {
                var iReactiveObjectInterfaceType = this.iReactiveObjectReference;
                var iReactiveObjectinterfaceDefinition = new InterfaceImplementation(iReactiveObjectInterfaceType);

                // If this object does not implement IReactiveObject, add it, plus a default implementation.
                if (!reactiveObjectTypeDefinition.Interfaces.Contains(iReactiveObjectinterfaceDefinition))
                {
                    var getOverride = iReactiveObjectInterfaceType.Resolve().Methods.Single(x => x.Name.Contains($"get_SharedState"));
                    var setOverride = iReactiveObjectInterfaceType.Resolve().Methods.Single(x => x.Name.Contains($"set_SharedState"));

                    reactiveObjectTypeDefinition.Interfaces.Add(iReactiveObjectinterfaceDefinition);

                    var methodAttributes = MethodAttributes.Private
                                              | MethodAttributes.Final
                                              | MethodAttributes.HideBySig
                                              | MethodAttributes.SpecialName
                                              | MethodAttributes.NewSlot
                                              | MethodAttributes.Virtual;

                    var fieldTypeReference = this.iSharedStateReference;

                    // add backing field for shared state to the class
                    var backingField = reactiveObjectTypeDefinition.CreateBackingField(fieldTypeReference, "Cortex.Net.Api.IReactiveObject.SharedState");

                    // add getter
                    var getter = reactiveObjectTypeDefinition.CreateDefaultGetter(backingField, "Cortex.Net.Api.IReactiveObject.SharedState", methodAttributes);
                    getter.Overrides.Add(moduleDefinition.ImportReference(getOverride));

                    // add setter
                    var setter = reactiveObjectTypeDefinition.CreateDefaultSetter(backingField, "Cortex.Net.Api.IReactiveObject.SharedState", methodAttributes, p => ExecuteProcessorActions(p, backingField, processorActions));
                    setter.Overrides.Add(moduleDefinition.ImportReference(setOverride));

                    // add property
                    var propertyDefinition = reactiveObjectTypeDefinition.CreateProperty("Cortex.Net.Api.IReactiveObject.SharedState", getter, setter);
                    if (addInjectAttribute)
                    {
                        // add Inject attribute.
                        AddInjectAttribute(moduleDefinition, propertyDefinition);
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }

        /// <summary>
        /// Adds an inject attribute to a property.
        /// </summary>
        /// <param name="moduleDefinition">The moduleDefinition to use.</param>
        /// <param name="propertyDefinition">The property Definition to use.</param>
        private static void AddInjectAttribute(ModuleDefinition moduleDefinition, PropertyDefinition propertyDefinition)
        {
            var assemblyNameReference = moduleDefinition.AssemblyReferences.First(x => x.Name == "Microsoft.AspNetCore.Components");
            AssemblyDefinition foo = moduleDefinition.AssemblyResolver.Resolve(assemblyNameReference);
            var typeReference = foo.MainModule.GetType("Microsoft.AspNetCore.Components.InjectAttribute");
            var injectAttributeReference = moduleDefinition.ImportReference(typeReference);
            var ctor = injectAttributeReference.Resolve().Methods.Single(x => x.IsConstructor);
            var ctorRef = moduleDefinition.ImportReference(ctor);
            var injectAttribute = new CustomAttribute(ctorRef, new byte[] { 01, 00, 00, 00 });
            propertyDefinition.CustomAttributes.Add(injectAttribute);
        }

        /// <summary>
        /// Executes the processoractions agains the processor.
        /// </summary>
        /// <param name="processor">The processor to use.</param>
        /// <param name="backingField">The backing field for the ISharedState" instance.</param>
        /// <param name="processorActions">The processor actions to execute.</param>
        private static void ExecuteProcessorActions(ILProcessor processor, FieldDefinition backingField, IEnumerable<Action<ILProcessor, FieldReference>> processorActions)
        {
            // if (value != null)
            processor.Emit(OpCodes.Ldarg_1);
            var nop = processor.Create(OpCodes.Nop);
            processor.Emit(OpCodes.Brtrue_S, nop);
            processor.Emit(OpCodes.Ret);
            processor.Append(nop);

            foreach (var action in processorActions)
            {
                action(processor, backingField);
            }
        }
    }
}
