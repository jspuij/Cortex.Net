// <copyright file="TraceExtensions.cs" company="Michel Weststrate, Jan-Willem Spuij">
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
    using System.Globalization;
    using System.Linq.Expressions;
    using System.Text;
    using Cortex.Net.Core;
    using Cortex.Net.Properties;
    using Cortex.Net.Types;

    /// <summary>
    /// Extension methods for tracing.
    /// </summary>
    public static class TraceExtensions
    {
        /// <summary>
        /// Traces the tracking derivation on the Shared State.
        /// </summary>
        /// <param name="sharedState">The Shared State to trace.</param>
        /// <param name="traceMode">The trace mode to use.</param>
        public static void Trace(this ISharedState sharedState, TraceMode traceMode = TraceMode.Log)
        {
            if (sharedState is null)
            {
                throw new ArgumentNullException(nameof(sharedState));
            }

            sharedState.TrackingDerivation?.Trace(traceMode);
        }

        /// <summary>
        /// Traces the tracking derivation on the Shared State.
        /// </summary>
        /// <param name="derivation">The derivation to trace.</param>
        /// <param name="traceMode">The trace mode to use.</param>
        public static void Trace(this IDerivation derivation, TraceMode traceMode = TraceMode.Log)
        {
            if (derivation is null)
            {
                throw new ArgumentNullException(nameof(derivation));
            }

            derivation.IsTracing = traceMode;
        }

        /// <summary>
        /// Traces the computed property given by the trace expression.
        /// </summary>
        /// <param name="toTrace">The object to trace.</param>
        /// <param name="expression">The member expression.</param>
        /// <param name="traceMode">The trace mode to use.</param>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <typeparam name="TMember">The property or method of the object.</typeparam>
        public static void Trace<TObject, TMember>(this TObject toTrace, Expression<Func<TObject, TMember>> expression, TraceMode traceMode = TraceMode.Log)
        {
            if (expression is null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            var observableObject = ObservableObject.GetFromObject(toTrace);

            if (observableObject == null)
            {
                throw new InvalidOperationException(Resources.CannotTraceNotObservable);
            }

            if (!(expression.Body is MemberExpression || expression.Body is MethodCallExpression))
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.ProvidedExpressionNoMemberExpression, expression));
            }

            string key = string.Empty;

            if (expression.Body is MemberExpression memberExpression)
            {
                key = memberExpression.Member.Name;
            }
            else if (expression.Body is MethodCallExpression methodCallExpression)
            {
                key = methodCallExpression.Method.Name;
            }

            var observableValue = observableObject[key];

            if (observableValue is IDerivation derivation)
            {
                derivation.Trace(traceMode);
                return;
            }

            throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.ProvidedMemberNoDerivation, key));
        }
    }
}
