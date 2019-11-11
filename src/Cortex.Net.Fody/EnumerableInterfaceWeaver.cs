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
    using System.Text;
    using global::Fody;
    using Mono.Cecil;

    /// <summary>
    /// Weaver for enumerable types.
    /// </summary>
    public class EnumerableInterfaceWeaver
    {
        private static readonly string IGenericListName = typeof(IList<>).FullName;
        private static readonly string IGenericReadonlyListName = typeof(IReadOnlyList<>).FullName;
        private static readonly string IGenericSetName = typeof(ISet<>).FullName;
        private static readonly string IGenericDictionaryName = typeof(IDictionary<,>).FullName;
        private static readonly string IGenericReadonlyDictionaryName = typeof(IReadOnlyDictionary<,>).FullName;
        private static readonly string IGenericCollectionName = typeof(ICollection<>).FullName;
        private static readonly string IGenericReadonlyCollectionName = typeof(IReadOnlyCollection<>).FullName;

        /// <summary>
        /// The parent weaver of this weaver.
        /// </summary>
        private readonly BaseModuleWeaver parentWeaver;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumerableInterfaceWeaver"/> class.
        /// </summary>
        /// <param name="parentWeaver">The parent weaver of this CollectionWeaver.</param>
        public EnumerableInterfaceWeaver(BaseModuleWeaver parentWeaver)
        {
            this.parentWeaver = parentWeaver ?? throw new ArgumentNullException(nameof(parentWeaver));
        }

        /// <summary>
        /// Weaves an enumerable property.
        /// </summary>
        /// <param name="property">The property to weave.</param>
        public void WeaveEnumerableProperty(PropertyDefinition property)
        {

        }
    }
}
