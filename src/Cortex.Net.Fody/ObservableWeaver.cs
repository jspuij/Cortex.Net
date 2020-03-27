// <copyright file="ObservableWeaver.cs" company="Jan-Willem Spuij">
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
    /// Weaves properties and classes decorated with an ObservableAttribute />.
    /// </summary>
    internal class ObservableWeaver : ObservableObjectWeaverBase
    {
        /// <summary>
        /// The weaver to use to pass enumerable implementations to.
        /// </summary>
        private readonly IEnumerableInterfaceWeaver enumerableInterfaceWeaver;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableWeaver"/> class.
        /// </summary>
        /// <param name="parentWeaver">A reference to the Parent Cortex.Net weaver.</param>
        /// <param name="enumerableInterfaceWeaver">An implementation of <see cref="IEnumerableInterfaceWeaver" />.</param>
        /// <param name="processorQueue">The queue to add ILProcessor actions to.</param>
        /// <param name="weavingContext">The resolved types necessary by this weaver.</param>
        /// <exception cref="ArgumentNullException">When any of the arguments is null.</exception>
        public ObservableWeaver(ModuleWeaver parentWeaver, IEnumerableInterfaceWeaver enumerableInterfaceWeaver,  ISharedStateAssignmentILProcessorQueue processorQueue, WeavingContext weavingContext)
            : base(parentWeaver, processorQueue, weavingContext)
        {
            this.enumerableInterfaceWeaver = enumerableInterfaceWeaver ?? throw new ArgumentNullException(nameof(enumerableInterfaceWeaver));
        }

        /// <summary>
        /// Executes this observable weaver.
        /// </summary>
        internal void Execute()
        {
            var decoratedProperties = from t in this.ParentWeaver.ModuleDefinition.GetTypes()
                                   from p in t.Properties
                                   where
                                      t != null &&
                                      t.IsClass &&
                                      t.BaseType != null &&
                                      p != null &&
                                      p.CustomAttributes != null &&
                                      p.CustomAttributes.Any(x => x.AttributeType.FullName == this.WeavingContext.CortexNetApiObservableAttribute.FullName)
                                   select p;

            foreach (var decoratedProperty in decoratedProperties.ToList())
            {
                if (decoratedProperty.PropertyType.IsReplaceableCollection(this.WeavingContext))
                {
                    this.enumerableInterfaceWeaver.WeaveEnumerableProperty(decoratedProperty, this.WeavingContext.CortexNetTypesDeepEnhancer);
                }
                else
                {
                    this.WeaveProperty(decoratedProperty, this.WeavingContext.CortexNetTypesDeepEnhancer);
                }
            }

            var decoratedClasses = from t in this.ParentWeaver.ModuleDefinition.GetTypes()
                                      where
                                         t != null &&
                                         t.IsClass &&
                                         t.BaseType != null &&
                                         t.CustomAttributes != null &&
                                         t.CustomAttributes.Any(x => x.AttributeType.FullName == this.WeavingContext.CortexNetApiObservableAttribute.FullName)
                                      select t;

            foreach (var decoratedClass in decoratedClasses.ToList())
            {
                this.WeaveClass(decoratedClass);
            }
        }

        /// <summary>
        /// Weaves an entire class.
        /// </summary>
        /// <param name="decoratedClass">The class that was decorated with the attribute.</param>
        private void WeaveClass(TypeDefinition decoratedClass)
        {
            var observableAttribute = decoratedClass.CustomAttributes.Single(x => x.AttributeType.FullName == this.WeavingContext.CortexNetApiObservableAttribute.FullName);

            var defaultEhancerType = this.WeavingContext.CortexNetTypesDeepEnhancer;
            var enhancerType = defaultEhancerType;
            foreach (var constructorArgument in observableAttribute.ConstructorArguments)
            {
                if (constructorArgument.Type.FullName == typeof(Type).FullName)
                {
                    if (constructorArgument.Value != null && constructorArgument.Value is TypeReference)
                    {
                        enhancerType = decoratedClass.Module.ImportReference(constructorArgument.Value as TypeReference);
                    }
                }

                if (constructorArgument.Type.FullName == typeof(Type).FullName)
                {
                }
            }

            foreach (var property in decoratedClass.Properties.Where(x => x.GetMethod != null && x.GetMethod.IsPublic))
            {
                if (property.PropertyType.IsReplaceableCollection(this.WeavingContext))
                {
                    this.enumerableInterfaceWeaver.WeaveEnumerableProperty(property, enhancerType);
                }
                else
                {
                    if (property.SetMethod != null && property.SetMethod.CustomAttributes.Any(x => x.AttributeType.FullName == this.WeavingContext.SystemRuntimeCompilerServicesCompilerGeneratedAttribute.FullName) &&
                        !property.SetMethod.Name.Contains("set_SharedState"))
                    {
                        this.WeaveProperty(property, enhancerType);
                    }
                }
            }
        }

        /// <summary>
        /// Weaves a property on an observable object.
        /// </summary>
        /// <param name="property">The property to make observable.</param>
        /// <param name="defaultEnhancer">The type of the default Enhancer.</param>
        private void WeaveProperty(PropertyDefinition property, TypeReference defaultEnhancer)
        {
            var module = property.Module;
            var getMethod = property.GetMethod;
            var setMethod = property.SetMethod;
            var declaringType = property.DeclaringType;

            if (!getMethod.CustomAttributes.Any(x => x.AttributeType.FullName == this.WeavingContext.SystemRuntimeCompilerServicesCompilerGeneratedAttribute.FullName))
            {
                this.ParentWeaver.WriteWarning(string.Format(CultureInfo.CurrentCulture, Resources.PropertyNotAutogenerated, property.Name, declaringType.Name));
                return;
            }

            if (setMethod == null)
            {
                this.ParentWeaver.WriteWarning(string.Format(CultureInfo.CurrentCulture, Resources.NoSetterForObservable, property.Name, declaringType.Name));
                return;
            }

            // property name
            var propertyName = property.Name;
            var observableAttribute = property.CustomAttributes.SingleOrDefault(x => x.AttributeType.FullName == this.WeavingContext.CortexNetApiObservableAttribute.FullName);

            // default enhancer
            var defaultEhancerType = defaultEnhancer;
            var enhancerType = defaultEhancerType;

            if (observableAttribute != null)
            {
                foreach (var constructorArgument in observableAttribute.ConstructorArguments)
                {
                    if (constructorArgument.Type.FullName == typeof(string).FullName)
                    {
                        propertyName = constructorArgument.Value as string;
                    }

                    if (constructorArgument.Type.FullName == typeof(Type).FullName)
                    {
                        enhancerType = module.ImportReference(constructorArgument.Value as Type);
                    }
                }
            }

            // Get the backing field and remove it.
            var backingField = property.GetBackingField();
            module.ImportReference(backingField);
            declaringType.Fields.Remove(backingField);

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

            // push IL code for initialization of a property to the queue to emit in the ISharedState setter.
            this.ProcessorQueue.SharedStateAssignmentQueue.Enqueue(
                (declaringType,
                false,
                (processor, sharedStateBackingField) => this.EmitObservablePropertyAdd(
                    processor,
                    propertyName,
                    property.PropertyType,
                    enhancerType,
                    sharedStateBackingField,
                    observableObjectField)));

            this.RewriteGetMethod(getMethod, observableObjectField, propertyName, property.PropertyType);
            this.RewriteSetMethod(setMethod, observableObjectField, propertyName, property.PropertyType);
        }

        /// <summary>
        /// Rewrites the Get Method of the Property.
        /// </summary>
        /// <param name="getMethod">The get method of the property.</param>
        /// <param name="observableObjectField">The field for the observable object.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="propertyType">The property Type.</param>
        private void RewriteGetMethod(MethodDefinition getMethod, FieldDefinition observableObjectField, string propertyName, TypeReference propertyType)
        {
            getMethod.Body.Instructions.Clear();
            var processor = getMethod.Body.GetILProcessor();

            var observableObjectType = this.WeavingContext.CortexNetTypesObservableObject.Resolve();
            var observableObjectReadPropertyMethod = new GenericInstanceMethod(observableObjectType.Methods.FirstOrDefault(m => m.Name == "Read"));
            observableObjectReadPropertyMethod.GenericArguments.Add(propertyType);

            processor.Emit(OpCodes.Ldarg_0);
            processor.Emit(OpCodes.Ldfld, observableObjectField);
            processor.Emit(OpCodes.Ldstr, propertyName);
            processor.Emit(OpCodes.Callvirt, this.ParentWeaver.ModuleDefinition.ImportReference(observableObjectReadPropertyMethod));
            processor.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// Rewrites the Set Method of the Property.
        /// </summary>
        /// <param name="setMethod">The set method of the property.</param>
        /// <param name="observableObjectField">The field for the observable object.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="propertyType">The property Type.</param>
        private void RewriteSetMethod(MethodDefinition setMethod, FieldDefinition observableObjectField, string propertyName, TypeReference propertyType)
        {
            setMethod.Body.Instructions.Clear();
            var processor = setMethod.Body.GetILProcessor();

            var observableObjectType = this.WeavingContext.CortexNetTypesObservableObject.Resolve();
            var observableObjectWritePropertyMethod = new GenericInstanceMethod(observableObjectType.Methods.FirstOrDefault(m => m.Name == "Write"));
            observableObjectWritePropertyMethod.GenericArguments.Add(propertyType);

            processor.Emit(OpCodes.Ldarg_0);
            processor.Emit(OpCodes.Ldfld, observableObjectField);
            processor.Emit(OpCodes.Ldstr, propertyName);
            processor.Emit(OpCodes.Ldarg_1);
            processor.Emit(OpCodes.Callvirt, this.ParentWeaver.ModuleDefinition.ImportReference(observableObjectWritePropertyMethod));
            processor.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// Emits the IL code to add an Observable property to the observable object. for the IReactiveObject.SharedState" setter.
        /// </summary>
        /// <param name="processor">The <see cref="ILProcessor"/> instance that will generate the setter body.</param>
        /// <param name="propertyName">The name of the object.</param>
        /// <param name="propertyType">The type reference of Property.</param>
        /// <param name="enhancerType">The type reference of the enhancer to use.</param>
        /// <param name="sharedStateBackingField">A <see cref="FieldReference"/> to the backing field of the Shared STate.</param>
        /// <param name="observableObjectField">A <see cref="FieldDefinition"/> for the field where the ObservableObject instance is kept.</param>
        private void EmitObservablePropertyAdd(
            ILProcessor processor,
            string propertyName,
            TypeReference propertyType,
            TypeReference enhancerType,
            FieldReference sharedStateBackingField,
            FieldDefinition observableObjectField)
        {
            var typeVariable = new VariableDefinition(propertyType);
            var getTypeFromHandlerMethod = this.ParentWeaver.ModuleDefinition.ImportReference(this.ParentWeaver.FindTypeDefinition("System.Type").Methods.Single(x => x.Name == "GetTypeFromHandle"));
            var getEnhancerMethod = this.ParentWeaver.ModuleDefinition.ImportReference(this.WeavingContext.CortexNetCoreActionExtensions.Resolve().Methods.Single(x => x.Name == "GetEnhancer"));

            var observableObjectType = this.WeavingContext.CortexNetTypesObservableObject.Resolve();
            var observableObjectAddPropertyMethod = new GenericInstanceMethod(observableObjectType.Methods.FirstOrDefault(m => m.Name == "AddObservableProperty"));
            observableObjectAddPropertyMethod.GenericArguments.Add(propertyType);

            int index = processor.Body.Variables.Count;
            processor.Body.InitLocals = true;
            processor.Body.Variables.Add(typeVariable);

            var instructions = new List<Instruction>
            {
                // this.observableObject.AddObservableProperty<T>(propertyName, default(propertyType), Cortex.Net.Core.ActionExtensions.GetEnhancer((ISharedState)sharedState, typeof(enhancer)));
                processor.Create(OpCodes.Ldarg_0),
                processor.Create(OpCodes.Ldfld, observableObjectField),
                processor.Create(OpCodes.Ldstr, propertyName),
                processor.Create(OpCodes.Ldloca_S, typeVariable),
                processor.Create(OpCodes.Initobj, propertyType),
                processor.Ldloc(index),
                processor.Create(OpCodes.Ldarg_0),
                processor.Create(OpCodes.Ldfld, sharedStateBackingField),
                processor.Create(OpCodes.Ldtoken, enhancerType),
                processor.Create(OpCodes.Call, getTypeFromHandlerMethod),
                processor.Create(OpCodes.Call, getEnhancerMethod),
                processor.Create(OpCodes.Callvirt, this.ParentWeaver.ModuleDefinition.ImportReference(observableObjectAddPropertyMethod)),
            };

            foreach (var instruction in instructions)
            {
                processor.Append(instruction);
            }
        }
    }
}
