// <copyright file="SharedState.generated.cs" company="Michel Weststrate, Jan-Willem Spuij">
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
    using Cortex.Net.Core;
    using Cortex.Net.Properties;
    using System;
    using System.Diagnostics;

    /// <summary>
    /// Holds the Shared state that all nodes of the Dependency Graph share.
    /// </summary>
    public sealed partial class SharedState
	{
	    /// <summary>
        /// Creates an Action that triggers reaction in all observables in the shared state.
        /// </summary>
        /// <param name="actionName">The name of this action.</param>
        /// <param name="scope">The scope of this action.</param>
        /// <param name="action">The action itself.</param>
        public Action CreateAction(string actionName, object scope, Action action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

			if (string.IsNullOrEmpty(actionName))
            {
                Trace.WriteLine(Resources.ActionNameNull);
            }

            return new Action(() => ActionExtensions.ExecuteAction(this, actionName, scope, action));
        }

	    /// <summary>
        /// Creates an Action that triggers reaction in all observables in the shared state.
        /// </summary>
        /// <param name="actionName">The name of this action.</param>
        /// <param name="scope">The scope of this action.</param>
        /// <param name="action">The action itself.</param>
        public Action<T1> CreateAction<T1>(string actionName, object scope, Action<T1> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

			if (string.IsNullOrEmpty(actionName))
            {
                Trace.WriteLine(Resources.ActionNameNull);
            }

            return new Action<T1>((T1 arg1) => ActionExtensions.ExecuteAction<T1>(this, actionName, scope, action, arg1));
        }

	    /// <summary>
        /// Creates an Action that triggers reaction in all observables in the shared state.
        /// </summary>
        /// <param name="actionName">The name of this action.</param>
        /// <param name="scope">The scope of this action.</param>
        /// <param name="action">The action itself.</param>
        public Action<T1,T2> CreateAction<T1,T2>(string actionName, object scope, Action<T1,T2> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

			if (string.IsNullOrEmpty(actionName))
            {
                Trace.WriteLine(Resources.ActionNameNull);
            }

            return new Action<T1,T2>((T1 arg1, T2 arg2) => ActionExtensions.ExecuteAction<T1,T2>(this, actionName, scope, action, arg1, arg2));
        }

	    /// <summary>
        /// Creates an Action that triggers reaction in all observables in the shared state.
        /// </summary>
        /// <param name="actionName">The name of this action.</param>
        /// <param name="scope">The scope of this action.</param>
        /// <param name="action">The action itself.</param>
        public Action<T1,T2,T3> CreateAction<T1,T2,T3>(string actionName, object scope, Action<T1,T2,T3> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

			if (string.IsNullOrEmpty(actionName))
            {
                Trace.WriteLine(Resources.ActionNameNull);
            }

            return new Action<T1,T2,T3>((T1 arg1, T2 arg2, T3 arg3) => ActionExtensions.ExecuteAction<T1,T2,T3>(this, actionName, scope, action, arg1, arg2, arg3));
        }

	    /// <summary>
        /// Creates an Action that triggers reaction in all observables in the shared state.
        /// </summary>
        /// <param name="actionName">The name of this action.</param>
        /// <param name="scope">The scope of this action.</param>
        /// <param name="action">The action itself.</param>
        public Action<T1,T2,T3,T4> CreateAction<T1,T2,T3,T4>(string actionName, object scope, Action<T1,T2,T3,T4> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

			if (string.IsNullOrEmpty(actionName))
            {
                Trace.WriteLine(Resources.ActionNameNull);
            }

            return new Action<T1,T2,T3,T4>((T1 arg1, T2 arg2, T3 arg3, T4 arg4) => ActionExtensions.ExecuteAction<T1,T2,T3,T4>(this, actionName, scope, action, arg1, arg2, arg3, arg4));
        }

	    /// <summary>
        /// Creates an Action that triggers reaction in all observables in the shared state.
        /// </summary>
        /// <param name="actionName">The name of this action.</param>
        /// <param name="scope">The scope of this action.</param>
        /// <param name="action">The action itself.</param>
        public Action<T1,T2,T3,T4,T5> CreateAction<T1,T2,T3,T4,T5>(string actionName, object scope, Action<T1,T2,T3,T4,T5> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

			if (string.IsNullOrEmpty(actionName))
            {
                Trace.WriteLine(Resources.ActionNameNull);
            }

            return new Action<T1,T2,T3,T4,T5>((T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) => ActionExtensions.ExecuteAction<T1,T2,T3,T4,T5>(this, actionName, scope, action, arg1, arg2, arg3, arg4, arg5));
        }

	    /// <summary>
        /// Creates an Action that triggers reaction in all observables in the shared state.
        /// </summary>
        /// <param name="actionName">The name of this action.</param>
        /// <param name="scope">The scope of this action.</param>
        /// <param name="action">The action itself.</param>
        public Action<T1,T2,T3,T4,T5,T6> CreateAction<T1,T2,T3,T4,T5,T6>(string actionName, object scope, Action<T1,T2,T3,T4,T5,T6> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

			if (string.IsNullOrEmpty(actionName))
            {
                Trace.WriteLine(Resources.ActionNameNull);
            }

            return new Action<T1,T2,T3,T4,T5,T6>((T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6) => ActionExtensions.ExecuteAction<T1,T2,T3,T4,T5,T6>(this, actionName, scope, action, arg1, arg2, arg3, arg4, arg5, arg6));
        }

	    /// <summary>
        /// Creates an Action that triggers reaction in all observables in the shared state.
        /// </summary>
        /// <param name="actionName">The name of this action.</param>
        /// <param name="scope">The scope of this action.</param>
        /// <param name="action">The action itself.</param>
        public Action<T1,T2,T3,T4,T5,T6,T7> CreateAction<T1,T2,T3,T4,T5,T6,T7>(string actionName, object scope, Action<T1,T2,T3,T4,T5,T6,T7> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

			if (string.IsNullOrEmpty(actionName))
            {
                Trace.WriteLine(Resources.ActionNameNull);
            }

            return new Action<T1,T2,T3,T4,T5,T6,T7>((T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7) => ActionExtensions.ExecuteAction<T1,T2,T3,T4,T5,T6,T7>(this, actionName, scope, action, arg1, arg2, arg3, arg4, arg5, arg6, arg7));
        }

	    /// <summary>
        /// Creates an Action that triggers reaction in all observables in the shared state.
        /// </summary>
        /// <param name="actionName">The name of this action.</param>
        /// <param name="scope">The scope of this action.</param>
        /// <param name="action">The action itself.</param>
        public Action<T1,T2,T3,T4,T5,T6,T7,T8> CreateAction<T1,T2,T3,T4,T5,T6,T7,T8>(string actionName, object scope, Action<T1,T2,T3,T4,T5,T6,T7,T8> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

			if (string.IsNullOrEmpty(actionName))
            {
                Trace.WriteLine(Resources.ActionNameNull);
            }

            return new Action<T1,T2,T3,T4,T5,T6,T7,T8>((T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8) => ActionExtensions.ExecuteAction<T1,T2,T3,T4,T5,T6,T7,T8>(this, actionName, scope, action, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8));
        }

	    /// <summary>
        /// Creates an Action that triggers reaction in all observables in the shared state.
        /// </summary>
        /// <param name="actionName">The name of this action.</param>
        /// <param name="scope">The scope of this action.</param>
        /// <param name="action">The action itself.</param>
        public Action<T1,T2,T3,T4,T5,T6,T7,T8,T9> CreateAction<T1,T2,T3,T4,T5,T6,T7,T8,T9>(string actionName, object scope, Action<T1,T2,T3,T4,T5,T6,T7,T8,T9> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

			if (string.IsNullOrEmpty(actionName))
            {
                Trace.WriteLine(Resources.ActionNameNull);
            }

            return new Action<T1,T2,T3,T4,T5,T6,T7,T8,T9>((T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9) => ActionExtensions.ExecuteAction<T1,T2,T3,T4,T5,T6,T7,T8,T9>(this, actionName, scope, action, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9));
        }

	    /// <summary>
        /// Creates an Action that triggers reaction in all observables in the shared state.
        /// </summary>
        /// <param name="actionName">The name of this action.</param>
        /// <param name="scope">The scope of this action.</param>
        /// <param name="action">The action itself.</param>
        public Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10> CreateAction<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10>(string actionName, object scope, Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

			if (string.IsNullOrEmpty(actionName))
            {
                Trace.WriteLine(Resources.ActionNameNull);
            }

            return new Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10>((T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10) => ActionExtensions.ExecuteAction<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10>(this, actionName, scope, action, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10));
        }

	    /// <summary>
        /// Creates an Action that triggers reaction in all observables in the shared state.
        /// </summary>
        /// <param name="actionName">The name of this action.</param>
        /// <param name="scope">The scope of this action.</param>
        /// <param name="action">The action itself.</param>
        public Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11> CreateAction<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11>(string actionName, object scope, Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

			if (string.IsNullOrEmpty(actionName))
            {
                Trace.WriteLine(Resources.ActionNameNull);
            }

            return new Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11>((T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11) => ActionExtensions.ExecuteAction<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11>(this, actionName, scope, action, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11));
        }

	    /// <summary>
        /// Creates an Action that triggers reaction in all observables in the shared state.
        /// </summary>
        /// <param name="actionName">The name of this action.</param>
        /// <param name="scope">The scope of this action.</param>
        /// <param name="action">The action itself.</param>
        public Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12> CreateAction<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12>(string actionName, object scope, Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

			if (string.IsNullOrEmpty(actionName))
            {
                Trace.WriteLine(Resources.ActionNameNull);
            }

            return new Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12>((T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12) => ActionExtensions.ExecuteAction<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12>(this, actionName, scope, action, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12));
        }

	    /// <summary>
        /// Creates an Action that triggers reaction in all observables in the shared state.
        /// </summary>
        /// <param name="actionName">The name of this action.</param>
        /// <param name="scope">The scope of this action.</param>
        /// <param name="action">The action itself.</param>
        public Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13> CreateAction<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13>(string actionName, object scope, Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

			if (string.IsNullOrEmpty(actionName))
            {
                Trace.WriteLine(Resources.ActionNameNull);
            }

            return new Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13>((T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13) => ActionExtensions.ExecuteAction<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13>(this, actionName, scope, action, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13));
        }

	    /// <summary>
        /// Creates an Action that triggers reaction in all observables in the shared state.
        /// </summary>
        /// <param name="actionName">The name of this action.</param>
        /// <param name="scope">The scope of this action.</param>
        /// <param name="action">The action itself.</param>
        public Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14> CreateAction<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14>(string actionName, object scope, Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

			if (string.IsNullOrEmpty(actionName))
            {
                Trace.WriteLine(Resources.ActionNameNull);
            }

            return new Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14>((T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14) => ActionExtensions.ExecuteAction<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14>(this, actionName, scope, action, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14));
        }

	    /// <summary>
        /// Creates an Action that triggers reaction in all observables in the shared state.
        /// </summary>
        /// <param name="actionName">The name of this action.</param>
        /// <param name="scope">The scope of this action.</param>
        /// <param name="action">The action itself.</param>
        public Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15> CreateAction<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15>(string actionName, object scope, Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

			if (string.IsNullOrEmpty(actionName))
            {
                Trace.WriteLine(Resources.ActionNameNull);
            }

            return new Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15>((T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15) => ActionExtensions.ExecuteAction<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15>(this, actionName, scope, action, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15));
        }

	    /// <summary>
        /// Creates an Action that triggers reaction in all observables in the shared state.
        /// </summary>
        /// <param name="actionName">The name of this action.</param>
        /// <param name="scope">The scope of this action.</param>
        /// <param name="action">The action itself.</param>
        public Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16> CreateAction<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16>(string actionName, object scope, Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16> action)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

			if (string.IsNullOrEmpty(actionName))
            {
                Trace.WriteLine(Resources.ActionNameNull);
            }

            return new Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16>((T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16) => ActionExtensions.ExecuteAction<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16>(this, actionName, scope, action, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16));
        }

	}
}