// <copyright file="TypeResolverDictionary.cs" company="Jan-Willem Spuij">
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
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Mono.Cecil;

    /// <summary>
    /// Type resolver class. Resolves types from referenced assemblies.
    /// </summary>
    public sealed class TypeResolverDictionary : IDictionary<string, TypeReference>
    {
        private readonly Dictionary<string, TypeReference> innerResolvedTypes;

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeResolverDictionary"/> class.
        /// </summary>
        /// <param name="moduleDefinition">The module definition to use.</param>
        /// <param name="typeAssemblyPairs">The type / assembly pairs to use.</param>
        public TypeResolverDictionary(ModuleDefinition moduleDefinition, Dictionary<string, string> typeAssemblyPairs)
        {
            if (moduleDefinition is null)
            {
                throw new ArgumentNullException(nameof(moduleDefinition));
            }

            if (typeAssemblyPairs is null)
            {
                throw new ArgumentNullException(nameof(typeAssemblyPairs));
            }

            this.innerResolvedTypes = (from pair in typeAssemblyPairs
                                       group pair by pair.Value into g
                                       let asd = moduleDefinition.AssemblyResolver.Resolve(moduleDefinition.AssemblyReferences.FirstOrDefault(asm => asm.Name == g.Key))
                                       from item in g
                                       select new
                                       {
                                           TypeName = item.Key,
                                           TypeReference = asd.MainModule.GetType(item.Key),
                                       }).ToDictionary(x => x.TypeName, x => moduleDefinition.ImportReference(x.TypeReference));
        }

        /// <inheritdoc />
        public ICollection<string> Keys => ((IDictionary<string, TypeReference>)this.innerResolvedTypes).Keys;

        /// <inheritdoc />
        public ICollection<TypeReference> Values => ((IDictionary<string, TypeReference>)this.innerResolvedTypes).Values;

        /// <inheritdoc />
        public int Count => this.innerResolvedTypes.Count;

        /// <inheritdoc />
        public bool IsReadOnly => ((IDictionary<string, TypeReference>)this.innerResolvedTypes).IsReadOnly;

        /// <inheritdoc />
        public TypeReference this[string key] { get => this.innerResolvedTypes[key]; set => this.innerResolvedTypes[key] = value; }

        /// <inheritdoc />
        public void Add(string key, TypeReference value)
        {
            this.innerResolvedTypes.Add(key, value);
        }

        /// <inheritdoc />
        public void Add(KeyValuePair<string, TypeReference> item)
        {
            ((IDictionary<string, TypeReference>)this.innerResolvedTypes).Add(item);
        }

        /// <inheritdoc />
        public void Clear()
        {
            this.innerResolvedTypes.Clear();
        }

        /// <inheritdoc />
        public bool Contains(KeyValuePair<string, TypeReference> item)
        {
            return ((IDictionary<string, TypeReference>)this.innerResolvedTypes).Contains(item);
        }

        /// <inheritdoc />
        public bool ContainsKey(string key)
        {
            return this.innerResolvedTypes.ContainsKey(key);
        }

        /// <inheritdoc />
        public void CopyTo(KeyValuePair<string, TypeReference>[] array, int arrayIndex)
        {
            ((IDictionary<string, TypeReference>)this.innerResolvedTypes).CopyTo(array, arrayIndex);
        }

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, TypeReference>> GetEnumerator()
        {
            return ((IDictionary<string, TypeReference>)this.innerResolvedTypes).GetEnumerator();
        }

        /// <inheritdoc />
        public bool Remove(string key)
        {
            return this.innerResolvedTypes.Remove(key);
        }

        /// <inheritdoc />
        public bool Remove(KeyValuePair<string, TypeReference> item)
        {
            return ((IDictionary<string, TypeReference>)this.innerResolvedTypes).Remove(item);
        }

        /// <inheritdoc />
        public bool TryGetValue(string key, out TypeReference value)
        {
            return this.innerResolvedTypes.TryGetValue(key, out value);
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IDictionary<string, TypeReference>)this.innerResolvedTypes).GetEnumerator();
        }
    }
}
