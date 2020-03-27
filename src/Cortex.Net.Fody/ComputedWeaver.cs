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
    using Cortex.Net.Fody.Properties;
    using global::Fody;
    using Mono.Cecil;
    using Mono.Cecil.Cil;

    /// <summary>
    /// Weaves properties and methods decorated with an ComputedAttribute.
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
        /// <param name="weavingContext">The resolved types necessary by this weaver.</param>
        public ComputedWeaver(ModuleWeaver parentWeaver, ISharedStateAssignmentILProcessorQueue processorQueue, WeavingContext weavingContext)
            : base(parentWeaver, processorQueue, weavingContext)
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
                                      m.CustomAttributes.Any(x => x.AttributeType.FullName == this.WeavingContext.CortexNetApiComputedAttribute.FullName)
                                   select m;

            foreach (var decoratedProperty in decoratedProperties.ToList())
            {
                this.WeaveProperty(decoratedProperty);
            }

            var decoratedMethods = from t in this.ParentWeaver.ModuleDefinition.GetTypes()
                                   from m in t.Methods
                                   where
                                      t != null &&
                                      t.IsClass &&
                                      t.BaseType != null &&
                                      m != null &&
                                      m.CustomAttributes != null &&
                                      m.CustomAttributes.Any(x => x.AttributeType.FullName == this.WeavingContext.CortexNetApiComputedAttribute.FullName)
                                   select m;

            foreach (var method in decoratedMethods.ToList())
            {
                this.WeaveMethod(method);
            }
        }

        /// <summary>
        /// Weaves a property that is decorated with a ComputedAttribute.
        /// </summary>
        /// <param name="propertyDefinition">The definition of the property.</param>
        private void WeaveProperty(PropertyDefinition propertyDefinition)
        {
            // property name
            var computedName = propertyDefinition.Name;
            var propertyType = propertyDefinition.PropertyType;
            var moduleDefinition = propertyDefinition.Module;
            var declaringType = propertyDefinition.DeclaringType;

            var computedAttribute = propertyDefinition.CustomAttributes.SingleOrDefault(x => x.AttributeType.FullName == this.WeavingContext.CortexNetApiComputedAttribute.FullName);

            // default enhancer
            var defaultEhancerType = this.WeavingContext.CortexNetTypesDeepEnhancer;
            TypeReference equalityComparerType = null;

            var keepAlive = false;
            var requiresReaction = false;

            int boolIndex = 0;

            foreach (var ca in computedAttribute.ConstructorArguments)
            {
                if (ca.Type == moduleDefinition.TypeSystem.String)
                {
                    computedName = ca.Value as string;
                }

                if (ca.Type.FullName == typeof(Type).FullName)
                {
                    if (ca.Value != null && ca.Value is TypeReference)
                    {
                        equalityComparerType = moduleDefinition.ImportReference(ca.Value as TypeReference);
                    }
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

            FieldDefinition observableObjectField = this.CreateObservableObjectField(declaringType, defaultEhancerType);

            if (propertyDefinition.GetMethod != null)
            {
                var methodDefinition = propertyDefinition.GetMethod;

                // push IL code for initialization of a computed member to the queue to emit in the ISharedState setter.
                this.ProcessorQueue.SharedStateAssignmentQueue.Enqueue(
                    (declaringType,
                    false,
                    (processor, sharedStateBackingField) => this.EmitComputedMemberAdd(
                        processor,
                        computedName,
                        methodDefinition,
                        propertyDefinition.SetMethod,
                        equalityComparerType,
                        requiresReaction,
                        keepAlive,
                        observableObjectField)));

                var fieldAttributes = FieldAttributes.Private;
                if (methodDefinition.IsStatic)
                {
                    fieldAttributes |= FieldAttributes.Static;
                }

                // add entrance counter field.
                var entranceCounterDefinition = declaringType.CreateField(moduleDefinition.TypeSystem.Int32, $"{InnerCounterFieldPrefix}{methodDefinition.Name}_EntranceCount", this.WeavingContext, fieldAttributes);

                // extend the method body.
                this.ExtendGetMethodBody(methodDefinition, computedName, entranceCounterDefinition, observableObjectField);
            }

            if (propertyDefinition.SetMethod != null)
            {
                var methodDefinition = propertyDefinition.SetMethod;

                var fieldAttributes = FieldAttributes.Private;
                if (methodDefinition.IsStatic)
                {
                    fieldAttributes |= FieldAttributes.Static;
                }

                // add entrance counter field.
                var entranceCounterDefinition = declaringType.CreateField(moduleDefinition.TypeSystem.Int32, $"{InnerCounterFieldPrefix}{methodDefinition.Name}_EntranceCount", this.WeavingContext, fieldAttributes);

                // extend the method body.
                this.ExtendSetMethodBody(methodDefinition, computedName, entranceCounterDefinition, observableObjectField);
            }
        }

        /// <summary>
        /// Extends the set method body of the Computed method.
        /// </summary>
        /// <param name="methodDefinition">The method Definition to extend.</param>
        /// <param name="computedName">The name of the computed member.</param>
        /// <param name="counterFieldDefinition">The entrance counter to register recursive calls.</param>
        /// <param name="observableObjectField">The observable object field.</param>
        private void ExtendSetMethodBody(MethodDefinition methodDefinition, string computedName, FieldDefinition counterFieldDefinition, FieldDefinition observableObjectField)
        {
            var processor = methodDefinition.Body.GetILProcessor();

            var originalStart = methodDefinition.Body.Instructions.First();
            var originalEnd = methodDefinition.Body.Instructions.Last();

            var observableObjectType = this.WeavingContext.CortexNetTypesObservableObject.Resolve();
            var observableObjectWritePropertyMethod = new GenericInstanceMethod(observableObjectType.Methods.FirstOrDefault(m => m.Name == "Write"));
            observableObjectWritePropertyMethod.GenericArguments.Add(methodDefinition.Parameters[0].ParameterType);

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

                // call the write method on the observable object.
                processor.Create(OpCodes.Ldstr, computedName),
                processor.Create(OpCodes.Ldarg_1),
                processor.Create(OpCodes.Callvirt, this.ParentWeaver.ModuleDefinition.ImportReference(observableObjectWritePropertyMethod)),

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
        /// Weaves a method that is decorated with a ComputedAttribute.
        /// </summary>
        /// <param name="methodDefinition">The method definition of the decorated method.</param>
        private void WeaveMethod(MethodDefinition methodDefinition)
        {
            // property name
            var computedName = methodDefinition.Name;
            var methodReturnType = methodDefinition.ReturnType;
            var moduleDefinition = methodDefinition.Module;
            var declaringType = methodDefinition.DeclaringType;

            var computedAttribute = methodDefinition.CustomAttributes.SingleOrDefault(x => x.AttributeType.FullName == this.WeavingContext.CortexNetApiComputedAttribute.FullName);

            // default enhancer
            var defaultEhancerType = this.WeavingContext.CortexNetTypesDeepEnhancer;
            TypeReference equalityComparerType = null;

            var keepAlive = false;
            var requiresReaction = false;

            int boolIndex = 0;

            foreach (var ca in computedAttribute.ConstructorArguments)
            {
                if (ca.Type == moduleDefinition.TypeSystem.String)
                {
                    computedName = ca.Value as string;
                }

                if (ca.Value != null && ca.Value is TypeReference)
                {
                    equalityComparerType = moduleDefinition.ImportReference(ca.Value as TypeReference);
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

            FieldDefinition observableObjectField = this.CreateObservableObjectField(declaringType, defaultEhancerType);

            // push IL code for initialization of a computed member to the queue to emit in the ISharedState setter.
            this.ProcessorQueue.SharedStateAssignmentQueue.Enqueue(
                (declaringType,
                false,
                (processor, sharedStateBackingField) => this.EmitComputedMemberAdd(
                    processor,
                    computedName,
                    methodDefinition,
                    null,
                    equalityComparerType,
                    requiresReaction,
                    keepAlive,
                    observableObjectField)));

            var fieldAttributes = FieldAttributes.Private;
            if (methodDefinition.IsStatic)
            {
                fieldAttributes |= FieldAttributes.Static;
            }

            // add entrance counter field.
            var entranceCounterDefinition = declaringType.CreateField(moduleDefinition.TypeSystem.Int32, $"{InnerCounterFieldPrefix}{methodDefinition.Name}_EntranceCount", this.WeavingContext, fieldAttributes);

            // extend the method body.
            this.ExtendGetMethodBody(methodDefinition, computedName, entranceCounterDefinition, observableObjectField);
        }

        /// <summary>
        /// Creates an ObservableObject field on the declaring type.
        /// </summary>
        /// <param name="declaringType">The declaring type.</param>
        /// <param name="defaultEhancerType">The default enhancer type.</param>
        /// <returns>A field definition for the Observable Object field.</returns>
        private FieldDefinition CreateObservableObjectField(TypeDefinition declaringType, TypeReference defaultEhancerType)
        {
            // get or create the ObservableObjectField.
            FieldDefinition observableObjectField = declaringType.Fields.FirstOrDefault(x => x.FieldType.FullName == this.WeavingContext.CortexNetTypesObservableObject.FullName);
            if (observableObjectField is null)
            {
                var observableFieldTypeReference = this.WeavingContext.CortexNetTypesObservableObject;
                observableObjectField = declaringType.CreateField(observableFieldTypeReference, InnerObservableObjectFieldName, this.WeavingContext);

                // push IL code for initialization of observableObject to the queue to emit in the ISharedState setter.
                this.ProcessorQueue.SharedStateAssignmentQueue.Enqueue(
                    (declaringType,
                    false,
                    (processor, sharedStateBackingField) => this.EmitObservableObjectInit(
                        processor,
                        declaringType.Name,
                        defaultEhancerType,
                        sharedStateBackingField,
                        observableObjectField)));
            }

            return observableObjectField;
        }

        /// <summary>
        /// Extends the method body of the Computed method.
        /// </summary>
        /// <param name="methodDefinition">The method Definition to extend.</param>
        /// <param name="computedName">The name of the computed member.</param>
        /// <param name="counterFieldDefinition">The entrance counter to register recursive calls.</param>
        /// <param name="observableObjectField">The observable object field.</param>
        private void ExtendGetMethodBody(MethodDefinition methodDefinition, string computedName, FieldDefinition counterFieldDefinition, FieldDefinition observableObjectField)
        {
            var processor = methodDefinition.Body.GetILProcessor();

            var originalStart = methodDefinition.Body.Instructions.First();
            var originalEnd = methodDefinition.Body.Instructions.Last();

            var observableObjectType = this.WeavingContext.CortexNetTypesObservableObject.Resolve();
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
        /// <param name="setMethodDefinition">The methodDefinition of the setter.</param>
        /// <param name="equalityComparerType">The equality comparer type.</param>
        /// <param name="requiresReaction">Whether the computed value requires a reaction.</param>
        /// <param name="keepAlive">Whether to keep the computed value alive.</param>
        /// <param name="observableObjectField">A <see cref="FieldDefinition"/> for the field where the ObservableObject instance is kept.</param>
        private void EmitComputedMemberAdd(
            ILProcessor processor,
            string computedName,
            MethodDefinition methodDefinition,
            MethodDefinition setMethodDefinition,
            TypeReference equalityComparerType,
            bool requiresReaction,
            bool keepAlive,
            FieldDefinition observableObjectField)
        {
            var module = methodDefinition.Module;
            var functionType = methodDefinition.GetFunctionType(this.WeavingContext);

            var observableObjectType = this.WeavingContext.CortexNetTypesObservableObject.Resolve();
            var observableObjectAddComputedMethod = new GenericInstanceMethod(observableObjectType.Methods.FirstOrDefault(m => m.Name == "AddComputedMember"));
            observableObjectAddComputedMethod.GenericArguments.Add(methodDefinition.ReturnType);

            MethodReference functionTypeConstructorReference = functionType.Resolve().Methods.Single(x => x.IsConstructor && !x.IsStatic);

            functionTypeConstructorReference = module.ImportReference(functionTypeConstructorReference.GetGenericMethodOnInstantance(functionType));
            var computedValueOptionsType = this.WeavingContext.CortexNetComputedValueOptions.Resolve();
            var computedValueOptionsConstructor = computedValueOptionsType.Methods.Single(x => x.IsConstructor && !x.IsStatic);

            var computedValueOptionsInstanceType = new GenericInstanceType(computedValueOptionsType);
            computedValueOptionsInstanceType.GenericArguments.Add(methodDefinition.ReturnType);

            var computedValueOptionsConstructorReference = module.ImportReference(computedValueOptionsConstructor.GetGenericMethodOnInstantance(computedValueOptionsInstanceType));
            var setContextMethod = computedValueOptionsType.Methods.Single(x => x.Name == "set_Context");
            var setContextReference = module.ImportReference(setContextMethod.GetGenericMethodOnInstantance(computedValueOptionsInstanceType));
            var setRequiresReactionMethod = computedValueOptionsType.Methods.Single(x => x.Name == "set_RequiresReaction");
            var setRequiresReactionReference = module.ImportReference(setRequiresReactionMethod.GetGenericMethodOnInstantance(computedValueOptionsInstanceType));
            var setKeepAliveMethod = computedValueOptionsType.Methods.Single(x => x.Name == "set_KeepAlive");
            var setKeepAliveReference = module.ImportReference(setKeepAliveMethod.GetGenericMethodOnInstantance(computedValueOptionsInstanceType));

            // reference to this.
            processor.Emit(OpCodes.Ldarg_0);

            // load observable object to call "AddComputedMember";
            processor.Emit(OpCodes.Ldfld, observableObjectField);

            // first argument, name of computed.
            processor.Emit(OpCodes.Ldstr, computedName);

            // first argument of ComputedValueOptions, getter definition.
            processor.Emit(OpCodes.Ldarg_0);
            processor.Emit(OpCodes.Ldftn, methodDefinition);
            processor.Emit(OpCodes.Newobj, functionTypeConstructorReference);

            // second argument of ComputedValueOptions, name again (kind of double. Is the same in Mobx. Maybe fix this?)
            processor.Emit(OpCodes.Ldstr, computedName);
            processor.Emit(OpCodes.Newobj, computedValueOptionsConstructorReference);

            // object initializers for computedvalueoptions. We dup the reference to call setters. First set context.
            processor.Emit(OpCodes.Dup);
            processor.Emit(OpCodes.Ldarg_0);
            processor.Emit(OpCodes.Callvirt, setContextReference);

            // set requiresReaction.
            processor.Emit(OpCodes.Dup);
            processor.Emit(requiresReaction ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
            processor.Emit(OpCodes.Callvirt, setRequiresReactionReference);

            // set keepAlive.
            processor.Emit(OpCodes.Dup);
            processor.Emit(keepAlive ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
            processor.Emit(OpCodes.Callvirt, setKeepAliveReference);

            // custom equality comparer in the attribute.
            if (equalityComparerType != null)
            {
                var setEqualityComparerMethod = computedValueOptionsType.Methods.Single(x => x.Name == "set_EqualityComparer");
                var setEqualityComparerReference = module.ImportReference(setEqualityComparerMethod.GetGenericMethodOnInstantance(computedValueOptionsInstanceType));
                MethodReference equalityComparerConstructorReference = equalityComparerType.Resolve().Methods.SingleOrDefault(x => x.IsConstructor && !x.IsStatic && x.Parameters.Count == 0);

                // The equalitycomparer needs to have a parameterless constructor.
                if (equalityComparerConstructorReference == null)
                {
                    this.ParentWeaver.WriteWarning(string.Format(
                        CultureInfo.CurrentCulture,
                        Resources.NoParameterLessConstructorForEqualityComparer,
                        equalityComparerType.Name,
                        computedName,
                        methodDefinition.DeclaringType.Name));
                }
                else
                {
                    if (equalityComparerType.IsGenericInstance)
                    {
                        equalityComparerConstructorReference = equalityComparerConstructorReference.GetGenericMethodOnInstantance(equalityComparerType);
                    }

                    processor.Emit(OpCodes.Dup);
                    processor.Emit(OpCodes.Newobj, module.ImportReference(equalityComparerConstructorReference));
                    processor.Emit(OpCodes.Callvirt, setEqualityComparerReference);
                }
            }

            // Add setter to the computed value.
            if (setMethodDefinition != null)
            {
                var setSetterMethod = computedValueOptionsType.Methods.Single(x => x.Name == "set_Setter");
                var setSetterReference = module.ImportReference(setSetterMethod.GetGenericMethodOnInstantance(computedValueOptionsInstanceType));
                var actionType = setMethodDefinition.GetActionType(this.WeavingContext);

                MethodReference actionTypeConstructorReference = actionType.Resolve().Methods.Single(x => x.IsConstructor && !x.IsStatic);

                actionTypeConstructorReference = module.ImportReference(actionTypeConstructorReference.GetGenericMethodOnInstantance(actionType));

                processor.Emit(OpCodes.Dup);
                processor.Emit(OpCodes.Ldarg_0);
                processor.Emit(OpCodes.Ldftn, setMethodDefinition);
                processor.Emit(OpCodes.Newobj, actionTypeConstructorReference);
                processor.Emit(OpCodes.Callvirt, setSetterReference);
            }

            // call AddComputedMember.
            processor.Emit(OpCodes.Callvirt, module.ImportReference(observableObjectAddComputedMethod));
        }
    }
}
