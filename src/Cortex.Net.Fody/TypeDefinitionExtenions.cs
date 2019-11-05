// <copyright file="TypeDefinitionExtenions.cs" company="Jan-Willem Spuij">
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
    using System.Linq;
    using Mono.Cecil;
    using Mono.Cecil.Cil;

    /// <summary>
    /// Extension methods for <see cref="TypeDefinition"/> instances.
    /// </summary>
    public static class TypeDefinitionExtenions
    {
        /// <summary>
        /// Creates a backing field for a property.
        /// </summary>
        /// <param name="classType">The type reference of the class.</param>
        /// <param name="fieldType">The type reference of the Field.</param>
        /// <param name="name">The name of the Property.</param>
        /// <returns>The Field definition. It has already been added to the class type.</returns>
        public static FieldDefinition CreateBackingField(this TypeDefinition classType, TypeReference fieldType, string name)
        {
            return CreateField(classType, fieldType, $"<{name}>k__BackingField");
        }

        /// <summary>
        /// Creates a field.
        /// </summary>
        /// <param name="classType">The type reference of the class.</param>
        /// <param name="fieldType">The type reference of the Field.</param>
        /// <param name="name">The name of the field.</param>
        /// <param name="fieldAttributes">The field Atrributes.</param>
        /// <returns>The Field definition. It has already been added to the class type.</returns>
        public static FieldDefinition CreateField(this TypeDefinition classType, TypeReference fieldType, string name, FieldAttributes fieldAttributes = FieldAttributes.Private)
        {
            if (classType is null)
            {
                throw new ArgumentNullException(nameof(classType));
            }

            if (fieldType is null)
            {
                throw new ArgumentNullException(nameof(fieldType));
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            var field = new FieldDefinition(name, fieldAttributes, fieldType);

            // Add compiler generated attribute.
            var compilerGeneratedAttributeType = classType.Module.ImportReference(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute));
            var ctor = compilerGeneratedAttributeType.Resolve().Methods.Single(x => x.IsConstructor);
            var ctorRef = classType.Module.ImportReference(ctor);
            var compilerGeneratedAttribute = new CustomAttribute(ctorRef, new byte[] { 01, 00, 00, 00 });
            field.CustomAttributes.Add(compilerGeneratedAttribute);

            // Add Debugger broswable attribute.
            var debuggerBrowsableAttributeType = classType.Module.ImportReference(typeof(System.Diagnostics.DebuggerBrowsableAttribute));
            ctor = debuggerBrowsableAttributeType.Resolve().Methods.Single(x => x.IsConstructor);
            ctorRef = classType.Module.ImportReference(ctor);
            var debuggerBrowsableAttribute = new CustomAttribute(ctorRef, new byte[] { 01, 00, 00, 00, 00, 00, 00, 00 });
            field.CustomAttributes.Add(debuggerBrowsableAttribute);

            classType.Fields.Add(field);
            return field;
        }

        /// <summary>
        /// Creates a default getter for a property.
        /// </summary>
        /// <param name="classType">The type reference of the class.</param>
        /// <param name="backingField">A reference to the backing field.</param>
        /// <param name="name">The name of the property.</param>
        /// <param name="methodAttributes">The methodAttributes for this getter. Default value is MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName.</param>
        /// <returns>A method definition. The method is already added to the <paramref name="classType"/>.</returns>
        public static MethodDefinition CreateDefaultGetter(this TypeDefinition classType, FieldDefinition backingField, string name, MethodAttributes methodAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName)
        {
            if (classType is null)
            {
                throw new ArgumentNullException(nameof(classType));
            }

            if (backingField is null)
            {
                throw new ArgumentNullException(nameof(backingField));
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            var moduleDefinition = classType.Module;

            var splittedName = name.Split('.');

            if (splittedName.Length > 1)
            {
                name = string.Join(".", splittedName.Take(splittedName.Length - 1));
                name += $".get_{splittedName.Last()}";
            }

            var method = new MethodDefinition(name, methodAttributes, backingField.FieldType);

            // Add compiler generated attribute.
            var compilerGeneratedAttributeType = classType.Module.ImportReference(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute));
            var ctor = compilerGeneratedAttributeType.Resolve().Methods.Single(x => x.IsConstructor);
            var ctorRef = classType.Module.ImportReference(ctor);
            var compilerGeneratedAttribute = new CustomAttribute(ctorRef, new byte[] { 01, 00, 00, 00 });
            method.CustomAttributes.Add(compilerGeneratedAttribute);

            var backingFieldReference = moduleDefinition.ImportReference(backingField);

            // generate method body.
            var processor = method.Body.GetILProcessor();
            processor.Emit(OpCodes.Ldarg_0);
            processor.Emit(OpCodes.Ldfld, backingFieldReference);
            processor.Emit(OpCodes.Ret);

            // add method to class type.
            classType.Methods.Add(method);

            return method;
        }

        /// <summary>
        /// Creates a default getter for a property.
        /// </summary>
        /// <param name="classType">The type reference of the class.</param>
        /// <param name="backingField">A reference to the backing field.</param>
        /// <param name="name">The name of the property.</param>
        /// <param name="methodAttributes">The methodAttributes for this getter. Default value is MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName.</param>
        /// <param name="emitAction">Extra emit action after default setter.</param>
        /// <returns>A method definition. The method is already added to the <paramref name="classType"/>.</returns>
        public static MethodDefinition CreateDefaultSetter(
            this TypeDefinition classType,
            FieldDefinition backingField,
            string name,
            MethodAttributes methodAttributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName,
            Action<ILProcessor> emitAction = null)
        {
            if (classType is null)
            {
                throw new ArgumentNullException(nameof(classType));
            }

            if (backingField is null)
            {
                throw new ArgumentNullException(nameof(backingField));
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            var moduleDefinition = classType.Module;

            var voidType = moduleDefinition.ImportReference(typeof(void));

            var splittedName = name.Split('.');

            if (splittedName.Length > 1)
            {
                name = string.Join(".", splittedName.Take(splittedName.Length - 1));
                name += $".set_{splittedName.Last()}";
            }

            var method = new MethodDefinition(name, methodAttributes, voidType);
            method.Parameters.Add(new ParameterDefinition("value", ParameterAttributes.None, backingField.FieldType));

            // Add compiler generated attribute.
            var compilerGeneratedAttributeType = classType.Module.ImportReference(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute));
            var ctor = compilerGeneratedAttributeType.Resolve().Methods.Single(x => x.IsConstructor);
            var ctorRef = classType.Module.ImportReference(ctor);
            var compilerGeneratedAttribute = new CustomAttribute(ctorRef, new byte[] { 01, 00, 00, 00 });
            method.CustomAttributes.Add(compilerGeneratedAttribute);

            var backingFieldReference = moduleDefinition.ImportReference(backingField);

            // generate method body.
            var processor = method.Body.GetILProcessor();
            processor.Emit(OpCodes.Ldarg_0);
            processor.Emit(OpCodes.Ldarg_1);
            processor.Emit(OpCodes.Stfld, backingFieldReference);

            // execute processor action before return.
            emitAction?.Invoke(processor);

            processor.Emit(OpCodes.Ret);

            // add method to class type.
            classType.Methods.Add(method);

            return method;
        }

        /// <summary>
        /// Creates a property using the specified getter / setter.
        /// </summary>
        /// <param name="classType">The type of the class to create the property on.</param>
        /// <param name="name">The name of the property.</param>
        /// <param name="getter">The getter for the property.</param>
        /// <param name="setter">The setter for the property. Can be null.</param>
        /// <returns>The created property definitition.</returns>
        public static PropertyDefinition CreateProperty(this TypeDefinition classType, string name, MethodDefinition getter, MethodDefinition setter = null)
        {
            if (classType is null)
            {
                throw new ArgumentNullException(nameof(classType));
            }

            if (getter is null)
            {
                throw new ArgumentNullException(nameof(getter));
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            // create the property.
            var property = new PropertyDefinition(name, PropertyAttributes.None, getter.ReturnType)
            {
                GetMethod = getter,
            };

            if (setter != null)
            {
                property.SetMethod = setter;
            }

            // add to classtype.
            classType.Properties.Add(property);
            return property;
        }
    }
}
