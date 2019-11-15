// <copyright file="ObservableObjectWeaverBase.cs" company="Jan-Willem Spuij">
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
    using global::Fody;
    using Mono.Cecil;
    using Mono.Cecil.Cil;

    /// <summary>
    /// Base class for Weavers that operate on an ObservableObject.
    /// </summary>
    internal abstract class ObservableObjectWeaverBase
    {
        /// <summary>
        /// The name of an Inner ObservableObjectField.
        /// </summary>
        protected const string InnerObservableObjectFieldName = "cortex_Net_Dogf73jc08asiy_ObservableObject";

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableObjectWeaverBase"/> class.
        /// </summary>
        /// <param name="parentWeaver">The parent weaver.</param>
        /// <param name="processorQueue">The processor queue to add delegates to.</param>
        /// <param name="weavingContext">The resolved types necessary by this weaver.</param>
        protected ObservableObjectWeaverBase(ModuleWeaver parentWeaver, ISharedStateAssignmentILProcessorQueue processorQueue, WeavingContext weavingContext)
        {
            this.ParentWeaver = parentWeaver ?? throw new ArgumentNullException(nameof(parentWeaver));
            this.ProcessorQueue = processorQueue ?? throw new ArgumentNullException(nameof(processorQueue));
            this.WeavingContext = weavingContext ?? throw new ArgumentNullException(nameof(weavingContext));
        }

        /// <summary>
        /// Gets the parent weaver of this Weaver.
        /// </summary>
        protected ModuleWeaver ParentWeaver { get; private set; }

        /// <summary>
        /// Gets the processor queue that contains delegates to be processed when the shared state is assigned.
        /// </summary>
        protected ISharedStateAssignmentILProcessorQueue ProcessorQueue { get; private set; }

        /// <summary>
        /// Gets the Weaving context.
        /// </summary>
        protected WeavingContext WeavingContext { get; }

        /// <summary>
        /// Emits the IL code to initialize the observable object. for the IReactiveObject.SharedState setter.
        /// </summary>
        /// <param name="processor">The <see cref="ILProcessor"/> instance that will generate the setter body.</param>
        /// <param name="objectName">The name of the object.</param>
        /// <param name="enhancerType">The type reference of the enhancer to use.</param>
        /// <param name="sharedStateBackingField">A <see cref="FieldReference"/> to the backing field of the Shared STate.</param>
        /// <param name="observableObjectField">A <see cref="FieldDefinition"/> for the field where the ObservableObject instance is kept.</param>
        protected void EmitObservableObjectInit(
            ILProcessor processor,
            string objectName,
            TypeReference enhancerType,
            FieldReference sharedStateBackingField,
            FieldDefinition observableObjectField)
        {
            var getTypeFromHandlerMethod = this.ParentWeaver.ModuleDefinition.ImportReference(this.ParentWeaver.FindType("System.Type").Methods.Single(x => x.Name == "GetTypeFromHandle"));
            var getEnhancerMethod = this.ParentWeaver.ModuleDefinition.ImportReference(this.WeavingContext.CortexNetCoreActionExtensions.Resolve().Methods.Single(x => x.Name == "GetEnhancer"));
            var observableObjectConstructor = this.ParentWeaver.ModuleDefinition.ImportReference(this.WeavingContext.CortexNetTypesObservableObject.Resolve().Methods.Single(x => x.IsConstructor));

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
    }
}
