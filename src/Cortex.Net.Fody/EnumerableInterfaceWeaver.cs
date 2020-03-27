// <copyright file="EnumerableInterfaceWeaver.cs" company="Jan-Willem Spuij">
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
    using System.Text;
    using Cortex.Net.Fody.Properties;
    using global::Fody;
    using Mono.Cecil;
    using Mono.Cecil.Cil;

    /// <summary>
    /// Weaver for enumerable types.
    /// </summary>
    public class EnumerableInterfaceWeaver : IEnumerableInterfaceWeaver
    {
        // generic replaceable enumerables.
        private static readonly string IGenericListName = typeof(IList<>).FullName;
        private static readonly string IGenericReadonlyListName = typeof(IReadOnlyList<>).FullName;
        private static readonly string IGenericSetName = typeof(ISet<>).FullName;
        private static readonly string IGenericDictionaryName = typeof(IDictionary<,>).FullName;
        private static readonly string IGenericReadonlyDictionaryName = typeof(IReadOnlyDictionary<,>).FullName;
        private static readonly string IGenericCollectionName = typeof(ICollection<>).FullName;
        private static readonly string IGenericReadonlyCollectionName = typeof(IReadOnlyCollection<>).FullName;
        private static readonly string IGenericEnumerableName = typeof(IEnumerable<>).FullName;

        // non generic replaceable enumerables.
        private static readonly string IListName = typeof(System.Collections.IList).FullName;
        private static readonly string IDictionaryName = typeof(System.Collections.IDictionary).FullName;
        private static readonly string ICollectionName = typeof(System.Collections.ICollection).FullName;
        private static readonly string IEnumerableName = typeof(System.Collections.IEnumerable).FullName;

        /// <summary>
        /// The parent weaver of this weaver.
        /// </summary>
        private readonly ModuleWeaver parentWeaver;

        /// <summary>
        /// The processor queue that contains delegates to be processed when the shared state is assigned.
        /// </summary>
        private readonly ISharedStateAssignmentILProcessorQueue processorQueue;

        /// <summary>
        /// Weaving context.
        /// </summary>
        private readonly WeavingContext weavingContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumerableInterfaceWeaver"/> class.
        /// </summary>
        /// <param name="parentWeaver">The parent weaver of this CollectionWeaver.</param>
        /// <param name="processorQueue">The queue to add ILProcessor actions to.</param>
        /// <param name="weavingContext">The resolved types necessary by this weaver.</param>
        public EnumerableInterfaceWeaver(ModuleWeaver parentWeaver, ISharedStateAssignmentILProcessorQueue processorQueue, WeavingContext weavingContext)
        {
            this.parentWeaver = parentWeaver ?? throw new ArgumentNullException(nameof(parentWeaver));
            this.processorQueue = processorQueue ?? throw new ArgumentNullException(nameof(processorQueue));
            this.weavingContext = weavingContext ?? throw new ArgumentNullException(nameof(weavingContext));
        }

        /// <summary>
        /// Weaves an enumerable property.
        /// </summary>
        /// <param name="propertyDefinition">The property to weave.</param>
        /// <param name="defaultEnhancer">The default enhancer type for the observable.</param>
        public void WeaveEnumerableProperty(PropertyDefinition propertyDefinition, TypeReference defaultEnhancer)
        {
            if (propertyDefinition is null)
            {
                throw new ArgumentNullException(nameof(propertyDefinition));
            }

            if (defaultEnhancer is null)
            {
                throw new ArgumentNullException(nameof(defaultEnhancer));
            }

            var propertyTypeName = propertyDefinition.PropertyType.Resolve().FullName;

            if (propertyTypeName == IGenericDictionaryName ||
                propertyTypeName == IGenericReadonlyDictionaryName ||
                propertyTypeName == IDictionaryName)
            {
                this.ReassignEnumerable(propertyDefinition, this.weavingContext.CortexNetTypesObservableDictionary, defaultEnhancer);
            }
            else if (propertyTypeName == IGenericSetName)
            {
                this.ReassignEnumerable(propertyDefinition, this.weavingContext.CortexNetTypesObservableSet, defaultEnhancer);
            }
            else if (propertyTypeName == IGenericListName ||
                     propertyTypeName == IGenericReadonlyListName ||
                     propertyTypeName == IGenericCollectionName ||
                     propertyTypeName == IGenericReadonlyCollectionName ||
                     propertyTypeName == IGenericEnumerableName ||
                     propertyTypeName == IListName ||
                     propertyTypeName == ICollectionName ||
                     propertyTypeName == IEnumerableName)
            {
                this.ReassignEnumerable(propertyDefinition, this.weavingContext.CortexNetTypesObservableCollection, defaultEnhancer);
            }
            else
            {
                this.parentWeaver.WriteWarning(string.Format(CultureInfo.CurrentCulture, Resources.NonReplaceableCollection, propertyDefinition.Name, propertyTypeName));
            }
        }

        /// <summary>
        /// Reassign an Enumerable property by calling a constructor of an observable enumerable type.
        /// </summary>
        /// <param name="propertyDefinition">The property definition to use to replace.</param>
        /// <param name="observableEnumerableType">The observable enumerable type.</param>
        /// <param name="defaultEnhancer">The default enhancer to use.</param>
        private void ReassignEnumerable(PropertyDefinition propertyDefinition, TypeReference observableEnumerableType, TypeReference defaultEnhancer)
        {
            var module = propertyDefinition.Module;
            var declaringType = propertyDefinition.DeclaringType;
            var importedType = observableEnumerableType;
            var genricConstructor = importedType.Resolve().Methods.Where(x => x.IsConstructor && !x.IsStatic).OrderBy(x => x.Parameters.Count).First();
            MethodReference constructorReference;

            // property name
            var propertyName = propertyDefinition.Name;
            var observableAttribute = propertyDefinition.CustomAttributes.SingleOrDefault(x => x.AttributeType.FullName == this.weavingContext.CortexNetApiObservableAttribute.FullName);

            // default enhancer
            var defaultEhancerType = module.ImportReference(defaultEnhancer);
            var enhancerType = defaultEhancerType;

            var propertyBackingField = propertyDefinition.GetBackingField();

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

            if (propertyDefinition.PropertyType.IsGenericInstance)
            {
                var instantiatedType = new GenericInstanceType(importedType);
                foreach (var argument in (propertyDefinition.PropertyType as GenericInstanceType).GenericArguments)
                {
                    instantiatedType.GenericArguments.Add(module.ImportReference(argument));
                }

                constructorReference = genricConstructor.GetGenericMethodOnInstantance(module.ImportReference(instantiatedType));
            }
            else
            {
                var instantiatedType = new GenericInstanceType(importedType);
                instantiatedType.GenericArguments.Add(module.TypeSystem.Object);
                constructorReference = genricConstructor.GetGenericMethodOnInstantance(module.ImportReference(instantiatedType));
            }

            // push IL code for initialization of a property to the queue to emit in the ISharedState setter.
            this.processorQueue.SharedStateAssignmentQueue.Enqueue(
                (declaringType,
                false,
                (processor, sharedStateBackingField) => this.EmitObservableEnumerableCreation(
                    processor,
                    propertyName,
                    constructorReference,
                    enhancerType,
                    sharedStateBackingField,
                    propertyBackingField)));
        }

        /// <summary>
        /// Emits IL-Code for calling the constructor of an Observable Enumerable type.
        /// </summary>
        /// <param name="processor">The IL-Processor to use.</param>
        /// <param name="propertyName">The name of the property to use.</param>
        /// <param name="constructorReference">The constructor reference to use.</param>
        /// <param name="enhancerType">The enhancer type to use.</param>
        /// <param name="sharedStateBackingField">The shared state backing field.</param>
        /// <param name="propertyBackingField">The property backing field.</param>
        private void EmitObservableEnumerableCreation(
            ILProcessor processor,
            string propertyName,
            MethodReference constructorReference,
            TypeReference enhancerType,
            FieldReference sharedStateBackingField,
            FieldDefinition propertyBackingField)
        {
            var getTypeFromHandlerMethod = this.parentWeaver.ModuleDefinition.ImportReference(this.parentWeaver.FindTypeDefinition("System.Type").Methods.Single(x => x.Name == "GetTypeFromHandle"));
            var getEnhancerMethod = this.parentWeaver.ModuleDefinition.ImportReference(this.weavingContext.CortexNetCoreActionExtensions.Resolve().Methods.Single(x => x.Name == "GetEnhancer"));

            var sharedStateInterfaceImport = this.weavingContext.CortexNetISharedState;
            processor.Emit(OpCodes.Ldarg_0);
            processor.Emit(OpCodes.Ldarg_0);
            processor.Emit(OpCodes.Ldfld, sharedStateBackingField);
            processor.Emit(OpCodes.Dup);
            processor.Emit(OpCodes.Ldtoken, enhancerType);
            processor.Emit(OpCodes.Call, getTypeFromHandlerMethod);
            processor.Emit(OpCodes.Call, getEnhancerMethod);
            processor.Emit(OpCodes.Ldstr, propertyName);
            processor.Emit(OpCodes.Newobj, this.parentWeaver.ModuleDefinition.ImportReference(constructorReference));
            processor.Emit(OpCodes.Stfld, propertyBackingField);
        }
    }
}
