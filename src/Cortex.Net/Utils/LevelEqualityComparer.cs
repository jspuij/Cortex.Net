// <copyright file="LevelEqualityComparer.cs" company="Michel Weststrate, Jan-Willem Spuij">
// Copyright 2019 Michel Weststrate, Jan-Willem Spuij
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

namespace Cortex.Net.Utils
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Compares two types based on their member values up to a certain depth.
    /// </summary>
    /// <typeparam name="T">The types to compare.</typeparam>
    public class LevelEqualityComparer<T> :
        IEqualityComparer<T>, IEqualityComparer
    {
        /// <summary>
        /// The depth to compare to.
        /// </summary>
        private readonly int depth;

        /// <summary>
        /// A dictionary of already completed Comparisons.
        /// </summary>
        private readonly IDictionary<object, object> visitedComparisons = new Dictionary<object, object>();

        /// <summary>
        /// Initializes a new instance of the <see cref="LevelEqualityComparer{T}"/> class.
        /// </summary>
        /// <param name="depth">The depth up to where needs to be checked.</param>
        public LevelEqualityComparer(int depth)
            : this(depth, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LevelEqualityComparer{T}"/> class.
        /// </summary>
        /// <param name="depth">The depth up to where needs to be checked.</param>
        /// <param name="visitedComparisons">A dictionary of already visited comparisons.</param>
        private LevelEqualityComparer(int depth, IDictionary<object, object> visitedComparisons)
        {
            this.depth = depth;
            this.visitedComparisons = visitedComparisons;
        }

        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        /// <param name="x">The first object of type T to compare.</param>
        /// <param name="y">The second object of type T to compare.</param>
        /// <returns>true if the specified objects are equal; otherwise, false.</returns>
        public bool Equals(T x, T y)
        {
            return ((IEqualityComparer)this).Equals(x, y);
        }

        /// <summary>
        /// Returns a hash code for the specified object.
        /// </summary>
        /// <param name="obj">The instance of type T for which a hash code is to be returned.</param>
        /// <returns>A hash code for the specified object.</returns>
        public int GetHashCode(T obj)
        {
            return ((IEqualityComparer)this).GetHashCode(obj);
        }

        /// <summary>
        ///  Determines whether the specified objects are equal.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns> true if the specified objects are equal; otherwise, false.</returns>
        bool IEqualityComparer.Equals(object x, object y)
        {
            int level = this.depth;
            return InternalEquals(this.visitedComparisons ?? new Dictionary<object, object>(), x, y, level);
        }

        /// <summary>
        /// Returns a hash code for the specified object.
        /// </summary>
        /// <param name="obj">The System..Object for which a hash code is to be returned.</param>
        /// <returns>A hash code for the specified object.</returns>
        int IEqualityComparer.GetHashCode(object obj)
        {
            throw new System.NotImplementedException();
        }

        private static bool InternalEquals(IDictionary<object, object> visitedComparisons, object x, object y, int level)
        {
            if (x is null && y is null)
            {
                return true;
            }

            if ((x is null && y != null) || (y is null && x != null))
            {
                return false;
            }

            var type = x.GetType();

            if (type != y.GetType())
            {
                return false;
            }

            if (type.IsPrimitive || typeof(string).Equals(type) || typeof(IEquatable<>).MakeGenericType(type).IsAssignableFrom(type))
            {
                return Equals(x, y);
            }

            if (level == 0)
            {
                return false;
            }
            else if (level < 0)
            {
                level = -1;
            }

            if (type.IsClass)
            {
                // handle circular references.
                if (visitedComparisons.TryGetValue(x, out object z))
                {
                    if (ReferenceEquals(y, z))
                    {
                        return true;
                    }
                }

                visitedComparisons.Add(x, y);
            }

            if (typeof(IEnumerable<object>).IsAssignableFrom(type))
            {
                var result = Enumerable.SequenceEqual<object>((IEnumerable<object>)x, (IEnumerable<object>)y, new LevelEqualityComparer<object>(level - 1, visitedComparisons));
                if (!result)
                {
                    return false;
                }
            }

            foreach (var member in type.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
            {
                var result = InternalEquals(visitedComparisons, member.GetValue(x), member.GetValue(y), level - 1);
                if (!result)
                {
                    return false;
                }
            }

            foreach (var member in type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
            {
                var result = InternalEquals(visitedComparisons, member.GetValue(x), member.GetValue(y), level - 1);
                if (!result)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
