// <copyright file="BlazorObserverWeaver.cs" company="Jan-Willem Spuij">
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
    using System.IO;
    using System.Linq;
    using global::Fody;
    using Mono.Cecil;
    using Mono.Cecil.Cil;

    /// <summary>
    /// Weaver for Blazor components decorated with the ObserverAttribute class.
    /// </summary>
    public class BlazorObserverWeaver
    {
        /// <summary>
        /// The prefix for an inner method that is the target of an action.
        /// </summary>
        private const string InnerCounterFieldPrefix = "cortex_Net_Fvclsnf97SxcMxlkizajkz_";

        /// <summary>
        /// The full name of the ComponentBasetype.
        /// </summary>
        private const string ComponentBaseTypeName = "Microsoft.AspNetCore.Components.ComponentBase";

        /// <summary>
        /// The name of an Inner ObserverObject field.
        /// </summary>
        private const string InnerObserverObjectFieldName = "cortex_Net_H90skjHYJKq9_ObserverObject";

        /// <summary>
        /// The processor queue.
        /// </summary>
        private readonly ISharedStateAssignmentILProcessorQueue processorQueue;

        /// <summary>
        /// Type reference to ObserverAttribute.
        /// </summary>
        private readonly TypeReference observerAttributeReference;

        /// <summary>
        /// Type reference to ObserverObject.
        /// </summary>
        private readonly TypeReference observerObjectReference;

        /// <summary>
        /// The parent weaver.
        /// </summary>
        private readonly BaseModuleWeaver parentWeaver;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlazorObserverWeaver"/> class.
        /// </summary>
        /// <param name="parentWeaver">The parent weaver of this weaver.</param>
        /// <param name="processorQueue">The processor queue to add delegates to, to be executed on ISharedState property assignment.</param>
        /// <param name="resolvedTypes">The resolved types necessary by this weaver.</param>
        public BlazorObserverWeaver(BaseModuleWeaver parentWeaver, ISharedStateAssignmentILProcessorQueue processorQueue, IDictionary<string, TypeReference> resolvedTypes)
        {
            if (resolvedTypes is null)
            {
                throw new ArgumentNullException(nameof(resolvedTypes));
            }

            this.parentWeaver = parentWeaver ?? throw new ArgumentNullException(nameof(parentWeaver));
            this.processorQueue = processorQueue ?? throw new ArgumentNullException(nameof(processorQueue));
            this.observerAttributeReference = resolvedTypes["Cortex.Net.Blazor.ObserverAttribute"];
            this.observerObjectReference = resolvedTypes["Cortex.Net.Blazor.ObserverObject"];
        }

        /// <summary>
        /// Executes this weaver.
        /// </summary>
        internal void Execute()
        {
            var decoratedClasses = from t in this.parentWeaver.ModuleDefinition.GetTypes()
                                   where
                                      t != null &&
                                      t.IsClass &&
                                      t.BaseType != null &&
                                      t.CustomAttributes != null &&
                                      t.CustomAttributes.Any(x => x.AttributeType.FullName == this.observerAttributeReference.FullName)
                                   select t;

            foreach (var decoratedClass in decoratedClasses.ToList())
            {
                var baseType = decoratedClass.BaseType.Resolve();

                while (baseType != null)
                {
                    if (baseType.FullName == ComponentBaseTypeName)
                    {
                        this.WeaveClass(decoratedClass);
                    }

                    baseType = baseType.BaseType?.Resolve();
                }
            }
        }

        /// <summary>
        /// Weave the ComponentBase derived class.
        /// </summary>
        /// <param name="decoratedClass">The derived class to weave.</param>
        private void WeaveClass(TypeDefinition decoratedClass)
        {
            var module = decoratedClass.Module;
            var decoratedType = decoratedClass.Resolve();

            var componentBaseType = decoratedType;

            var stateHasChangedMethod = componentBaseType.Methods.FirstOrDefault(x => x.Name == "StateHasChanged");
            var buildRenderTreeMethod = componentBaseType.Methods.FirstOrDefault(x => x.Name == "BuildRenderTree" && x.Parameters.Count == 1);

            while (stateHasChangedMethod == null)
            {
                if (componentBaseType.BaseType != null)
                {
                    componentBaseType = componentBaseType.BaseType.Resolve();
                    stateHasChangedMethod = componentBaseType.Methods.FirstOrDefault(x => x.Name == "StateHasChanged");
                }
                else
                {
                    this.parentWeaver.LogWarning("StateHasChanged not found");
                    return;
                }
            }

            var observerObjectType = this.observerObjectReference;
            var innerObserverObjectField = decoratedType.CreateField(observerObjectType, InnerObserverObjectFieldName);

            // observerName name
            var observerName = decoratedClass.Name;
            var observerAttribute = decoratedClass.CustomAttributes.SingleOrDefault(x => x.AttributeType.FullName == this.observerAttributeReference.FullName);

            if (observerAttribute != null)
            {
                foreach (var constructorArgument in observerAttribute.ConstructorArguments)
                {
                    if (constructorArgument.Type.FullName == typeof(string).FullName)
                    {
                        observerName = constructorArgument.Value as string;
                    }
                }
            }

            this.processorQueue.SharedStateAssignmentQueue.Enqueue((decoratedClass, true, (processor, sharedStateBackingField) => this.EmitObserverObjectInit(
                processor,
                observerName,
                innerObserverObjectField,
                buildRenderTreeMethod,
                stateHasChangedMethod)));

            // add entrance counter field.
            var entranceCounterDefinition = decoratedType.CreateField(module.TypeSystem.Int32, $"{InnerCounterFieldPrefix}BuildRenderTree_EntranceCount", FieldAttributes.Private);

            // Weave the render tree method.
            this.WeaveBuildRenderTreeMethod(buildRenderTreeMethod, observerObjectType, innerObserverObjectField, entranceCounterDefinition);

            // Add a dispose method.
            this.AddDispose(decoratedType, observerObjectType, innerObserverObjectField);
        }

        /// <summary>
        /// Adds a Dispose method to the Component.
        /// </summary>
        /// <param name="decoratedType">The type of the observer decorated component.</param>
        /// <param name="observerObjectType">The type of the internal observer object.</param>
        /// <param name="innerObserverObjectField">The field of the internal observer object.</param>
        private void AddDispose(TypeDefinition decoratedType, TypeReference observerObjectType, FieldDefinition innerObserverObjectField)
        {
            var module = this.parentWeaver.ModuleDefinition;

            var disposeMethodDefinition = decoratedType.Methods.Where(x => x.Name == "Dispose").OrderBy(x => x.Parameters.Count).FirstOrDefault();

            bool isNew = false;

            // add Dispose method.
            if (disposeMethodDefinition == null)
            {
                disposeMethodDefinition = new MethodDefinition("Dispose", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual, module.TypeSystem.Void);
                decoratedType.Methods.Add(disposeMethodDefinition);
                isNew = true;
            }

            var disposeReference = module.ImportReference(observerObjectType.Resolve().Methods.Single(x => x.Name == "Dispose"));

            var processor = disposeMethodDefinition.Body.GetILProcessor();

            if (isNew)
            {
                processor.Emit(OpCodes.Ret);
            }

            var originalStart = processor.Body.Instructions.First();

            var instructions = new List<Instruction>
            {
                processor.Create(OpCodes.Ldarg_0),
                processor.Create(OpCodes.Ldfld, innerObserverObjectField),
                processor.Create(OpCodes.Brfalse_S, originalStart),
                processor.Create(OpCodes.Ldarg_0),
                processor.Create(OpCodes.Ldfld, innerObserverObjectField),
                processor.Create(OpCodes.Callvirt, disposeReference),
            };

            foreach (var instruction in instructions)
            {
                processor.InsertBefore(originalStart, instruction);
            }
        }

        /// <summary>
        /// Emits IL code for assignment of the inner observer object field.
        /// </summary>
        /// <param name="processor">The processor to use.</param>
        /// <param name="observerName">The name of the observer.</param>
        /// <param name="innerObserverObjectField">The inner observer object field to assign.</param>
        /// <param name="buildRenderTreeMethod">The buildRenderTreeMethod on the compoent.</param>
        /// <param name="stateChangedMethod">The method to call to set the state to changed.</param>
        private void EmitObserverObjectInit(ILProcessor processor, string observerName, FieldDefinition innerObserverObjectField, MethodDefinition buildRenderTreeMethod, MethodDefinition stateChangedMethod)
        {
            var module = this.parentWeaver.ModuleDefinition;

            var observableObjectConstructor = module.ImportReference(this.parentWeaver.ModuleDefinition.ImportReference(this.observerObjectReference).Resolve().Methods.Single(x => x.IsConstructor));
            var buildRenderTreeReference = module.ImportReference(buildRenderTreeMethod);
            var stateChangedReference = module.ImportReference(stateChangedMethod);

            var renderActionType = buildRenderTreeMethod.GetActionType();
            MethodReference renderActionConstructorType = renderActionType.Resolve().Methods.Single(x => x.IsConstructor);
            var renderActionConstructorReference = module.ImportReference(renderActionConstructorType.GetGenericMethodOnInstantance(renderActionType));

            var stateChangedActionType = stateChangedMethod.GetActionType();
            MethodReference stateChangedActionConstructorType = stateChangedActionType.Resolve().Methods.Single(x => x.IsConstructor);
            var stateChangedActionConstructorReference = module.ImportReference(stateChangedActionConstructorType);

            var instructions = new List<Instruction>
            {
                // this.observerObject = new ObserverObject(sharedState, name, (Action<RenderTreeBuilder>)buildRenderTreeAction, (Action)stateChangedAction);
                processor.Create(OpCodes.Ldarg_0),
                processor.Create(OpCodes.Ldarg_1),
                processor.Create(OpCodes.Ldstr, observerName),
                processor.Create(OpCodes.Ldarg_0),
                processor.Create(OpCodes.Dup),
                processor.Create(OpCodes.Ldvirtftn, buildRenderTreeReference),
                processor.Create(OpCodes.Newobj, renderActionConstructorReference),
                processor.Create(OpCodes.Ldarg_0),
                processor.Create(OpCodes.Ldftn, stateChangedReference),
                processor.Create(OpCodes.Newobj, stateChangedActionConstructorReference),
                processor.Create(OpCodes.Newobj, observableObjectConstructor),
                processor.Create(OpCodes.Stfld, innerObserverObjectField),
            };

            foreach (var instruction in instructions)
            {
                processor.Append(instruction);
            }
        }

        /// <summary>
        /// Weaves the BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder) Method.
        /// </summary>
        /// <param name="buildRenderTreeMethod">The method definition of the method to weave.</param>
        /// <param name="observerObjectType">The type of the inner observer object.</param>
        /// <param name="observerObjectDefinition">The field definition of the observer object.</param>
        /// <param name="counterFieldDefinition">The field definition of the entrance counter field.</param>
        private void WeaveBuildRenderTreeMethod(MethodDefinition buildRenderTreeMethod, TypeReference observerObjectType, FieldDefinition observerObjectDefinition, FieldDefinition counterFieldDefinition)
        {
            var module = this.parentWeaver.ModuleDefinition;

            var getTypeReference = module.ImportReference(typeof(object)).Resolve().Methods.Single(x => x.Name == "GetType" && x.Parameters.Count == 0 && !x.IsStatic);
            var getFullNameReference = module.ImportReference(typeof(Type)).Resolve().Methods.Single(x => x.Name == "get_FullName" && x.Parameters.Count == 0 && !x.IsStatic);
            var writeLineReference = module.ImportReference(typeof(Console)).Resolve().Methods.Single(x => x.Name == "WriteLine" && x.Parameters.Count == 1 && x.IsStatic && x.Parameters[0].ParameterType.FullName == typeof(string).FullName);

            var processor = buildRenderTreeMethod.Body.GetILProcessor();

            var originalStart = buildRenderTreeMethod.Body.Instructions.First();
            var originalEnd = buildRenderTreeMethod.Body.Instructions.Last();

            var prefix = new List<Instruction>
            {
                // if fieldDefinition == null, jump to originalStart.
                processor.Create(OpCodes.Ldarg_0),
                processor.Create(OpCodes.Ldfld, observerObjectDefinition),
                processor.Create(OpCodes.Brfalse_S, originalStart),

                // this pointers for later store and refetch. This bypasses local variable declarations that may not play nice with existing local variables.
                processor.Create(OpCodes.Ldarg_0),
                processor.Create(OpCodes.Ldarg_0),

                // load counterfield definition and add 1
                processor.Create(OpCodes.Ldarg_0),
                processor.Create(OpCodes.Ldfld, counterFieldDefinition),
                processor.Create(OpCodes.Ldc_I4_1),
                processor.Create(OpCodes.Add),

                // store and refetch. Divide by 2 and keep remainder.
                processor.Create(OpCodes.Stfld, counterFieldDefinition),
                processor.Create(OpCodes.Ldfld, counterFieldDefinition),
                processor.Create(OpCodes.Ldc_I4_2),
                processor.Create(OpCodes.Rem),

                // if remainder is not 1, jump to original start of function.
                processor.Create(OpCodes.Ldc_I4_1),
                processor.Create(OpCodes.Bne_Un_S, originalStart),

                // write the name of the function to the console.
                processor.Create(OpCodes.Ldarg_0),
                processor.Create(OpCodes.Call, module.ImportReference(getTypeReference)),
                processor.Create(OpCodes.Callvirt, module.ImportReference(getFullNameReference)),
                processor.Create(OpCodes.Call, module.ImportReference(writeLineReference)),

                // load the field where the action delegate is stored.
                processor.Create(OpCodes.Ldarg_0),
                processor.Create(OpCodes.Ldfld, observerObjectDefinition),
            };

            var invokeMethod = observerObjectType.Resolve().Methods.Single(x => x.Name == "BuildRenderTree");
            var invokeReference = module.ImportReference(invokeMethod);

            // push all function arguments onto the evaluation stack.
            for (int i = 0; i < buildRenderTreeMethod.Parameters.Count; i++)
            {
                prefix.Add(processor.Ldarg(i + 1));
            }

            // call the action delegate with the arguments on the evaluation stack.
            prefix.Add(processor.Create(OpCodes.Callvirt, invokeReference));

            // this pointers for fetch and store;
            prefix.Add(processor.Create(OpCodes.Ldarg_0));
            prefix.Add(processor.Create(OpCodes.Ldarg_0));

            // this.counterFieldDefinition -= 2;
            prefix.Add(processor.Create(OpCodes.Ldfld, counterFieldDefinition));
            prefix.Add(processor.Create(OpCodes.Ldc_I4_2));
            prefix.Add(processor.Create(OpCodes.Sub));
            prefix.Add(processor.Create(OpCodes.Stfld, counterFieldDefinition));

            prefix.Add(processor.Create(OpCodes.Ret));

            foreach (var instruction in prefix)
            {
                processor.InsertBefore(originalStart, instruction);
            }
        }
    }
}
