// <copyright file="ObservableObjectInterfaceWeaver.cs" company="Jan-Willem Spuij">
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
    using System.Text;
    using Cortex.Net.Api;
    using Mono.Cecil;
    using Mono.Cecil.Cil;

    /// <summary>
    /// Weaves the implementation for <see cref="IObservableObject"/> on an object.
    /// </summary>
    public class ObservableObjectInterfaceWeaver : ISharedStateAssignmentILProcessorQueue
    {
        /// <summary>
        /// A reference to the parent Cortex.Net weaver.
        /// </summary>
        private readonly CortexWeaver cortexWeaver;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableObjectInterfaceWeaver"/> class.
        /// </summary>
        /// <param name="cortexWeaver">The cortex weaver.</param>
        public ObservableObjectInterfaceWeaver(CortexWeaver cortexWeaver)
        {
            this.cortexWeaver = cortexWeaver ?? throw new ArgumentNullException(nameof(cortexWeaver));
        }

        /// <summary>
        /// Gets a Queue with delegates to be executed to emit the IL code on sharedStateAssignment.
        /// </summary>
        public Queue<(TypeDefinition, Action<ILProcessor, FieldReference>)> SharedStateAssignmentQueue { get; } = new Queue<(TypeDefinition, Action<ILProcessor, FieldReference>)>();

        /// <summary>
        /// Executes this weaver.
        /// </summary>
        internal void Execute()
        {
            var moduleDefinition = this.cortexWeaver.ModuleDefinition;

            // sort queue contents by object, and gro
            var queueContent = from q in this.SharedStateAssignmentQueue.ToList()
                               group q by q.Item1 into g
                               select (g.Key, g.Select(x => x.Item2));

            this.SharedStateAssignmentQueue.Clear();

            foreach ((TypeDefinition observableObjectTypeDefinition, IEnumerable<Action<ILProcessor, FieldReference>> processorActions) in queueContent)
            {
                var iObservableObjectInterfaceType = moduleDefinition.ImportReference(typeof(IObservableObject));
                var iObservableObjectinterfaceDefinition = new InterfaceImplementation(iObservableObjectInterfaceType);

                // If this object does not implement IObservableObject, add it, plus a default implementation.
                if (!observableObjectTypeDefinition.Interfaces.Contains(iObservableObjectinterfaceDefinition))
                {
                    var getOverride = iObservableObjectInterfaceType.Resolve().Methods.Single(x => x.Name.Contains($"get_SharedState"));
                    var setOverride = iObservableObjectInterfaceType.Resolve().Methods.Single(x => x.Name.Contains($"set_SharedState"));

                    observableObjectTypeDefinition.Interfaces.Add(iObservableObjectinterfaceDefinition);

                    var methodAttributes = MethodAttributes.Private
                                           | MethodAttributes.Final
                                           | MethodAttributes.HideBySig
                                           | MethodAttributes.SpecialName
                                           | MethodAttributes.NewSlot
                                           | MethodAttributes.Virtual;

                    var fieldTypeReference = moduleDefinition.ImportReference(typeof(ISharedState));

                    // add backing field for shared state to the class
                    var backingField = observableObjectTypeDefinition.CreateBackingField(fieldTypeReference, "Cortex.Net.Api.IObservableObject.SharedState");

                    // add getter
                    var getter = observableObjectTypeDefinition.CreateDefaultGetter(backingField, "Cortex.Net.Api.IObservableObject.SharedState", methodAttributes);
                    getter.Overrides.Add(moduleDefinition.ImportReference(getOverride));

                    // add setter
                    var setter = observableObjectTypeDefinition.CreateDefaultSetter(backingField, "Cortex.Net.Api.IObservableObject.SharedState", methodAttributes, p => ExecuteProcessorActions(p, backingField, processorActions));
                    setter.Overrides.Add(moduleDefinition.ImportReference(setOverride));

                    // add property
                    observableObjectTypeDefinition.CreateProperty("Cortex.Net.Api.IObservableObject.SharedState", getter, setter);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }

        /// <summary>
        /// Executes the processoractions agains the processor.
        /// </summary>
        /// <param name="processor">The processor to use.</param>
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
