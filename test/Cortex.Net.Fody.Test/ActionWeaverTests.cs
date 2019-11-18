// <copyright file="ActionWeaverTests.cs" company="Jan-Willem Spuij">
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

namespace Cortex.Net.Fody.Test
{
    using System;
    using System.Collections.Generic;
    using Cortex.Net.Api;
    using global::Fody;
    using Mono.Cecil;
    using Moq;
    using Xunit;

    /// <summary>
    /// Unit tests for the <see cref="ActionWeaver"/> class.
    /// </summary>
    public class ActionWeaverTests
    {
        /// <summary>
        /// Tests execution of the <see cref="ActionWeaver"/> class.
        /// </summary>
        [Fact]
        public void ExecuteTests()
        {
            var sharedState = new SharedState();

            var testClass = new ActionTestClass();
            Assert.True(testClass is IReactiveObject);
            string expected = "Lisa";
            testClass.SetName(expected);
            Assert.Equal(expected, testClass.Name);
            testClass.SetNameToJohn();
            Assert.Equal("John", testClass.Name);
        }

        /// <summary>
        /// Tests that executing operations on an object that is not attached to shared state does not throw exceptions.
        /// </summary>
        [Fact]
        public void NoSharedStateDoesNotThrow()
        {
            var testClass = new ActionTestClass();
            Assert.True(testClass is IReactiveObject);
            string expected = "Lisa";
            testClass.SetName(expected);
            Assert.Equal(expected, testClass.Name);
            testClass.SetNameToJohn();
            Assert.Equal("John", testClass.Name);
        }

        private class ActionTestClass
        {
            public string Name { get; private set; }

            [Action]
            public void SetName(string newName)
            {
                this.Name = newName;
            }

            [Action]
            public void SetNameToJohn()
            {
                this.Name = "John";
            }
        }
    }
}
