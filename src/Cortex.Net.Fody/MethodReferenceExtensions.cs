// <copyright file="MethodReferenceExtensions.cs" company="Jan-Willem Spuij">
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
    using Cortex.Net.Fody.Properties;
    using Mono.Cecil;

    /// <summary>
    /// Extension methods for <see cref="MethodReference"/> instances.
    /// </summary>
    public static class MethodReferenceExtensions
    {
        /// <summary>
        /// Gets a method reference to the generic method on the instantiated type specified.
        /// </summary>
        /// <param name="genericMethod">A reference to the generic method.</param>
        /// <param name="instantiantedType">The instantiated type.</param>
        /// <returns>A new method reference.</returns>
        public static MethodReference GetGenericMethodOnInstantance(this MethodReference genericMethod, TypeReference instantiantedType)
        {
            if (genericMethod is null)
            {
                throw new ArgumentNullException(nameof(genericMethod));
            }

            if (instantiantedType is null)
            {
                throw new ArgumentNullException(nameof(instantiantedType));
            }

            var parameters = genericMethod.Parameters;
            var module = instantiantedType.Module;
            var hasThis = genericMethod.HasThis;
            var explicitThis = genericMethod.ExplicitThis;

            var result = new MethodReference(genericMethod.Name, genericMethod.ReturnType, instantiantedType)
            {
                HasThis = hasThis,
                ExplicitThis = explicitThis,
            };

            foreach (var parameter in parameters)
            {
                var pt = parameter.ParameterType;
                if (!pt.ContainsGenericParameter)
                {
                    pt = module.ImportReference(parameter.ParameterType);
                }

                var newParameter = new ParameterDefinition(parameter.Name, parameter.Attributes, pt)
                {
                    IsIn = parameter.IsIn,
                    IsLcid = parameter.IsLcid,
                    IsOptional = parameter.IsOptional,
                    IsOut = parameter.IsOut,
                    IsReturnValue = parameter.IsReturnValue,
                };

                result.Parameters.Add(newParameter);
            }

            return result;
        }

        /// <summary>
        /// Gets the Action type for the private field that is added to the class for the private method.
        /// </summary>
        /// <param name="methodDefinition">The method definition for the action.</param>
        /// <param name="weavingContext">The weaving context.</param>
        /// <returns>A type reference.</returns>
        public static TypeReference GetActionType(this MethodDefinition methodDefinition, WeavingContext weavingContext)
        {
            if (methodDefinition is null)
            {
                throw new ArgumentNullException(nameof(methodDefinition));
            }

            if (weavingContext is null)
            {
                throw new ArgumentNullException(nameof(weavingContext));
            }

            var moduleDefinition = methodDefinition.Module;

            if (methodDefinition.Parameters == null || !methodDefinition.Parameters.Any())
            {
                return moduleDefinition.ImportReference(weavingContext.SystemAction[0]);
            }

            TypeReference genericActionType;

            switch (methodDefinition.Parameters.Count)
            {
                case int i when i > 0 && i <= 16:
                    genericActionType = moduleDefinition.ImportReference(weavingContext.SystemAction[i]);
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

        /// <summary>
        /// Gets the Function type for the computed method that is passed.
        /// </summary>
        /// <param name="methodDefinition">The method definition for the function.</param>
        /// <param name="weavingContext">The weaving context.</param>
        /// <returns>A type reference.</returns>
        public static TypeReference GetFunctionType(this MethodDefinition methodDefinition, WeavingContext weavingContext)
        {
            if (methodDefinition is null)
            {
                throw new ArgumentNullException(nameof(methodDefinition));
            }

            if (weavingContext is null)
            {
                throw new ArgumentNullException(nameof(weavingContext));
            }

            var moduleDefinition = methodDefinition.Module;

            TypeReference genericFunctionType;

            switch (methodDefinition.Parameters.Count)
            {
                case int i when i >= 0 && i <= 16:
                    genericFunctionType = moduleDefinition.ImportReference(weavingContext.SystemFunc[i]);
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
