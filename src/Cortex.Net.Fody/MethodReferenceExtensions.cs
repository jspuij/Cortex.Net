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

                var newParameter = new ParameterDefinition(parameter.Name, parameter.Attributes, pt);
                newParameter.IsIn = parameter.IsIn;
                newParameter.IsLcid = parameter.IsLcid;
                newParameter.IsOptional = parameter.IsOptional;
                newParameter.IsOut = parameter.IsOut;
                newParameter.IsReturnValue = parameter.IsReturnValue;
                result.Parameters.Add(newParameter);
            }

            return result;
        }

        /// <summary>
        /// Gets the Action type for the private field that is added to the class for the private method.
        /// </summary>
        /// <param name="methodDefinition">The method definition for the action.</param>
        /// <returns>A type reference.</returns>
        public static TypeReference GetActionType(this MethodDefinition methodDefinition)
        {
            if (methodDefinition is null)
            {
                throw new ArgumentNullException(nameof(methodDefinition));
            }

            var moduleDefinition = methodDefinition.Module;

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

        /// <summary>
        /// Gets the Function type for the computed method that is passed.
        /// </summary>
        /// <param name="methodDefinition">The method definition for the function.</param>
        /// <returns>A type reference.</returns>
        public static TypeReference GetFunctionType(this MethodDefinition methodDefinition)
        {
            if (methodDefinition is null)
            {
                throw new ArgumentNullException(nameof(methodDefinition));
            }

            var moduleDefinition = methodDefinition.Module;

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
