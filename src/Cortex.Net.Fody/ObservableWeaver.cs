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
    using System.Runtime.CompilerServices;
    using System.Text;
    using Cortex.Net.Api;
    using Cortex.Net.Fody.Properties;
    using Cortex.Net.Types;
    using global::Fody;
    using Mono.Cecil;
    using Mono.Cecil.Cil;

    /// <summary>
    /// Weaves properties and classes decorated with an <see cref="ObservableAttribute"/>.
    /// </summary>
    internal class ObservableWeaver
    {
        /// <summary>
        /// The name of an Inner ObservableObjectField.
        /// </summary>
        private const string InnerObservableObjectFieldName = "cortex_Net_Dogf73jc08asiy_ObservableObject";

        /// <summary>
        /// A reference to the parent Cortex.Net weaver.
        /// </summary>
        private readonly BaseModuleWeaver cortexWeaver;

        /// <summary>
        /// The queue to add ILProcessor actions to.
        /// </summary>
        private readonly ISharedStateAssignmentILProcessorQueue processorQueue;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableWeaver"/> class.
        /// </summary>
        /// <param name="parentWeaver">A reference to the Parent Cortex.Net weaver.</param>
        /// <param name="processorQueue">The queue to add ILProcessor actions to.</param>
        /// <exception cref="ArgumentNullException">When any of the arguments is null.</exception>
        public ObservableWeaver(BaseModuleWeaver parentWeaver, ISharedStateAssignmentILProcessorQueue processorQueue)
        {
            this.cortexWeaver = parentWeaver ?? throw new ArgumentNullException(nameof(parentWeaver));
            this.processorQueue = processorQueue ?? throw new ArgumentNullException(nameof(processorQueue));
        }

        /// <summary>
        /// Executes this observable weaver.
        /// </summary>
        internal void Execute()
        {
            var decoratedProperties = from t in this.cortexWeaver.ModuleDefinition.GetTypes()
                                   from m in t.Properties
                                   where
                                      t != null &&
                                      t.IsClass &&
                                      t.BaseType != null &&
                                      m != null &&
                                      m.CustomAttributes != null &&
                                      m.CustomAttributes.Any(x => x.AttributeType.FullName == typeof(ObservableAttribute).FullName)
                                   select m;

            foreach (var decoratedProperty in decoratedProperties.ToList())
            {
                this.WeaveProperty(decoratedProperty);
            }

            var decoratedClasses = from t in this.cortexWeaver.ModuleDefinition.GetTypes()
                                      where
                                         t != null &&
                                         t.IsClass &&
                                         t.BaseType != null &&
                                         t.CustomAttributes != null &&
                                         t.CustomAttributes.Any(x => x.AttributeType.FullName == typeof(ObservableAttribute).FullName)
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
        }

        /// <summary>
        /// Weaves a property on an observable object.
        /// </summary>
        /// <param name="property">The property to make observable.</param>
        private void WeaveProperty(PropertyDefinition property)
        {
            var module = property.Module;
            var getMethod = property.GetMethod;
            var setMethod = property.SetMethod;
            var declaringType = property.DeclaringType;

            if (!getMethod.CustomAttributes.Any(x => x.AttributeType.FullName == typeof(CompilerGeneratedAttribute).FullName))
            {
                this.cortexWeaver.LogWarning(string.Format(CultureInfo.CurrentCulture, Resources.PropertyNotAutogenerated, property.Name, declaringType.Name));
                return;
            }

            // property name
            var propertyName = property.Name;
            var observableAttribute = property.CustomAttributes.Single(x => x.AttributeType.FullName == typeof(ObservableAttribute).FullName);

            // default enhancer
            var defaultEhancerType = module.ImportReference(typeof(DeepEnhancer));
            var enhancerType = defaultEhancerType;
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

            // Get the backing field and remove it.
            var backingField = property.GetBackingField();
            module.ImportReference(backingField);
            declaringType.Fields.Remove(backingField);

            // get or create the ObservableObjectField.
            FieldDefinition observableObjectField = declaringType.Fields.FirstOrDefault(x => x.FieldType.FullName == typeof(ObservableObject).FullName);
            if (observableObjectField is null)
            {
                var observableFieldTypeReference = module.ImportReference(typeof(ObservableObject));
                observableObjectField = declaringType.CreateField(observableFieldTypeReference, InnerObservableObjectFieldName);

                // push IL code for initialization of observableObject to the queue to emit in the ISharedState setter.
                this.processorQueue.SharedStateAssignmentQueue.Enqueue(
                    (declaringType,
                    (processor, sharedStateBackingField) => this.EmitObservableObjectInit(
                        processor,
                        declaringType.Name,
                        defaultEhancerType,
                        sharedStateBackingField,
                        observableObjectField)));
            }

            // push IL code for initialization of a property to the queue to emit in the ISharedState setter.
            this.processorQueue.SharedStateAssignmentQueue.Enqueue(
                (declaringType,
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

            var observableObjectType = this.cortexWeaver.ModuleDefinition.ImportReference(typeof(ObservableObject)).Resolve();
            var observableObjectReadPropertyMethod = new GenericInstanceMethod(observableObjectType.Methods.FirstOrDefault(m => m.Name == "Read"));
            observableObjectReadPropertyMethod.GenericArguments.Add(propertyType);

            processor.Emit(OpCodes.Ldarg_0);
            processor.Emit(OpCodes.Ldfld, observableObjectField);
            processor.Emit(OpCodes.Ldstr, propertyName);
            processor.Emit(OpCodes.Callvirt, this.cortexWeaver.ModuleDefinition.ImportReference(observableObjectReadPropertyMethod));
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

            var observableObjectType = this.cortexWeaver.ModuleDefinition.ImportReference(typeof(ObservableObject)).Resolve();
            var observableObjectWritePropertyMethod = new GenericInstanceMethod(observableObjectType.Methods.FirstOrDefault(m => m.Name == "Write"));
            observableObjectWritePropertyMethod.GenericArguments.Add(propertyType);

            processor.Emit(OpCodes.Ldarg_0);
            processor.Emit(OpCodes.Ldfld, observableObjectField);
            processor.Emit(OpCodes.Ldstr, propertyName);
            processor.Emit(OpCodes.Ldarg_1);
            processor.Emit(OpCodes.Callvirt, this.cortexWeaver.ModuleDefinition.ImportReference(observableObjectWritePropertyMethod));
            processor.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// Emits the IL code to initialize the observable object. for the <see cref="IObservableObject.SharedState"/> setter.
        /// </summary>
        /// <param name="processor">The <see cref="ILProcessor"/> instance that will generate the setter body.</param>
        /// <param name="objectName">The name of the object.</param>
        /// <param name="enhancerType">The type reference of the enhancer to use.</param>
        /// <param name="sharedStateBackingField">A <see cref="FieldReference"/> to the backing field of the Shared STate.</param>
        /// <param name="observableObjectField">A <see cref="FieldDefinition"/> for the field where the <see cref="ObservableObject"/> instance is kept.</param>
        private void EmitObservableObjectInit(
            ILProcessor processor,
            string objectName,
            TypeReference enhancerType,
            FieldReference sharedStateBackingField,
            FieldDefinition observableObjectField)
        {
            var getTypeFromHandlerMethod = this.cortexWeaver.ModuleDefinition.ImportReference(this.cortexWeaver.ModuleDefinition.ImportReference(typeof(Type)).Resolve().Methods.Single(x => x.Name == "GetTypeFromHandle"));
            var getEnhancerMethod = this.cortexWeaver.ModuleDefinition.ImportReference(this.cortexWeaver.ModuleDefinition.ImportReference(typeof(Core.ActionExtensions)).Resolve().Methods.Single(x => x.Name == "GetEnhancer"));
            var observableObjectConstructor = this.cortexWeaver.ModuleDefinition.ImportReference(this.cortexWeaver.ModuleDefinition.ImportReference(typeof(ObservableObject)).Resolve().Methods.Single(x => x.IsConstructor));

            var instructions = new List<Instruction>
            {
                // this.observableObject = new ObservableObject(objectName, Cortex.Net.Core.ActionExtensions.GetEnhancer((ISharedState)sharedState, typeof(DeepEnhancer)), sharedState, null);
                processor.Create(OpCodes.Ldarg_0),
                processor.Create(OpCodes.Ldstr, objectName),
                processor.Create(OpCodes.Ldarg_0),
                processor.Create(OpCodes.Ldfld, sharedStateBackingField),
                processor.Create(OpCodes.Ldtoken, enhancerType),
                processor.Create(OpCodes.Call, getTypeFromHandlerMethod),
                processor.Create(OpCodes.Call, getEnhancerMethod),
                processor.Create(OpCodes.Ldarg_1),
                processor.Create(OpCodes.Ldnull),
                processor.Create(OpCodes.Newobj, observableObjectConstructor),
                processor.Create(OpCodes.Stfld, observableObjectField),
            };

            foreach (var instruction in instructions)
            {
                processor.Append(instruction);
            }
        }

        /// <summary>
        /// Emits the IL code to add an Observable property to the observable object. for the <see cref="IObservableObject.SharedState"/> setter.
        /// </summary>
        /// <param name="processor">The <see cref="ILProcessor"/> instance that will generate the setter body.</param>
        /// <param name="propertyName">The name of the object.</param>
        /// <param name="propertyType">The type reference of Property.</param>
        /// <param name="enhancerType">The type reference of the enhancer to use.</param>
        /// <param name="sharedStateBackingField">A <see cref="FieldReference"/> to the backing field of the Shared STate.</param>
        /// <param name="observableObjectField">A <see cref="FieldDefinition"/> for the field where the <see cref="ObservableObject"/> instance is kept.</param>
        private void EmitObservablePropertyAdd(
            ILProcessor processor,
            string propertyName,
            TypeReference propertyType,
            TypeReference enhancerType,
            FieldReference sharedStateBackingField,
            FieldDefinition observableObjectField)
        {
            var typeVariable = new VariableDefinition(propertyType);
            var getTypeFromHandlerMethod = this.cortexWeaver.ModuleDefinition.ImportReference(this.cortexWeaver.ModuleDefinition.ImportReference(typeof(Type)).Resolve().Methods.Single(x => x.Name == "GetTypeFromHandle"));
            var getEnhancerMethod = this.cortexWeaver.ModuleDefinition.ImportReference(this.cortexWeaver.ModuleDefinition.ImportReference(typeof(Core.ActionExtensions)).Resolve().Methods.Single(x => x.Name == "GetEnhancer"));

            var observableObjectType = this.cortexWeaver.ModuleDefinition.ImportReference(typeof(ObservableObject)).Resolve();
            var observableObjectAddPropertyMethod = new GenericInstanceMethod(observableObjectType.Methods.FirstOrDefault(m => m.Name == "AddObservableProperty"));
            observableObjectAddPropertyMethod.GenericArguments.Add(propertyType);

            int index = processor.Body.Variables.Count;
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
                processor.Create(OpCodes.Callvirt, this.cortexWeaver.ModuleDefinition.ImportReference(observableObjectAddPropertyMethod)),
            };

            foreach (var instruction in instructions)
            {
                processor.Append(instruction);
            }
        }
    }
}
