// <copyright file="ActionRunInfo.cs" company="Michel Weststrate, Jan-Willem Spuij">
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

    /// <summary>
    /// Data class with run info about an action.
    /// </summary>
    internal class ActionRunInfo
    {
        /// <summary>
        /// Gets or sets the shared state that this <see cref="ActionRunInfo"/> references.
        /// </summary>
        internal ISharedState SharedState { get; set; }

        /// <summary>
        /// Gets or sets previous derivation.
        /// </summary>
        internal IDerivation PreviousDerivation { get; set; }

        /// <summary>
        /// Gets or sets the Action Name.
        /// </summary>
        internal string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether previous value of <see cref="ISharedState.AllowStateChanges"/>.
        /// </summary>
        internal bool PreviousAllowStateChanges { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether previous value of <see cref="ISharedState.AllowStateReads"/>.
        /// </summary>
        internal bool PreviousAllowStateReads { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to notify spy.
        /// </summary>
        internal bool NotifySpy { get; set; }

        /// <summary>
        /// Gets or sets start date / time of the action.
        /// </summary>
        internal DateTime StartDateTime { get; set; }

        /// <summary>
        /// Gets or sets exception received from the action.
        /// </summary>
        internal Exception Exception { get; set; }

        /// <summary>
        /// Gets or sets iD of the parent action.
        /// </summary>
        internal int ParentActionId { get; set; }

        /// <summary>
        /// Gets or sets iD of the action.
        /// </summary>
        internal int ActionId { get; set; }
    }
}
