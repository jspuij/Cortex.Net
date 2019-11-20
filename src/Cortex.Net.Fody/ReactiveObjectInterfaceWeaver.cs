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
    using System.Globalization;
    using System.Linq;
    using Cortex.Net.Fody.Properties;
    using global::Fody;
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
        private readonly ModuleWeaver parentWeaver;

        /// <summary>
        /// Weaving context.
        /// </summary>
        private readonly WeavingContext weavingContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReactiveObjectInterfaceWeaver"/> class.
        /// </summary>
        /// <param name="parentWeaver">The parent weaver.</param>
        /// <param name="weavingContext">The resolved types necessary by this weaver.</param>
        public ReactiveObjectInterfaceWeaver(ModuleWeaver parentWeaver, WeavingContext weavingContext)
        {
            this.parentWeaver = parentWeaver ?? throw new ArgumentNullException(nameof(parentWeaver));
            this.weavingContext = weavingContext ?? throw new ArgumentNullException(nameof(weavingContext));
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
                FieldDefinition backingField = null;

                var iReactiveObjectInterfaceType = this.weavingContext.CortexNetIReactiveObject;
                var iReactiveObjectinterfaceDefinition = new InterfaceImplementation(iReactiveObjectInterfaceType);
                var fieldTypeReference = this.weavingContext.CortexNetISharedState;

                // If this object does not implement IReactiveObject, add it, plus a default implementation.
                if (!reactiveObjectTypeDefinition.Interfaces.Any(x => x.InterfaceType.FullName == iReactiveObjectInterfaceType.FullName))
                {
                    var getOverride = iReactiveObjectInterfaceType.Resolve().Methods.Single(x => x.Name.Contains($"get_SharedState"));

                    reactiveObjectTypeDefinition.Interfaces.Add(iReactiveObjectinterfaceDefinition);

                    var methodAttributes = MethodAttributes.Private
                                              | MethodAttributes.Final
                                              | MethodAttributes.HideBySig
                                              | MethodAttributes.SpecialName
                                              | MethodAttributes.NewSlot
                                              | MethodAttributes.Virtual;

                    // add backing field for shared state to the class
                    backingField = reactiveObjectTypeDefinition.CreateBackingField(fieldTypeReference, "Cortex.Net.Api.IReactiveObject.SharedState", this.weavingContext);

                    // add getter
                    var getter = reactiveObjectTypeDefinition.CreateDefaultGetter(backingField, "Cortex.Net.Api.IReactiveObject.SharedState", this.weavingContext, methodAttributes);
                    getter.Overrides.Add(moduleDefinition.ImportReference(getOverride));

                    // add property
                    var propertyDefinition = reactiveObjectTypeDefinition.CreateProperty("Cortex.Net.Api.IReactiveObject.SharedState", getter);

                    // create a private setter.
                    var setter = reactiveObjectTypeDefinition.CreateDefaultSetter(backingField, "Cortex.Net.Api.IReactiveObject.SharedState", this.weavingContext, methodAttributes, p => ExecuteProcessorActions(p, backingField, processorActions));

                    foreach (var constructor in reactiveObjectTypeDefinition.Methods.Where(x => x.IsConstructor && !x.IsStatic))
                    {
                        if (!CallsOtherConstructor(constructor))
                        {
                            // call the setter from each constructor.
                            this.EmitConstructorCall(constructor.Body.GetILProcessor(), backingField, setter);
                        }
                    }
                }
                else
                {
                    try
                    {
                        var setOverride = reactiveObjectTypeDefinition.Resolve().Methods.Single(x => x.Name.Contains($"set_SharedState"));

                        if (setOverride != null)
                        {
                            backingField = reactiveObjectTypeDefinition.Fields.Single(x => x.Name.ToUpperInvariant().Contains("SHAREDSTATE") && x.FieldType.FullName == fieldTypeReference.FullName);

                            if (backingField != null)
                            {
                                // remove ret
                                setOverride.Body.Instructions.Remove(setOverride.Body.Instructions.Last());
                                var processor = setOverride.Body.GetILProcessor();
                                ExecuteProcessorActions(processor, backingField, processorActions);
                                processor.Emit(OpCodes.Ret);
                                continue;
                            }
                        }
                    }
#pragma warning disable CA1031 // Do not catch general exception types
                    catch
#pragma warning restore CA1031 // Do not catch general exception types
                    {
                        throw;
                    }

                    throw new WeavingException(string.Format(CultureInfo.CurrentCulture, Resources.DoNotMixAttributesAndIReactiveObject, reactiveObjectTypeDefinition.FullName));
                }
            }
        }

        /// <summary>
        /// Checks whether this constructor calls another constructor.
        /// </summary>
        /// <param name="constructor">The constructor to call.</param>
        /// <returns>Whether this constructor calls another constructor.</returns>
        private static bool CallsOtherConstructor(MethodDefinition constructor)
        {
            foreach (var instruction in constructor.Body.Instructions)
            {
                if (instruction.OpCode == OpCodes.Call ||
                    instruction.OpCode == OpCodes.Calli ||
                    instruction.OpCode == OpCodes.Callvirt)
                {
                    if (instruction.Operand is MethodReference methodReference)
                    {
                        var md = methodReference.Resolve();
                        if (md.IsConstructor && methodReference.DeclaringType == constructor.DeclaringType && !md.IsStatic)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the instruction that calls the base constructor.
        /// </summary>
        /// <param name="methodBody">The methodBody to search.</param>
        /// <param name="constructorType">The decalring type of the constructor.</param>
        /// <returns>The instruction that calls the base constructor.</returns>
        private static Instruction FindBaseConstructor(MethodBody methodBody, TypeDefinition constructorType)
        {
            foreach (var instruction in methodBody.Instructions)
            {
                if (instruction.OpCode == OpCodes.Call ||
                    instruction.OpCode == OpCodes.Calli ||
                    instruction.OpCode == OpCodes.Callvirt)
                {
                    if (instruction.Operand is MethodReference methodReference)
                    {
                        var md = methodReference.Resolve();
                        if (md.IsConstructor && !md.IsStatic)
                        {
                            var baseType = constructorType.BaseType;
                            while (baseType != null && baseType.FullName != methodReference.DeclaringType.FullName)
                            {
                                baseType = baseType.Resolve().BaseType;
                            }

                            if (baseType != null)
                            {
                                return instruction;
                            }
                        }
                    }
                }
            }

            return null;
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

        /// <summary>
        /// Executes the processoractions agains the processor.
        /// </summary>
        /// <param name="processor">The processor to use.</param>
        /// <param name="backingField">The backing field for the ISharedState" instance.</param>
        /// <param name="setterReference">A reference to the new private setter.</param>
        private void EmitConstructorCall(ILProcessor processor, FieldDefinition backingField, MethodReference setterReference)
        {
            var module = backingField.DeclaringType.Module;
            var resolveReference = module.ImportReference(this.weavingContext.CortexNetSharedState.Resolve().Methods.Single(x => x.Name == "ResolveState"));

            var instructions = new List<Instruction>()
            {
                processor.Create(OpCodes.Ldarg_0),
                processor.Create(OpCodes.Ldnull),
                processor.Create(OpCodes.Call, resolveReference),
                processor.Create(OpCodes.Callvirt, setterReference),
            };

            Instruction callBase = FindBaseConstructor(processor.Body, backingField.DeclaringType);

            foreach (var instruction in instructions.Reverse<Instruction>())
            {
                processor.InsertAfter(callBase, instruction);
            }
        }
    }
}
