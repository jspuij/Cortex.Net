// <copyright file="SharedStateObservableExtensions.cs" company="Michel Weststrate, Jan-Willem Spuij">
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

namespace Cortex.Net.Api
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Cortex.Net.Core;
    using Cortex.Net.Types;

    /// <summary>
    /// Extension methods for ISharedState to make attach an object to a shared state and make sure that
    /// Enhancer methods are properly followed.
    /// </summary>
    public static class SharedStateObservableExtensions
    {
        /// <summary>
        /// Makes sure that the observable is initialized against the shared state and any other objects along the tree.
        /// </summary>
        /// <typeparam name="T">The type to return.</typeparam>
        /// <param name="sharedState">The shared state to attach to.</param>
        /// <param name="initializerFunction">The initializer function.</param>
        /// <returns>The observable you wannt.</returns>
        public static T Observable<T>(this ISharedState sharedState, Func<T> initializerFunction)
        {
            if (sharedState is null)
            {
                throw new ArgumentNullException(nameof(sharedState));
            }

            if (initializerFunction is null)
            {
                throw new ArgumentNullException(nameof(initializerFunction));
            }

            try
            {
                SharedState.SetAsyncLocalState(sharedState);
                var result = initializerFunction();
                if (((IReactiveObject)result).SharedState != sharedState)
                {
                    // shared state should have been assigned.
                    throw new ArgumentOutOfRangeException(nameof(initializerFunction));
                }

                return result;
            }
            finally
            {
                SharedState.SetAsyncLocalState(null);
            }
        }

        /// <summary>
        /// Boxes the value T inside an <see cref="IObservableValue{T}" /> instance.
        /// </summary>
        /// <typeparam name="T">The type to box.</typeparam>
        /// <param name="sharedState">The shared state to operate on.</param>
        /// <param name="initialValue">The initial value to use.</param>
        /// <param name="name">The name of the observable value.</param>
        /// <param name="enhancer">The optional enhancer to use. default enhancer is the referenceEnhancer.</param>
        /// <returns>The observable.</returns>
        public static IObservableValue<T> Box<T>(this ISharedState sharedState, T initialValue, string name = null, IEnhancer enhancer = null)
        {
            if (sharedState is null)
            {
                throw new ArgumentNullException(nameof(sharedState));
            }

            if (enhancer is null)
            {
                enhancer = sharedState.ReferenceEnhancer();
            }

            if (string.IsNullOrEmpty(name))
            {
                name = $"{nameof(ObservableValue<T>)}@{sharedState.GetUniqueId()}";
            }

            return new ObservableValue<T>(sharedState, name, enhancer, initialValue);
        }

        /// <summary>
        /// Creates an observable Collection from an IEnumerable.
        /// </summary>
        /// <typeparam name="T">The type of the elements.</typeparam>
        /// <param name="sharedState">The shared state to operate on.</param>
        /// <param name="initialValues">The initial values to use.</param>
        /// <param name="name">The name of the observable collection.</param>
        /// <param name="enhancer">The optional enhancer to use. default enhancer is the deep enhancer.</param>
        /// <returns>The observable.</returns>
        public static ICollection<T> Collection<T>(this ISharedState sharedState, IEnumerable<T> initialValues = null, string name = null, IEnhancer enhancer = null)
        {
            if (sharedState is null)
            {
                throw new ArgumentNullException(nameof(sharedState));
            }

            if (enhancer is null)
            {
                enhancer = sharedState.DeepEnhancer();
            }

            if (string.IsNullOrEmpty(name))
            {
                name = $"{nameof(ObservableCollection<T>)}@{sharedState.GetUniqueId()}";
            }

            return new ObservableCollection<T>(sharedState, enhancer, initialValues, name);
        }

        /// <summary>
        /// Creates an observable Set from an initial Ienumberable.
        /// </summary>
        /// <typeparam name="T">The type of the elements.</typeparam>
        /// <param name="sharedState">The shared state to operate on.</param>
        /// <param name="initialValues">The initial values to use.</param>
        /// <param name="name">The name of the observable collection.</param>
        /// <param name="enhancer">The optional enhancer to use. default enhancer is the deep enhancer.</param>
        /// <returns>The observable.</returns>
        public static ISet<T> Set<T>(this ISharedState sharedState, IEnumerable<T> initialValues = null, string name = null, IEnhancer enhancer = null)
        {
            if (sharedState is null)
            {
                throw new ArgumentNullException(nameof(sharedState));
            }

            if (enhancer is null)
            {
                enhancer = sharedState.DeepEnhancer();
            }

            if (string.IsNullOrEmpty(name))
            {
                name = $"{nameof(ObservableSet<T>)}@{sharedState.GetUniqueId()}";
            }

            return new ObservableSet<T>(sharedState, enhancer, initialValues, name);
        }

        /// <summary>
        /// Creates an observable Dictionary from an initial dictionary.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="sharedState">The shared state to operate on.</param>
        /// <param name="initialValues">The initial values to use.</param>
        /// <param name="name">The name of the observable collection.</param>
        /// <param name="enhancer">The optional enhancer to use. default enhancer is the deep enhancer.</param>
        /// <returns>The observable.</returns>
        public static IDictionary<TKey, TValue> Dictionary<TKey, TValue>(this ISharedState sharedState, IDictionary<TKey, TValue> initialValues = null, string name = null, IEnhancer enhancer = null)
        {
            if (sharedState is null)
            {
                throw new ArgumentNullException(nameof(sharedState));
            }

            if (enhancer is null)
            {
                enhancer = sharedState.DeepEnhancer();
            }

            if (string.IsNullOrEmpty(name))
            {
                name = $"{nameof(ObservableDictionary<TKey, TValue>)}@{sharedState.GetUniqueId()}";
            }

            return new ObservableDictionary<TKey, TValue>(sharedState, enhancer, initialValues, name);
        }

        /// <summary>
        /// Creates a computed value.
        /// </summary>
        /// <typeparam name="T">The type of the elements.</typeparam>
        /// <param name="sharedState">The shared state to operate on.</param>
        /// <param name="getter">The getter function to use.</param>
        /// <param name="name">The name of the observable collection.</param>
        /// <returns>The computed value.</returns>
        public static IComputedValue<T> Computed<T>(this ISharedState sharedState, Func<T> getter, string name = null)
        {
            if (sharedState is null)
            {
                throw new ArgumentNullException(nameof(sharedState));
            }

            if (string.IsNullOrEmpty(name))
            {
                name = $"{nameof(ComputedValue<T>)}@{sharedState.GetUniqueId()}";
            }

            return new ComputedValue<T>(sharedState, new ComputedValueOptions<T>(getter, name));
        }
    }
}
