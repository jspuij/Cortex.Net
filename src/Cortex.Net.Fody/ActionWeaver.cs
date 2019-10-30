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
    using System.Globalization;
    using System.Linq;
    using Cortex.Net.Api;
    using Cortex.Net.Fody.Properties;
    using global::Fody;
    using Mono.Cecil;
    using Mono.Cecil.Cil;

    /// <summary>
    /// Weaves methods decorated with an <see cref="ActionAttribute"/>.
    /// </summary>
    internal class ActionWeaver
    {
        /// <summary>
        /// The prefix for an inner method that is the target of an action.
        /// </summary>
        private const string InnerMethodPrefix = "Cortex_Net_Fvclsnf97SxcMxlkizajkz_";

        /// <summary>
        /// The prefix for an inner field that contains the action.
        /// </summary>
        private const string InnerActionFieldPrefix = "cortex_Net_Dls82uidkJs37dlaksjid_";

        /// <summary>
        /// A reference to the parent Cortex.Net weaver.
        /// </summary>
        private readonly BaseModuleWeaver cortexWeaver;

        /// <summary>
        /// The queue to add ILProcessor actions to.
        /// </summary>
        private readonly ISharedStateAssignmentILProcessorQueue processorQueue;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionWeaver"/> class.
        /// </summary>
        /// <param name="parentWeaver">A reference to the Parent Cortex.Net weaver.</param>
        /// <param name="processorQueue">The queue to add ILProcessor actions to.</param>
        /// <exception cref="ArgumentNullException">When any of the arguments is null.</exception>
        public ActionWeaver(BaseModuleWeaver parentWeaver, ISharedStateAssignmentILProcessorQueue processorQueue)
        {
            this.cortexWeaver = parentWeaver ?? throw new ArgumentNullException(nameof(parentWeaver));
            this.processorQueue = processorQueue ?? throw new ArgumentNullException(nameof(processorQueue));
        }

        /// <summary>
        /// Executes this action weaver.
        /// </summary>
        internal void Execute()
        {
            var decoratedMethods = from t in this.cortexWeaver.ModuleDefinition.GetTypes()
                             from m in t.Methods
                             where
                                t != null &&
                                t.IsClass &&
                                t.BaseType != null &&
                                m != null &&
                                m.CustomAttributes != null &&
                                m.CustomAttributes.Any(x => x.AttributeType.FullName == typeof(ActionAttribute).FullName)
                             select m;

            foreach (var method in decoratedMethods.ToList())
            {
                this.WeaveMethod(method);
            }
        }

        /// <summary>
        /// Duplicates a method with with the same body as the source method
        /// and adds it to the class to the class.
        /// </summary>
        /// <param name="sourceMethod">The source method.</param>
        private static MethodDefinition DuplicateAsInnerMethod(MethodDefinition sourceMethod)
        {
            var innerDefinition = new MethodDefinition($"{InnerMethodPrefix}{sourceMethod.Name}", (sourceMethod.Attributes & ~MethodAttributes.Public) | MethodAttributes.Private, sourceMethod.ReturnType)
            {
                Body = sourceMethod.Body,
                IsStatic = sourceMethod.IsStatic,
            };

            foreach (var parameter in sourceMethod.Parameters)
            {
                innerDefinition.Parameters.Add(parameter);
            }

            sourceMethod.DeclaringType.Methods.Add(innerDefinition);

            return innerDefinition;
        }

        /// <summary>
        /// Rewrites the body of the method with the Action Attribute to call the Cortex.NET action.
        /// </summary>
        /// <param name="methodDefinition">The method definition to rewrite.</param>
        /// <param name="actionType">The type of the action delegate to invoke.</param>
        /// <param name="fieldDefinition">The definition of the field that holds the action delegate.</param>
        private static void RewriteActionMethodBody(MethodDefinition methodDefinition, TypeReference actionType, FieldDefinition fieldDefinition)
        {
            methodDefinition.Body = new MethodBody(methodDefinition);
            var processor = methodDefinition.Body.GetILProcessor();

            var invokeMethod = actionType.Resolve().Methods.Single(x => x.Name == "Invoke");
            var invokeReference = invokeMethod.GetGenericMethodOnInstantance(actionType);

            // load the field where the action delegate is stored.
            processor.Emit(OpCodes.Ldarg_0);
            processor.Emit(OpCodes.Ldfld, fieldDefinition);

            // push all function arguments onto the evaluation stack.
            for (int i = 0; i < methodDefinition.Parameters.Count; i++)
            {
                switch (i)
                {
                    case 0:
                        processor.Emit(OpCodes.Ldarg_1);
                        break;
                    case 1:
                        processor.Emit(OpCodes.Ldarg_2);
                        break;
                    case 2:
                        processor.Emit(OpCodes.Ldarg_3);
                        break;
                    default:
                        processor.Emit(OpCodes.Ldarg_S, i + 1);
                        break;
                }
            }

            // call the action delegate with the arguments on the evaluation stack.
            processor.Emit(OpCodes.Callvirt, invokeReference);
            processor.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// Emits most of the body of the shared state setter.
        /// </summary>
        /// <param name="processor">The ILProcessor to use.</param>
        /// <param name="sharedStateBackingField">The backing field for the shared state.</param>
        /// <param name="actionName">The action name.</param>
        /// <param name="innerDefinition">The inner definition of the action method.</param>
        /// <param name="actionType">The action type.</param>
        /// <param name="actionFieldDefinition">The field definition.</param>
        private static void EmitSharedStateSetter(
            ILProcessor processor,
            FieldReference sharedStateBackingField,
            string actionName,
            MethodDefinition innerDefinition,
            TypeReference actionType,
            FieldDefinition actionFieldDefinition)
        {
            var moduleDefinition = sharedStateBackingField.Module;

            var actionExtensions = moduleDefinition.ImportReference(typeof(ActionExtensions));
            var voidType = moduleDefinition.ImportReference(typeof(void));

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
            processor.Emit(OpCodes.Ldftn, innerDefinition);
            processor.Emit(OpCodes.Newobj, actionTypeConstructorReference);
            processor.Emit(OpCodes.Call, createActionMethod);
            processor.Emit(OpCodes.Stfld, actionFieldDefinition);
        }

        /// <summary>
        /// Weaves a method that was Decorated with the <see cref="ActionAttribute"/>.
        /// </summary>
        /// <param name="methodDefinition">The method definition.</param>
        private void WeaveMethod(MethodDefinition methodDefinition)
        {
            var moduleDefinition = methodDefinition.Module;
            var declaringType = methodDefinition.DeclaringType;

            // Duplicate the action method as private inner method.
            var innerDefinition = DuplicateAsInnerMethod(methodDefinition);

            var fieldAttributes = FieldAttributes.Private;
            if (methodDefinition.IsStatic)
            {
                fieldAttributes |= FieldAttributes.Static;
            }

            // convert the method definition to a corresponding Action<> delegate.
            var actionType = this.GetActionType(methodDefinition);

            // add the delegate as field to the class.
            var fieldDefinition = new FieldDefinition($"{InnerActionFieldPrefix}{methodDefinition.Name}_Action", fieldAttributes, actionType);
            declaringType.Fields.Add(fieldDefinition);

            // push IL code for initialization of action members to the queue to emit in the ISharedState setter.
            this.processorQueue.SharedStateAssignmentQueue.Enqueue(
                (declaringType,
                (processor, sharedStateBackingField) => EmitSharedStateSetter(
                    processor,
                    sharedStateBackingField,
                    methodDefinition.Name,
                    innerDefinition,
                    actionType,
                    fieldDefinition)));

            // rewrite the action method body.
            RewriteActionMethodBody(methodDefinition, actionType, fieldDefinition);
        }

        /// <summary>
        /// Gets the Action type for the private field that is added to the class for the private method.
        /// </summary>
        /// <param name="methodDefinition">The method definition for the action.</param>
        /// <returns>A type reference.</returns>
        private TypeReference GetActionType(MethodDefinition methodDefinition)
        {
            var moduleDefinition = this.cortexWeaver.ModuleDefinition;

            if (methodDefinition.Parameters == null || !methodDefinition.Parameters.Any())
            {
                return moduleDefinition.ImportReference(typeof(Action));
            }

            TypeReference genericActionType;

            switch (methodDefinition.Parameters.Count)
            {
                case 1:
                    genericActionType = moduleDefinition.ImportReference(typeof(Action<>));
                    break;
                case 2:
                    genericActionType = moduleDefinition.ImportReference(typeof(Action<,>));
                    break;
                case 3:
                    genericActionType = moduleDefinition.ImportReference(typeof(Action<,,>));
                    break;
                case 4:
                    genericActionType = moduleDefinition.ImportReference(typeof(Action<,,,>));
                    break;
                case 5:
                    genericActionType = moduleDefinition.ImportReference(typeof(Action<,,,,>));
                    break;
                case 6:
                    genericActionType = moduleDefinition.ImportReference(typeof(Action<,,,,,>));
                    break;
                case 7:
                    genericActionType = moduleDefinition.ImportReference(typeof(Action<,,,,,,>));
                    break;
                case 8:
                    genericActionType = moduleDefinition.ImportReference(typeof(Action<,,,,,,,>));
                    break;
                case 9:
                    genericActionType = moduleDefinition.ImportReference(typeof(Action<,,,,,,,,>));
                    break;
                case 10:
                    genericActionType = moduleDefinition.ImportReference(typeof(Action<,,,,,,,,,>));
                    break;
                case 11:
                    genericActionType = moduleDefinition.ImportReference(typeof(Action<,,,,,,,,,,>));
                    break;
                case 12:
                    genericActionType = moduleDefinition.ImportReference(typeof(Action<,,,,,,,,,,,>));
                    break;
                case 13:
                    genericActionType = moduleDefinition.ImportReference(typeof(Action<,,,,,,,,,,,,>));
                    break;
                case 14:
                    genericActionType = moduleDefinition.ImportReference(typeof(Action<,,,,,,,,,,,,,>));
                    break;
                case 15:
                    genericActionType = moduleDefinition.ImportReference(typeof(Action<,,,,,,,,,,,,,,>));
                    break;
                case 16:
                    genericActionType = moduleDefinition.ImportReference(typeof(Action<,,,,,,,,,,,,,,,>));
                    break;
                default:
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.MoreThan16Parameters, methodDefinition.Name));
            }

            var instance = new GenericInstanceType(genericActionType);

            foreach (var parameter in methodDefinition.Parameters)
            {
                instance.GenericArguments.Add(parameter.ParameterType);
            }

            return instance;
        }
    }
}
