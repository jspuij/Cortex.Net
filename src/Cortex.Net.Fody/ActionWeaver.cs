// <copyright file="ActionWeaver.cs" company="Jan-Willem Spuij">
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
    /// Weaves methods decorated with an ActionAttribute.
    /// </summary>
    internal class ActionWeaver
    {
        /// <summary>
        /// The prefix for an inner method that is the target of an action.
        /// </summary>
        private const string InnerCounterFieldPrefix = "cortex_Net_Fvclsnf97SxcMxlkizajkz_";

        /// <summary>
        /// The prefix for an inner field that contains the action.
        /// </summary>
        private const string InnerActionFieldPrefix = "cortex_Net_Dls82uidkJs37dlaksjid_";

        /// <summary>
        /// A reference to the parent Cortex.Net weaver.
        /// </summary>
        private readonly CortexWeaver parentWeaver;

        /// <summary>
        /// The queue to add ILProcessor actions to.
        /// </summary>
        private readonly ISharedStateAssignmentILProcessorQueue processorQueue;

        /// <summary>
        /// Weaving context.
        /// </summary>
        private readonly WeavingContext weavingContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionWeaver"/> class.
        /// </summary>
        /// <param name="parentWeaver">A reference to the Parent Cortex.Net weaver.</param>
        /// <param name="processorQueue">The queue to add ILProcessor actions to.</param>
        /// <param name="weavingContext">The resolved types necessary by this weaver.</param>
        /// <exception cref="ArgumentNullException">When any of the arguments is null.</exception>
        public ActionWeaver(CortexWeaver parentWeaver, ISharedStateAssignmentILProcessorQueue processorQueue, WeavingContext weavingContext)
        {
            this.parentWeaver = parentWeaver ?? throw new ArgumentNullException(nameof(parentWeaver));
            this.processorQueue = processorQueue ?? throw new ArgumentNullException(nameof(processorQueue));
            this.weavingContext = weavingContext ?? throw new ArgumentNullException(nameof(weavingContext));
        }

        /// <summary>
        /// Executes this action weaver.
        /// </summary>
        internal void Execute()
        {
            var decoratedMethods = from t in this.parentWeaver.ModuleDefinition.GetTypes()
                                   from m in t.Methods
                                   where
                                      t != null &&
                                      t.IsClass &&
                                      t.BaseType != null &&
                                      m != null &&
                                      m.CustomAttributes != null &&
                                      m.CustomAttributes.Any(x => x.AttributeType.FullName == this.weavingContext.CortexNetApiActionAttribute.FullName)
                                   select m;

            foreach (var method in decoratedMethods.ToList())
            {
                this.WeaveMethod(method);
            }
        }

        /// <summary>
        /// Rewrites the body of the method with the Action Attribute to call the Cortex.NET action.
        /// </summary>
        /// <param name="methodDefinition">The method definition to rewrite.</param>
        /// <param name="actionType">The type of the action delegate to invoke.</param>
        /// <param name="counterFieldDefinition">The definition of the field that holds the entrance counter.</param>
        /// <param name="actionFieldDefinition">The definition of the field that holds the action delegate.</param>
        private static void ExtendActionMethodBody(MethodDefinition methodDefinition, TypeReference actionType, FieldDefinition counterFieldDefinition, FieldDefinition actionFieldDefinition)
        {
            if (methodDefinition is null)
            {
                throw new ArgumentNullException(nameof(methodDefinition));
            }

            if (actionType is null)
            {
                throw new ArgumentNullException(nameof(actionType));
            }

            if (counterFieldDefinition is null)
            {
                throw new ArgumentNullException(nameof(counterFieldDefinition));
            }

            if (actionFieldDefinition is null)
            {
                throw new ArgumentNullException(nameof(actionFieldDefinition));
            }

            var processor = methodDefinition.Body.GetILProcessor();

            var originalStart = methodDefinition.Body.Instructions.First();
            var originalEnd = methodDefinition.Body.Instructions.Last();

            var prefix = new List<Instruction>
            {
                // if fieldDefinition == null, jump to originalStart.
                processor.Create(OpCodes.Ldarg_0),
                processor.Create(OpCodes.Ldfld, actionFieldDefinition),
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

                // load the field where the action delegate is stored.
                processor.Create(OpCodes.Ldarg_0),
                processor.Create(OpCodes.Ldfld, actionFieldDefinition),
            };

            var invokeMethod = actionType.Resolve().Methods.Single(x => x.Name == "Invoke");
            var invokeReference = invokeMethod.GetGenericMethodOnInstantance(actionType);

            // push all function arguments onto the evaluation stack.
            for (int i = 0; i < methodDefinition.Parameters.Count; i++)
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

        /// <summary>
        /// Emits most of the body of the shared state setter.
        /// </summary>
        /// <param name="processor">The ILProcessor to use.</param>
        /// <param name="sharedStateBackingField">The backing field for the shared state.</param>
        /// <param name="methodDefinition">The inner definition of the action method.</param>
        /// <param name="actionType">The action type.</param>
        /// <param name="actionFieldDefinition">The field definition.</param>
        private void EmitSharedStateSetter(
        ILProcessor processor,
        FieldReference sharedStateBackingField,
        MethodDefinition methodDefinition,
        TypeReference actionType,
        FieldDefinition actionFieldDefinition)
        {
            var moduleDefinition = sharedStateBackingField.Module;

            // determine the name of the action.
            var attribute = methodDefinition.CustomAttributes.Single(x => x.AttributeType.FullName == this.weavingContext.CortexNetApiActionAttribute.FullName);
            var actionName = methodDefinition.Name;
            var attributeArgument = attribute.ConstructorArguments.FirstOrDefault();
            if (!string.IsNullOrEmpty(attributeArgument.Value as string))
            {
                actionName = attributeArgument.Value as string;
            }

            var actionExtensions = this.weavingContext.CortexNetApiActionExtensions;
            var voidType = moduleDefinition.TypeSystem.Void;

            MethodReference createActionMethod;

            MethodReference actionTypeConstructorReference = actionType.Resolve().Methods.Single(x => x.IsConstructor);

            actionTypeConstructorReference = actionTypeConstructorReference.GetGenericMethodOnInstantance(actionType);

            if (actionType is GenericInstanceType)
            {
                actionTypeConstructorReference = moduleDefinition.ImportReference(actionTypeConstructorReference, (GenericInstanceType)actionType);

                createActionMethod = actionExtensions.Resolve().Methods.Single(x => x.Name.Contains("CreateAction") && x.GenericParameters.Count == ((GenericInstanceType)actionType).GenericArguments.Count);
                var createActionInstanceMethod = new GenericInstanceMethod(createActionMethod);
                foreach (var parameter in ((GenericInstanceType)actionType).GenericArguments)
                {
                    createActionInstanceMethod.GenericArguments.Add(parameter.Resolve());
                }

                createActionMethod = moduleDefinition.ImportReference(createActionInstanceMethod);
            }
            else
            {
                actionTypeConstructorReference = moduleDefinition.ImportReference(actionTypeConstructorReference);
                createActionMethod = moduleDefinition.ImportReference(actionExtensions.Resolve().Methods.Single(x => x.Name.Contains("CreateAction") && !x.GenericParameters.Any()));
            }

            processor.Emit(OpCodes.Ldarg_0);
            processor.Emit(OpCodes.Ldarg_0);
            processor.Emit(OpCodes.Ldfld, sharedStateBackingField);
            processor.Emit(OpCodes.Ldstr, actionName);
            processor.Emit(OpCodes.Ldarg_0);
            processor.Emit(OpCodes.Ldarg_0);
            processor.Emit(OpCodes.Ldftn, methodDefinition);
            processor.Emit(OpCodes.Newobj, actionTypeConstructorReference);
            processor.Emit(OpCodes.Call, createActionMethod);
            processor.Emit(OpCodes.Stfld, actionFieldDefinition);
        }

        /// <summary>
        /// Weaves a method that was Decorated with the Action attribute.
        /// </summary>
        /// <param name="methodDefinition">The method definition.</param>
        /// <remarks>
        /// The reason that methods are extended with a counter counting recursive entrance is that this
        /// way debugging is kept intact.
        /// </remarks>
        private void WeaveMethod(MethodDefinition methodDefinition)
        {
            var moduleDefinition = methodDefinition.Module;
            var declaringType = methodDefinition.DeclaringType;

            var fieldAttributes = FieldAttributes.Private;
            if (methodDefinition.IsStatic)
            {
                fieldAttributes |= FieldAttributes.Static;
            }

            // convert the method definition to a corresponding Action<> delegate.
            var actionType = methodDefinition.GetActionType(this.weavingContext);

            // add the delegate as field to the class.
            var actionFieldDefinition = declaringType.CreateField(actionType, $"{InnerActionFieldPrefix}{methodDefinition.Name}_Action", this.weavingContext, fieldAttributes);

            // add entrance counter field.
            var entranceCounterDefinition = declaringType.CreateField(moduleDefinition.TypeSystem.Int32, $"{InnerCounterFieldPrefix}{methodDefinition.Name}_EntranceCount", this.weavingContext, fieldAttributes);

            // push IL code for initialization of action members to the queue to emit in the ISharedState setter.
            this.processorQueue.SharedStateAssignmentQueue.Enqueue(
                (declaringType,
                false,
                (processor, sharedStateBackingField) => this.EmitSharedStateSetter(
                    processor,
                    sharedStateBackingField,
                    methodDefinition,
                    actionType,
                    actionFieldDefinition)));

            // extend the action method body.
            ExtendActionMethodBody(methodDefinition, actionType, entranceCounterDefinition, actionFieldDefinition);
        }
    }
}
