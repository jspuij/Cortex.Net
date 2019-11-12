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
    using Cortex.Net.Blazor;
    using global::Fody;
    using Microsoft.AspNetCore.Components;
    using Mono.Cecil;
    using Mono.Cecil.Cil;

    /// <summary>
    /// Weaver for Blazor components decorated with the <see cref="ObserverAttribute" /> class.
    /// </summary>
    public class BlazorObserverWeaver
    {
        private const string ComponentBaseTypeName = "Microsoft.AspNetCore.Components.ComponentBase";

        private BaseModuleWeaver parentWeaver;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlazorObserverWeaver"/> class.
        /// </summary>
        /// <param name="parentWeaver">The parent weaver of this weaver.</param>
        public BlazorObserverWeaver(BaseModuleWeaver parentWeaver)
        {
            this.parentWeaver = parentWeaver ?? throw new ArgumentNullException(nameof(parentWeaver));
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
                                      t.BaseType != null
                                   /*                                    &&  t.CustomAttributes != null &&
                                                                         t.CustomAttributes.Any(x => x.AttributeType.FullName == typeof(ObserverAttribute).FullName)*/
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
        /// Weave the <see cref="ComponentBase" /> derived class.
        /// </summary>
        /// <param name="decoratedClass">The derived class to weave.</param>
        private void WeaveClass(TypeDefinition decoratedClass)
        {
            var decoratedType = decoratedClass.Resolve();
            var module = decoratedClass.Module;
            var buildRenderTreeMethod = decoratedType.Methods.Single(x => x.Name == "BuildRenderTree" && x.Parameters.Count == 1);
            var getTypeReference = module.ImportReference(typeof(object)).Resolve().Methods.Single(x => x.Name == "GetType" && x.Parameters.Count == 0 && !x.IsStatic);
            var getFullNameReference = module.ImportReference(typeof(Type)).Resolve().Methods.Single(x => x.Name == "get_FullName" && x.Parameters.Count == 0 && !x.IsStatic);
            var writeLineReference = module.ImportReference(typeof(Console)).Resolve().Methods.Single(x => x.Name == "WriteLine" && x.Parameters.Count == 1 && x.IsStatic && x.Parameters[0].ParameterType.FullName == typeof(string).FullName);

            var processor = buildRenderTreeMethod.Body.GetILProcessor();

            var originalStart = processor.Body.Instructions.First();

            var instructions = new List<Instruction>()
            {
                processor.Create(OpCodes.Ldarg_0),
                processor.Create(OpCodes.Call, module.ImportReference(getTypeReference)),
                processor.Create(OpCodes.Callvirt, module.ImportReference(getFullNameReference)),
                processor.Create(OpCodes.Call, module.ImportReference(writeLineReference)),
            };

            foreach (var instruction in instructions)
            {
                processor.InsertBefore(originalStart, instruction);
            }
        }
    }
}
