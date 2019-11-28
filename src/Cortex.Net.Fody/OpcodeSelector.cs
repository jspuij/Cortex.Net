// <copyright file="OpcodeSelector.cs" company="Jan-Willem Spuij">
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
    using Mono.Cecil.Cil;

    /// <summary>
    /// Opcode selector for Short Forms.
    /// </summary>
    public static class OpcodeSelector
    {
        /// <summary>
        /// Gets the best short form opcode for Ldarg.
        /// </summary>
        /// <param name="processor">The Il Processor to generate the instruction on.</param>
        /// <param name="index">The index.</param>
        /// <returns>An instruction.</returns>
        public static Instruction Ldarg(this ILProcessor processor, int index)
        {
            if (processor is null)
            {
                throw new ArgumentNullException(nameof(processor));
            }

            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return processor.GetBestShortForm(
                OpCodes.Ldarg_0,
                OpCodes.Ldarg_1,
                OpCodes.Ldarg_2,
                OpCodes.Ldarg_3,
                OpCodes.Ldarg_S,
                OpCodes.Ldarg,
                index);
        }

        /// <summary>
        /// Gets the best short form opcode for Ldloc.
        /// </summary>
        /// <param name="processor">The Il Processor to generate the instruction on.</param>
        /// <param name="index">The index.</param>
        /// <returns>An instruction.</returns>
        public static Instruction Ldloc(this ILProcessor processor, int index)
        {
            if (processor is null)
            {
                throw new ArgumentNullException(nameof(processor));
            }

            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return processor.GetBestShortForm(
                OpCodes.Ldloc_0,
                OpCodes.Ldloc_1,
                OpCodes.Ldloc_2,
                OpCodes.Ldloc_3,
                OpCodes.Ldloc_S,
                OpCodes.Ldloc,
                index);
        }

        /// <summary>
        /// Gets the best short form for an Instruction.
        /// </summary>
        /// <param name="processor">The IL Processor to create the instruction on.</param>
        /// <param name="opCode0">The shortform opcode for 0.</param>
        /// <param name="opCode1">The shortform opcode for 1.</param>
        /// <param name="opCode2">The shortform opcode for 2.</param>
        /// <param name="opCode3">The shortform opcode for 3.</param>
        /// <param name="opCodeS">The shortform opcode for 4-255.</param>
        /// <param name="opCode">The opcode for everything else..</param>
        /// <param name="index">The index for the instruction.</param>
        /// <returns>An instruction.</returns>
        private static Instruction GetBestShortForm(this ILProcessor processor, OpCode opCode0, OpCode opCode1, OpCode opCode2, OpCode opCode3, OpCode opCodeS, OpCode opCode,  int index)
        {
            switch (index)
            {
                case 0:
                    return processor.Create(opCode0);
                case 1:
                    return processor.Create(opCode1);
                case 2:
                    return processor.Create(opCode2);
                case 3:
                    return processor.Create(opCode3);
                case int n when n > 3 && n <= 255:
                    return processor.Create(opCodeS, Convert.ToByte(index));
                default:
                    return processor.Create(opCode, index);
            }
        }
    }
}
