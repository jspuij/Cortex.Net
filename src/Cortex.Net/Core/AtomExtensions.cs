// <copyright file="AtomExtensions.cs" company="Michel Weststrate, Jan-Willem Spuij">
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
    using System.Diagnostics;
    using System.Globalization;
    using System.Text;
    using Cortex.Net.Properties;

    /// <summary>
    /// Extension methods for the <see cref="IAtom"/> interface.
    /// </summary>
    public static class AtomExtensions
    {
        /// <summary>
        /// Checks if State modifications are allowed and writes a warning to the Trace log.
        /// </summary>
        /// <param name="atom">The observable to report.</param>
        internal static void CheckIfStateModificationsAreAllowed(this IAtom atom)
        {
            var sharedState = atom.SharedState;
            var configuration = sharedState.Configuration;

            // Should never be possible to change an observed observable from inside computed, see Mobx #798
            if (sharedState.ComputationDepth > 0 && atom.HasObservers())
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.ComputedValuesAreNotAllowedToCauseSideEffects, atom.Name));
            }

            // Should not be possible to change observed state outside strict mode, except during initialization, see Mobx #563
            if (!sharedState.AllowStateChanges && (atom.HasObservers() || sharedState.Configuration.EnforceActions == EnforceAction.Always))
            {
                throw new InvalidOperationException(string.Format(
                    CultureInfo.CurrentCulture,
                    sharedState.Configuration.EnforceActions == EnforceAction.Always ? Resources.ModifiedOutsideActionEnforceAlways : Resources.ModifiedOutsideAction,
                    atom.Name));
            }
        }
    }
}
