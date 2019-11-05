// <copyright file="ComputedWeaver.cs" company="Jan-Willem Spuij">
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
    using System.Runtime.CompilerServices;
    using System.Text;
    using Cortex.Net.Api;
    using Cortex.Net.Fody.Properties;
    using Cortex.Net.Types;
    using global::Fody;
    using Mono.Cecil;
    using Mono.Cecil.Cil;

    /// <summary>
    /// Weaves properties and methods decorated with an <see cref="ComputedAttribute"/>.
    /// </summary>
    internal class ComputedWeaver : ObservableObjectWeaverBase
    {
        /// <summary>
        /// The prefix for an inner method that is the target of an action.
        /// </summary>
        private const string InnerCounterFieldPrefix = "cortex_Net_Fvclsnf97SxcMxlkizajkz_";

        /// <summary>
        /// Initializes a new instance of the <see cref="ComputedWeaver"/> class.
        /// </summary>
        /// <param name="parentWeaver">A reference to the Parent Cortex.Net weaver.</param>
        /// <param name="processorQueue">The queue to add ILProcessor actions to.</param>
        /// <exception cref="ArgumentNullException">When any of the arguments is null.</exception>
        public ComputedWeaver(BaseModuleWeaver parentWeaver, ISharedStateAssignmentILProcessorQueue processorQueue)
            : base(parentWeaver, processorQueue)
        {
        }

        /// <summary>
        /// Executes this observable weaver.
        /// </summary>
        internal void Execute()
        {
            var decoratedProperties = from t in this.ParentWeaver.ModuleDefinition.GetTypes()
                                   from m in t.Properties
                                   where
                                      t != null &&
                                      t.IsClass &&
                                      t.BaseType != null &&
                                      m != null &&
                                      m.CustomAttributes != null &&
                                      m.CustomAttributes.Any(x => x.AttributeType.FullName == typeof(ComputedAttribute).FullName)
                                   select m;

            foreach (var decoratedProperty in decoratedProperties.ToList())
            {
                // this.WeaveProperty(decoratedProperty, typeof(DeepEnhancer));
            }

            var decoratedMethods = from t in this.ParentWeaver.ModuleDefinition.GetTypes()
                                   from m in t.Methods
                                   where
                                      t != null &&
                                      t.IsClass &&
                                      t.BaseType != null &&
                                      m != null &&
                                      m.CustomAttributes != null &&
                                      m.CustomAttributes.Any(x => x.AttributeType.FullName == typeof(ComputedAttribute).FullName)
                                   select m;


            foreach (var method in decoratedMethods.ToList())
            {
                this.WeaveMethod(method);
            }
        }

        /// <summary>
        /// Weaves a method that is decorated with a <see cref="ComputedAttribute"/>.
        /// </summary>
        /// <param name="methodDefinition">The method definition of the decorated method.</param>
        private void WeaveMethod(MethodDefinition methodDefinition)
        {
            // property name
            var computedName = methodDefinition.Name;
            var methodReturnType = methodDefinition.ReturnType;
            var moduleDefinition = methodDefinition.Module;
            var declaringType = methodDefinition.DeclaringType;

            // default enhancer
            var defaultEhancerType = moduleDefinition.ImportReference(typeof(DeepEnhancer));
            var iequalityComparerType = moduleDefinition.ImportReference(typeof(System.Collections.IEqualityComparer));
            var defaultMethodDefinition = moduleDefinition.ImportReference(typeof(EqualityComparer<>)).Resolve().Properties.Single(x => x.Name == "Default").GetMethod;
            TypeReference equalityComparerType = new GenericInstanceType(moduleDefinition.ImportReference(typeof(EqualityComparer<>)));
            ((GenericInstanceType)equalityComparerType).GenericArguments.Add(methodDefinition.ReturnType);

            var computedAttribute = methodDefinition.CustomAttributes.SingleOrDefault(x => x.AttributeType.FullName == typeof(ComputedAttribute).FullName);

            var fieldAttributes = FieldAttributes.Private;
            if (methodDefinition.IsStatic)
            {
                fieldAttributes |= FieldAttributes.Static;
            }

            // add entrance counter field.
            var entranceCounterDefinition = declaringType.CreateField(moduleDefinition.TypeSystem.Int32, $"{InnerCounterFieldPrefix}{methodDefinition.Name}_EntranceCount", fieldAttributes);

            var keepAlive = false;
            var requiresReaction = false;

            int boolIndex = 0;

            foreach (var ca in computedAttribute.ConstructorArguments)
            {
                if (ca.Type == moduleDefinition.TypeSystem.String)
                {
                    computedName = ca.Value as string;
                }

                if (ca.Type == iequalityComparerType)
                {
                    equalityComparerType = moduleDefinition.ImportReference(ca.Value as System.Type);
                }

                if (ca.Type == moduleDefinition.TypeSystem.Boolean)
                {
                    if (boolIndex++ == 0)
                    {
                        requiresReaction = (bool)ca.Value;
                    }
                    else
                    {
                        keepAlive = (bool)ca.Value;
                    }
                }
            }

            var defaultMethodReference = moduleDefinition.ImportReference(defaultMethodDefinition.GetGenericMethodOnInstantance(equalityComparerType));

            // get or create the ObservableObjectField.
            FieldDefinition observableObjectField = declaringType.Fields.FirstOrDefault(x => x.FieldType.FullName == typeof(ObservableObject).FullName);
            if (observableObjectField is null)
            {
                var observableFieldTypeReference = moduleDefinition.ImportReference(typeof(ObservableObject));
                observableObjectField = declaringType.CreateField(observableFieldTypeReference, InnerObservableObjectFieldName);

                // push IL code for initialization of observableObject to the queue to emit in the ISharedState setter.
                this.ProcessorQueue.SharedStateAssignmentQueue.Enqueue(
                    (declaringType,
                    (processor, sharedStateBackingField) => this.EmitObservableObjectInit(
                        processor,
                        declaringType.Name,
                        defaultEhancerType,
                        sharedStateBackingField,
                        observableObjectField)));
            }

            // push IL code for initialization of a computed member to the queue to emit in the ISharedState setter.
            this.ProcessorQueue.SharedStateAssignmentQueue.Enqueue(
                (declaringType,
                (processor, sharedStateBackingField) => this.EmitComputedMemberAdd(
                    processor,
                    computedName,
                    methodDefinition,
                    equalityComparerType,
                    requiresReaction,
                    keepAlive,
                    sharedStateBackingField,
                    observableObjectField)));

            // extend the method body.
            this.ExtendMethodBody(methodDefinition, computedName, entranceCounterDefinition, observableObjectField);
        }

        /// <summary>
        /// Extends the method body of the Computed method.
        /// </summary>
        /// <param name="methodDefinition">The method Definition to extend.</param>
        /// <param name="counterFieldDefinition">The entrance counter to register recursive calls.</param>
        /// <param name="observableObjectField">The observable object field.</param>
        private void ExtendMethodBody(MethodDefinition methodDefinition, string computedName, FieldDefinition counterFieldDefinition, FieldDefinition observableObjectField)
        {
            var processor = methodDefinition.Body.GetILProcessor();

            var originalStart = methodDefinition.Body.Instructions.First();
            var originalEnd = methodDefinition.Body.Instructions.Last();

            var observableObjectType = this.ParentWeaver.ModuleDefinition.ImportReference(typeof(ObservableObject)).Resolve();
            var observableObjectReadPropertyMethod = new GenericInstanceMethod(observableObjectType.Methods.FirstOrDefault(m => m.Name == "Read"));
            observableObjectReadPropertyMethod.GenericArguments.Add(methodDefinition.ReturnType);

            var prefix = new List<Instruction>
            {
                // if fieldDefinition == null, jump to originalStart.
                processor.Create(OpCodes.Ldarg_0),
                processor.Create(OpCodes.Ldfld, observableObjectField),
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
                processor.Create(OpCodes.Ldfld, observableObjectField),

                // call the read method on the observable object.
                processor.Create(OpCodes.Ldstr, computedName),
                processor.Create(OpCodes.Callvirt, this.ParentWeaver.ModuleDefinition.ImportReference(observableObjectReadPropertyMethod)),

                // this pointers for fetch and store;
                processor.Create(OpCodes.Ldarg_0),
                processor.Create(OpCodes.Ldarg_0),

                // this.counterFieldDefinition -= 2;
                processor.Create(OpCodes.Ldfld, counterFieldDefinition),
                processor.Create(OpCodes.Ldc_I4_2),
                processor.Create(OpCodes.Sub),
                processor.Create(OpCodes.Stfld, counterFieldDefinition),

                processor.Create(OpCodes.Ret),
            };

            foreach (var instruction in prefix)
            {
                processor.InsertBefore(originalStart, instruction);
            }
        }

        /// <summary>
        /// Emits IL code for adding a computed member on this observable object.
        /// </summary>
        /// <param name="processor">The ILProcessor to use.</param>
        /// <param name="computedName">The name of the computed member.</param>
        /// <param name="methodDefinition">The methodDefinition of the member.</param>
        /// <param name="equalityComparerType">The equality comparer type.</param>
        /// <param name="requiresReaction">Whether the computed value requires a reaction.</param>
        /// <param name="keepAlive">Whether to keep the computed value alive.</param>
        /// <param name="sharedStateBackingField">The backing field for the shared state.</param>
        /// <param name="observableObjectField">A <see cref="FieldDefinition"/> for the field where the <see cref="ObservableObject"/> instance is kept.</param>
        private void EmitComputedMemberAdd(
            ILProcessor processor,
            string computedName,
            MethodDefinition methodDefinition,
            TypeReference equalityComparerType,
            bool requiresReaction,
            bool keepAlive,
            FieldReference sharedStateBackingField,
            FieldDefinition observableObjectField)
        {
            var module = methodDefinition.Module;
            var functionType = this.GetFunctionType(methodDefinition);

            var observableObjectType = this.ParentWeaver.ModuleDefinition.ImportReference(typeof(ObservableObject)).Resolve();
            var observableObjectAddComputedMethod = new GenericInstanceMethod(observableObjectType.Methods.FirstOrDefault(m => m.Name == "AddComputedMember"));
            observableObjectAddComputedMethod.GenericArguments.Add(methodDefinition.ReturnType);

            MethodReference functionTypeConstructorReference = functionType.Resolve().Methods.Single(x => x.IsConstructor);

            functionTypeConstructorReference = module.ImportReference(functionTypeConstructorReference.GetGenericMethodOnInstantance(functionType));

            var computedValueOptionsType = module.ImportReference(typeof(ComputedValueOptions<>)).Resolve();
            var computedValueOptionsConstructor = computedValueOptionsType.Methods.Single(x => x.IsConstructor);
            var computedValueOptionsInstanceType = new GenericInstanceType(computedValueOptionsType);
            computedValueOptionsInstanceType.GenericArguments.Add(methodDefinition.ReturnType);

            var ctorReference = module.ImportReference(computedValueOptionsConstructor.GetGenericMethodOnInstantance(computedValueOptionsInstanceType));
            var setContextMethod = computedValueOptionsType.Methods.Single(x => x.Name == "set_Context");
            var setContextReference = module.ImportReference(setContextMethod.GetGenericMethodOnInstantance(computedValueOptionsInstanceType));
            var setRequiresReactionMethod = computedValueOptionsType.Methods.Single(x => x.Name == "set_RequiresReaction");
            var setRequiresReactionReference = module.ImportReference(setRequiresReactionMethod.GetGenericMethodOnInstantance(computedValueOptionsInstanceType));
            var setKeepAliveMethod = computedValueOptionsType.Methods.Single(x => x.Name == "set_KeepAlive");
            var setKeepAliveReference = module.ImportReference(setKeepAliveMethod.GetGenericMethodOnInstantance(computedValueOptionsInstanceType));

            processor.Emit(OpCodes.Ldarg_0);
            processor.Emit(OpCodes.Ldfld, observableObjectField);
            processor.Emit(OpCodes.Ldstr, computedName);
            processor.Emit(OpCodes.Ldarg_0);
            processor.Emit(OpCodes.Ldftn, methodDefinition);
            processor.Emit(OpCodes.Newobj, functionTypeConstructorReference);
            processor.Emit(OpCodes.Ldstr, computedName);
            processor.Emit(OpCodes.Newobj, ctorReference);
            processor.Emit(OpCodes.Dup);
            processor.Emit(OpCodes.Ldarg_0);
            processor.Emit(OpCodes.Callvirt, setContextReference);
            processor.Emit(OpCodes.Dup);
            processor.Emit(requiresReaction ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
            processor.Emit(OpCodes.Callvirt, setRequiresReactionReference);
            processor.Emit(OpCodes.Dup);
            processor.Emit(keepAlive ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
            processor.Emit(OpCodes.Callvirt, setKeepAliveReference);
            processor.Emit(OpCodes.Callvirt, module.ImportReference(observableObjectAddComputedMethod));
        }

        /// <summary>
        /// Gets the Function type for the computed method that is passed.
        /// </summary>
        /// <param name="methodDefinition">The method definition for the function.</param>
        /// <returns>A type reference.</returns>
        private TypeReference GetFunctionType(MethodDefinition methodDefinition)
        {
            var moduleDefinition = this.ParentWeaver.ModuleDefinition;

            TypeReference genericFunctionType;

            switch (methodDefinition.Parameters.Count)
            {
                case 0:
                    genericFunctionType = moduleDefinition.ImportReference(typeof(Func<>));
                    break;
                case 1:
                    genericFunctionType = moduleDefinition.ImportReference(typeof(Func<,>));
                    break;
                case 2:
                    genericFunctionType = moduleDefinition.ImportReference(typeof(Func<,,>));
                    break;
                case 3:
                    genericFunctionType = moduleDefinition.ImportReference(typeof(Func<,,,>));
                    break;
                case 4:
                    genericFunctionType = moduleDefinition.ImportReference(typeof(Func<,,,,>));
                    break;
                case 5:
                    genericFunctionType = moduleDefinition.ImportReference(typeof(Func<,,,,,>));
                    break;
                case 6:
                    genericFunctionType = moduleDefinition.ImportReference(typeof(Func<,,,,,,>));
                    break;
                case 7:
                    genericFunctionType = moduleDefinition.ImportReference(typeof(Func<,,,,,,,>));
                    break;
                case 8:
                    genericFunctionType = moduleDefinition.ImportReference(typeof(Func<,,,,,,,,>));
                    break;
                case 9:
                    genericFunctionType = moduleDefinition.ImportReference(typeof(Func<,,,,,,,,,>));
                    break;
                case 10:
                    genericFunctionType = moduleDefinition.ImportReference(typeof(Func<,,,,,,,,,,>));
                    break;
                case 11:
                    genericFunctionType = moduleDefinition.ImportReference(typeof(Func<,,,,,,,,,,,>));
                    break;
                case 12:
                    genericFunctionType = moduleDefinition.ImportReference(typeof(Func<,,,,,,,,,,,,>));
                    break;
                case 13:
                    genericFunctionType = moduleDefinition.ImportReference(typeof(Func<,,,,,,,,,,,,,>));
                    break;
                case 14:
                    genericFunctionType = moduleDefinition.ImportReference(typeof(Func<,,,,,,,,,,,,,,>));
                    break;
                case 15:
                    genericFunctionType = moduleDefinition.ImportReference(typeof(Func<,,,,,,,,,,,,,,,>));
                    break;
                case 16:
                    genericFunctionType = moduleDefinition.ImportReference(typeof(Func<,,,,,,,,,,,,,,,,>));
                    break;
                default:
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.MoreThan16Parameters, methodDefinition.Name));
            }

            var instance = new GenericInstanceType(genericFunctionType);

            foreach (var parameter in methodDefinition.Parameters)
            {
                instance.GenericArguments.Add(parameter.ParameterType);
            }

            instance.GenericArguments.Add(methodDefinition.ReturnType);

            return instance;
        }
    }
}
