// <copyright file="ReactiveObjectTypeUtilExtensions.cs" company="Michel Weststrate, Jan-Willem Spuij">
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
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Text;
    using Cortex.Net.Core;
    using Cortex.Net.Types;

    /// <summary>
    /// Type utility extensions on the IReactiveObject interface.
    /// </summary>
    public static class ReactiveObjectTypeUtilExtensions
    {
        /// <summary>
        /// Gets the underlying observable from the IReactiveObject instance.
        /// </summary>
        /// <param name="reactiveObject">The reactive object to get the atom for.</param>
        /// <returns>The IAtom instance to get.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when there is no observable object internally.</exception>
        public static IObservable GetObservable(this IReactiveObject reactiveObject)
        {
            return GetObservable((object)reactiveObject);
        }

        /// <summary>
        /// Gets the underlying atom from the IReactiveObject instance.
        /// </summary>
        /// <param name="reactiveObject">The reactive object to get the atom for.</param>
        /// <returns>The IAtom instance to get.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when there is no atom provider internally.</exception>
        public static IObservable GetObservable(this object reactiveObject)
        {
            if (reactiveObject is null)
            {
                throw new ArgumentNullException(nameof(reactiveObject));
            }

            var observableObject = ObservableObject.GetFromObject(reactiveObject);

            if (observableObject != null)
            {
                return ((IAtomProvider)observableObject).Atom;
            }

            if (observableObject is IAtomProvider atomProvider)
            {
                return atomProvider.Atom;
            }

            throw new ArgumentOutOfRangeException(nameof(reactiveObject));
        }

        /// <summary>
        /// Gets the observable for a member of an object.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <typeparam name="TMember">The type of the member.</typeparam>
        /// <param name="target">The target object to get a property from.</param>
        /// <param name="expression">The member expression.</param>
        /// <returns>An atom for the member of an object.</returns>
        public static IObservable GetObservable<TObject, TMember>(this TObject target, Expression<Func<TObject, TMember>> expression)
            where TObject : class
        {
            if (target is null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (expression is null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            var observableObject = ObservableObject.GetFromObject(target);

            if (observableObject != null)
            {
                var name = expression.ExtractNameFromMemberExpression();

                if (!string.IsNullOrEmpty(name))
                {
                    if (observableObject.Has(name))
                    {
                        var value = observableObject[name];

                        return (IObservable)value;
                    }
                }
            }

            var ex = expression.Compile();
            var result = ex(target);

            if (result is IAtomProvider atomProvider)
            {
                return atomProvider.Atom;
            }

            throw new ArgumentOutOfRangeException(nameof(expression));
        }
    }
}
