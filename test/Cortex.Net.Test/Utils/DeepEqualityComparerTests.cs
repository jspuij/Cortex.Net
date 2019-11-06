// <copyright file="DeepEqualityComparerTests.cs" company="Michel Weststrate, Jan-Willem Spuij">
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

namespace Cortex.Net.Test.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Cortex.Net.Utils;
    using Xunit;

    /// <summary>
    /// Unit tests for the <see cref="DeepEqualityComparer{T}"/> class.
    /// </summary>
    public class DeepEqualityComparerTests
    {
        /// <summary>
        /// Tests value types for equality and inequality.
        /// </summary>
        [Fact]
        public void ValueTypeTests()
        {
            var intComparer = new DeepEqualityComparer<int>();

            Assert.True(intComparer.Equals(3, 3));
            Assert.False(intComparer.Equals(3, 2));
        }

        /// <summary>
        /// Tests string types for equality and inequality.
        /// </summary>
        [Fact]
        public void StringTypeTests()
        {
            var stringComparer = new DeepEqualityComparer<string>();

            Assert.True(stringComparer.Equals("Test", "Test"));
            Assert.False(stringComparer.Equals("Test", "Wrong"));

            var stringBuilder = new StringBuilder();
            stringBuilder.Append("Te");
            stringBuilder.Append("st");

            Assert.True(stringComparer.Equals("Test", stringBuilder.ToString()));
        }

        /// <summary>
        /// Tests struct types.
        /// </summary>
        [Fact]
        public void StructTypeTests()
        {
            var dateComparer = new DeepEqualityComparer<DateTime>();
            Assert.True(dateComparer.Equals(new DateTime(1900, 1, 1), new DateTime(1900, 1, 1)));
            Assert.False(dateComparer.Equals(new DateTime(2020, 1, 1), new DateTime(1900, 1, 1)));

            var intComparer = new DeepEqualityComparer<int?>();

            Assert.True(intComparer.Equals(3, 3));
            Assert.True(intComparer.Equals(null, null));
            Assert.False(intComparer.Equals(3, 2));
            Assert.False(intComparer.Equals(3, null));

            var structComparer = new DeepEqualityComparer<Test>();

            Assert.True(structComparer.Equals(new Test { }, new Test { }));
            Assert.True(structComparer.Equals(new Test { X = "Test", Y = 1 }, new Test { X = "Test", Y = 1 }));
            Assert.False(structComparer.Equals(new Test { X = "False", Y = 1 }, new Test { X = "Test", Y = 0 }));
        }

        private struct Test
        {
            public string X;
            public int Y;
        }
    }
}
