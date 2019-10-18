// <copyright file="IDerivationState.cs" company="Michel Weststrate, Jan-Willem Spuij">
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

namespace Cortex.Net
{
    /// <summary>
    /// The state of the <see cref="IDerivation"/> instance.
    /// </summary>
    public enum IDerivationState
    {
        /// <summary>
        /// Before being run or (outside batch and not being observed).
        /// at this point derivation is not holding any data about dependency tree
        /// </summary>
        NotTracking = -1,

        /// <summary>
        ///  No shallow dependency changed since last computation.
        ///  We won't recalculate the derivation.
        ///  This is what makes Cortex.Net fast.
        /// </summary>
        UpToDate = 0,

        /// <summary>
        /// Some deep dependency has changed, but we don't know if a shallow dependency has changed.
        /// This will require to check first if this derivation is UpToDate or PossiblyStale.
        /// Currently only ComputedValue will propagate PossiblyStale.
        ///
        /// Having this state is the second big optimization:
        /// We don't have to recompute on every dependency change, but only when it's necessary.
        /// </summary>
        PossiblyStale = 1,

        /// <summary>
        /// A shallow dependency has changed since last computation and the derivation
        /// will need to recompute when it's necessary next.
        /// </summary>
        Stale = 2,
    }
}
