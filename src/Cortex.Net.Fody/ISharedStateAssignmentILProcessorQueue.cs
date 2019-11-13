// <copyright file="ISharedStateAssignmentILProcessorQueue.cs" company="Jan-Willem Spuij">
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
    using Mono.Cecil;
    using Mono.Cecil.Cil;

    /// <summary>
    /// Interface that holds a Queue with <see cref="ILProcessor"/> actions to be executed when <see cref="ISharedState"/> is assigned on an observable oject.
    /// </summary>
    public interface ISharedStateAssignmentILProcessorQueue
    {
        /// <summary>
        /// Gets a <see cref="Queue{T}"/> with actions to be executed to emit the IL code on <see cref="ISharedState"/> Assignment.
        /// </summary>
        Queue<(TypeDefinition, bool, Action<ILProcessor, FieldReference>)> SharedStateAssignmentQueue { get; }
    }
}
