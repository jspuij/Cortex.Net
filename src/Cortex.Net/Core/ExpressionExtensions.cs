// <copyright file="ExpressionExtensions.cs" company="Michel Weststrate, Jan-Willem Spuij">
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

namespace Cortex.Net.Core
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq.Expressions;
    using System.Text;
    using Cortex.Net.Properties;

    /// <summary>
    /// Extension methods for a range of different Expressions.
    /// </summary>
    public static class ExpressionExtensions
    {
        /// <summary>
        /// Extracts the property or method name from a Property or member access expression.
        /// </summary>
        /// <typeparam name="TObject">The type of the object to get the members from.</typeparam>
        /// <typeparam name="TMember">The member of the object.</typeparam>
        /// <param name="expression">The expression.</param>
        /// <returns>The name of the method or property.</returns>
        /// <exception cref="ArgumentNullException">When expression is null.</exception>
        public static string ExtractNameFromMemberExpression<TObject, TMember>(this Expression<Func<TObject, TMember>> expression)
        {
            if (expression is null)
            {
                throw new ArgumentNullException(nameof(expression));
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

            return key;
        }
    }
}
