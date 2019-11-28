// <copyright file="ActionExtensions.generated.cs" company="Michel Weststrate, Jan-Willem Spuij">
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
    using Cortex.Net.Properties;
    using System;
    using System.Diagnostics;

    /// <summary>
    /// Extensions class for <see cref="ISharedState"/> instances.
    /// </summary>
    public static partial class ActionExtensions
	{
	    /// <summary>
        /// Creates an Action that triggers reaction in all observables in the shared state.
        /// </summary>
		/// <param name="sharedState">The name of the shared state to use to create this action.</param>
        /// <param name="action">The action itself.</param>
        public static Action CreateAction(this ISharedState sharedState, Action action)
        {
            return CreateAction(sharedState, "<unnamed action>", null, action);
        }

	    /// <summary>
        /// Creates an Action that triggers reaction in all observables in the shared state.
        /// </summary>
		/// <param name="sharedState">The name of the shared state to use to create this action.</param>
        /// <param name="actionName">The name of this action.</param>
        /// <param name="action">The action itself.</param>
        public static Action CreateAction(this ISharedState sharedState, string actionName, Action action)
        {
            return CreateAction(sharedState, actionName, null, action);
        }

	    /// <summary>
        /// Creates an Action that triggers reaction in all observables in the shared state.
        /// </summary>
		/// <param name="sharedState">The name of the shared state to use to create this action.</param>
        /// <param name="actionName">The name of this action.</param>
        /// <param name="scope">The scope of this action.</param>
        /// <param name="action">The action itself.</param>
        public static Action CreateAction(this ISharedState sharedState, string actionName, object scope, Action action)
        {
			if (sharedState is null)
            {
                throw new ArgumentNullException(nameof(sharedState));
            }

            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

			if (string.IsNullOrEmpty(actionName))
            {
                Trace.WriteLine(Resources.ActionNameNull);
            }

            return new Action(() => Core.ActionExtensions.ExecuteAction(sharedState, actionName, scope, action));
        }

	    /// <summary>
        /// Creates an Action that triggers reaction in all observables in the shared state.
        /// </summary>
		/// <param name="sharedState">The name of the shared state to use to create this action.</param>
        /// <param name="action">The action itself.</param>
        public static Action<T1> CreateAction<T1>(this ISharedState sharedState, Action<T1> action)
        {
            return CreateAction<T1>(sharedState, "<unnamed action>", null, action);
        }

	    /// <summary>
        /// Creates an Action that triggers reaction in all observables in the shared state.
        /// </summary>
		/// <param name="sharedState">The name of the shared state to use to create this action.</param>
        /// <param name="actionName">The name of this action.</param>
        /// <param name="action">The action itself.</param>
        public static Action<T1> CreateAction<T1>(this ISharedState sharedState, string actionName, Action<T1> action)
        {
            return CreateAction<T1>(sharedState, actionName, null, action);
        }

	    /// <summary>
        /// Creates an Action that triggers reaction in all observables in the shared state.
        /// </summary>
		/// <param name="sharedState">The name of the shared state to use to create this action.</param>
        /// <param name="actionName">The name of this action.</param>
        /// <param name="scope">The scope of this action.</param>
        /// <param name="action">The action itself.</param>
        public static Action<T1> CreateAction<T1>(this ISharedState sharedState, string actionName, object scope, Action<T1> action)
        {
			if (sharedState is null)
            {
                throw new ArgumentNullException(nameof(sharedState));
            }

            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

			if (string.IsNullOrEmpty(actionName))
            {
                Trace.WriteLine(Resources.ActionNameNull);
            }

            return new Action<T1>((T1 arg1) => Core.ActionExtensions.ExecuteAction<T1>(sharedState, actionName, scope, action, arg1));
        }

	    /// <summary>
        /// Creates an Action that triggers reaction in all observables in the shared state.
        /// </summary>
		/// <param name="sharedState">The name of the shared state to use to create this action.</param>
        /// <param name="action">The action itself.</param>
        public static Action<T1,T2> CreateAction<T1,T2>(this ISharedState sharedState, Action<T1,T2> action)
        {
            return CreateAction<T1,T2>(sharedState, "<unnamed action>", null, action);
        }

	    /// <summary>
        /// Creates an Action that triggers reaction in all observables in the shared state.
        /// </summary>
		/// <param name="sharedState">The name of the shared state to use to create this action.</param>
        /// <param name="actionName">The name of this action.</param>
        /// <param name="action">The action itself.</param>
        public static Action<T1,T2> CreateAction<T1,T2>(this ISharedState sharedState, string actionName, Action<T1,T2> action)
        {
            return CreateAction<T1,T2>(sharedState, actionName, null, action);
        }

	    /// <summary>
        /// Creates an Action that triggers reaction in all observables in the shared state.
        /// </summary>
		/// <param name="sharedState">The name of the shared state to use to create this action.</param>
        /// <param name="actionName">The name of this action.</param>
        /// <param name="scope">The scope of this action.</param>
        /// <param name="action">The action itself.</param>
        public static Action<T1,T2> CreateAction<T1,T2>(this ISharedState sharedState, string actionName, object scope, Action<T1,T2> action)
        {
			if (sharedState is null)
            {
                throw new ArgumentNullException(nameof(sharedState));
            }

            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

			if (string.IsNullOrEmpty(actionName))
            {
                Trace.WriteLine(Resources.ActionNameNull);
            }

            return new Action<T1,T2>((T1 arg1, T2 arg2) => Core.ActionExtensions.ExecuteAction<T1,T2>(sharedState, actionName, scope, action, arg1, arg2));
        }

	    /// <summary>
        /// Creates an Action that triggers reaction in all observables in the shared state.
        /// </summary>
		/// <param name="sharedState">The name of the shared state to use to create this action.</param>
        /// <param name="action">The action itself.</param>
        public static Action<T1,T2,T3> CreateAction<T1,T2,T3>(this ISharedState sharedState, Action<T1,T2,T3> action)
        {
            return CreateAction<T1,T2,T3>(sharedState, "<unnamed action>", null, action);
        }

	    /// <summary>
        /// Creates an Action that triggers reaction in all observables in the shared state.
        /// </summary>
		/// <param name="sharedState">The name of the shared state to use to create this action.</param>
        /// <param name="actionName">The name of this action.</param>
        /// <param name="action">The action itself.</param>
        public static Action<T1,T2,T3> CreateAction<T1,T2,T3>(this ISharedState sharedState, string actionName, Action<T1,T2,T3> action)
        {
            return CreateAction<T1,T2,T3>(sharedState, actionName, null, action);
        }

	    /// <summary>
        /// Creates an Action that triggers reaction in all observables in the shared state.
        /// </summary>
		/// <param name="sharedState">The name of the shared state to use to create this action.</param>
        /// <param name="actionName">The name of this action.</param>
        /// <param name="scope">The scope of this action.</param>
        /// <param name="action">The action itself.</param>
        public static Action<T1,T2,T3> CreateAction<T1,T2,T3>(this ISharedState sharedState, string actionName, object scope, Action<T1,T2,T3> action)
        {
			if (sharedState is null)
            {
                throw new ArgumentNullException(nameof(sharedState));
            }

            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

			if (string.IsNullOrEmpty(actionName))
            {
                Trace.WriteLine(Resources.ActionNameNull);
            }

            return new Action<T1,T2,T3>((T1 arg1, T2 arg2, T3 arg3) => Core.ActionExtensions.ExecuteAction<T1,T2,T3>(sharedState, actionName, scope, action, arg1, arg2, arg3));
        }

	    /// <summary>
        /// Creates an Action that triggers reaction in all observables in the shared state.
        /// </summary>
		/// <param name="sharedState">The name of the shared state to use to create this action.</param>
        /// <param name="action">The action itself.</param>
        public static Action<T1,T2,T3,T4> CreateAction<T1,T2,T3,T4>(this ISharedState sharedState, Action<T1,T2,T3,T4> action)
        {
            return CreateAction<T1,T2,T3,T4>(sharedState, "<unnamed action>", null, action);
        }

	    /// <summary>
        /// Creates an Action that triggers reaction in all observables in the shared state.
        /// </summary>
		/// <param name="sharedState">The name of the shared state to use to create this action.</param>
        /// <param name="actionName">The name of this action.</param>
        /// <param name="action">The action itself.</param>
        public static Action<T1,T2,T3,T4> CreateAction<T1,T2,T3,T4>(this ISharedState sharedState, string actionName, Action<T1,T2,T3,T4> action)
        {
            return CreateAction<T1,T2,T3,T4>(sharedState, actionName, null, action);
        }

	    /// <summary>
        /// Creates an Action that triggers reaction in all observables in the shared state.
        /// </summary>
		/// <param name="sharedState">The name of the shared state to use to create this action.</param>
        /// <param name="actionName">The name of this action.</param>
        /// <param name="scope">The scope of this action.</param>
        /// <param name="action">The action itself.</param>
        public static Action<T1,T2,T3,T4> CreateAction<T1,T2,T3,T4>(this ISharedState sharedState, string actionName, object scope, Action<T1,T2,T3,T4> action)
        {
			if (sharedState is null)
            {
                throw new ArgumentNullException(nameof(sharedState));
            }

            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

			if (string.IsNullOrEmpty(actionName))
            {
                Trace.WriteLine(Resources.ActionNameNull);
            }

            return new Action<T1,T2,T3,T4>((T1 arg1, T2 arg2, T3 arg3, T4 arg4) => Core.ActionExtensions.ExecuteAction<T1,T2,T3,T4>(sharedState, actionName, scope, action, arg1, arg2, arg3, arg4));
        }

	    /// <summary>
        /// Creates an Action that triggers reaction in all observables in the shared state.
        /// </summary>
		/// <param name="sharedState">The name of the shared state to use to create this action.</param>
        /// <param name="action">The action itself.</param>
        public static Action<T1,T2,T3,T4,T5> CreateAction<T1,T2,T3,T4,T5>(this ISharedState sharedState, Action<T1,T2,T3,T4,T5> action)
        {
            return CreateAction<T1,T2,T3,T4,T5>(sharedState, "<unnamed action>", null, action);
        }

	    /// <summary>
        /// Creates an Action that triggers reaction in all observables in the shared state.
        /// </summary>
		/// <param name="sharedState">The name of the shared state to use to create this action.</param>
        /// <param name="actionName">The name of this action.</param>
        /// <param name="action">The action itself.</param>
        public static Action<T1,T2,T3,T4,T5> CreateAction<T1,T2,T3,T4,T5>(this ISharedState sharedState, string actionName, Action<T1,T2,T3,T4,T5> action)
        {
            return CreateAction<T1,T2,T3,T4,T5>(sharedState, actionName, null, action);
        }

	    /// <summary>
        /// Creates an Action that triggers reaction in all observables in the shared state.
        /// </summary>
		/// <param name="sharedState">The name of the shared state to use to create this action.</param>
        /// <param name="actionName">The name of this action.</param>
        /// <param name="scope">The scope of this action.</param>
        /// <param name="action">The action itself.</param>
        public static Action<T1,T2,T3,T4,T5> CreateAction<T1,T2,T3,T4,T5>(this ISharedState sharedState, string actionName, object scope, Action<T1,T2,T3,T4,T5> action)
        {
			if (sharedState is null)
            {
                throw new ArgumentNullException(nameof(sharedState));
            }

            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

			if (string.IsNullOrEmpty(actionName))
            {
                Trace.WriteLine(Resources.ActionNameNull);
            }

            return new Action<T1,T2,T3,T4,T5>((T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) => Core.ActionExtensions.ExecuteAction<T1,T2,T3,T4,T5>(sharedState, actionName, scope, action, arg1, arg2, arg3, arg4, arg5));
        }

	    /// <summary>
        /// Creates an Action that triggers reaction in all observables in the shared state.
        /// </summary>
		/// <param name="sharedState">The name of the shared state to use to create this action.</param>
        /// <param name="action">The action itself.</param>
        public static Action<T1,T2,T3,T4,T5,T6> CreateAction<T1,T2,T3,T4,T5,T6>(this ISharedState sharedState, Action<T1,T2,T3,T4,T5,T6> action)
        {
            return CreateAction<T1,T2,T3,T4,T5,T6>(sharedState, "<unnamed action>", null, action);
        }

	    /// <summary>
        /// Creates an Action that triggers reaction in all observables in the shared state.
        /// </summary>
		/// <param name="sharedState">The name of the shared state to use to create this action.</param>
        /// <param name="actionName">The name of this action.</param>
        /// <param name="action">The action itself.</param>
        public static Action<T1,T2,T3,T4,T5,T6> CreateAction<T1,T2,T3,T4,T5,T6>(this ISharedState sharedState, string actionName, Action<T1,T2,T3,T4,T5,T6> action)
        {
            return CreateAction<T1,T2,T3,T4,T5,T6>(sharedState, actionName, null, action);
        }

	    /// <summary>
        /// Creates an Action that triggers reaction in all observables in the shared state.
        /// </summary>
		/// <param name="sharedState">The name of the shared state to use to create this action.</param>
        /// <param name="actionName">The name of this action.</param>
        /// <param name="scope">The scope of this action.</param>
        /// <param name="action">The action itself.</param>
        public static Action<T1,T2,T3,T4,T5,T6> CreateAction<T1,T2,T3,T4,T5,T6>(this ISharedState sharedState, string actionName, object scope, Action<T1,T2,T3,T4,T5,T6> action)
        {
			if (sharedState is null)
            {
                throw new ArgumentNullException(nameof(sharedState));
            }

            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

			if (string.IsNullOrEmpty(actionName))
            {
                Trace.WriteLine(Resources.ActionNameNull);
            }

            return new Action<T1,T2,T3,T4,T5,T6>((T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6) => Core.ActionExtensions.ExecuteAction<T1,T2,T3,T4,T5,T6>(sharedState, actionName, scope, action, arg1, arg2, arg3, arg4, arg5, arg6));
        }

	    /// <summary>
        /// Creates an Action that triggers reaction in all observables in the shared state.
        /// </summary>
		/// <param name="sharedState">The name of the shared state to use to create this action.</param>
        /// <param name="action">The action itself.</param>
        public static Action<T1,T2,T3,T4,T5,T6,T7> CreateAction<T1,T2,T3,T4,T5,T6,T7>(this ISharedState sharedState, Action<T1,T2,T3,T4,T5,T6,T7> action)
        {
            return CreateAction<T1,T2,T3,T4,T5,T6,T7>(sharedState, "<unnamed action>", null, action);
        }

	    /// <summary>
        /// Creates an Action that triggers reaction in all observables in the shared state.
        /// </summary>
		/// <param name="sharedState">The name of the shared state to use to create this action.</param>
        /// <param name="actionName">The name of this action.</param>
        /// <param name="action">The action itself.</param>
        public static Action<T1,T2,T3,T4,T5,T6,T7> CreateAction<T1,T2,T3,T4,T5,T6,T7>(this ISharedState sharedState, string actionName, Action<T1,T2,T3,T4,T5,T6,T7> action)
        {
            return CreateAction<T1,T2,T3,T4,T5,T6,T7>(sharedState, actionName, null, action);
        }

	    /// <summary>
        /// Creates an Action that triggers reaction in all observables in the shared state.
        /// </summary>
		/// <param name="sharedState">The name of the shared state to use to create this action.</param>
        /// <param name="actionName">The name of this action.</param>
        /// <param name="scope">The scope of this action.</param>
        /// <param name="action">The action itself.</param>
        public static Action<T1,T2,T3,T4,T5,T6,T7> CreateAction<T1,T2,T3,T4,T5,T6,T7>(this ISharedState sharedState, string actionName, object scope, Action<T1,T2,T3,T4,T5,T6,T7> action)
        {
			if (sharedState is null)
            {
                throw new ArgumentNullException(nameof(sharedState));
            }

            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

			if (string.IsNullOrEmpty(actionName))
            {
                Trace.WriteLine(Resources.ActionNameNull);
            }

            return new Action<T1,T2,T3,T4,T5,T6,T7>((T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7) => Core.ActionExtensions.ExecuteAction<T1,T2,T3,T4,T5,T6,T7>(sharedState, actionName, scope, action, arg1, arg2, arg3, arg4, arg5, arg6, arg7));
        }

	    /// <summary>
        /// Creates an Action that triggers reaction in all observables in the shared state.
        /// </summary>
		/// <param name="sharedState">The name of the shared state to use to create this action.</param>
        /// <param name="action">The action itself.</param>
        public static Action<T1,T2,T3,T4,T5,T6,T7,T8> CreateAction<T1,T2,T3,T4,T5,T6,T7,T8>(this ISharedState sharedState, Action<T1,T2,T3,T4,T5,T6,T7,T8> action)
        {
            return CreateAction<T1,T2,T3,T4,T5,T6,T7,T8>(sharedState, "<unnamed action>", null, action);
        }

	    /// <summary>
        /// Creates an Action that triggers reaction in all observables in the shared state.
        /// </summary>
		/// <param name="sharedState">The name of the shared state to use to create this action.</param>
        /// <param name="actionName">The name of this action.</param>
        /// <param name="action">The action itself.</param>
        public static Action<T1,T2,T3,T4,T5,T6,T7,T8> CreateAction<T1,T2,T3,T4,T5,T6,T7,T8>(this ISharedState sharedState, string actionName, Action<T1,T2,T3,T4,T5,T6,T7,T8> action)
        {
            return CreateAction<T1,T2,T3,T4,T5,T6,T7,T8>(sharedState, actionName, null, action);
        }

	    /// <summary>
        /// Creates an Action that triggers reaction in all observables in the shared state.
        /// </summary>
		/// <param name="sharedState">The name of the shared state to use to create this action.</param>
        /// <param name="actionName">The name of this action.</param>
        /// <param name="scope">The scope of this action.</param>
        /// <param name="action">The action itself.</param>
        public static Action<T1,T2,T3,T4,T5,T6,T7,T8> CreateAction<T1,T2,T3,T4,T5,T6,T7,T8>(this ISharedState sharedState, string actionName, object scope, Action<T1,T2,T3,T4,T5,T6,T7,T8> action)
        {
			if (sharedState is null)
            {
                throw new ArgumentNullException(nameof(sharedState));
            }

            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

			if (string.IsNullOrEmpty(actionName))
            {
                Trace.WriteLine(Resources.ActionNameNull);
            }

            return new Action<T1,T2,T3,T4,T5,T6,T7,T8>((T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8) => Core.ActionExtensions.ExecuteAction<T1,T2,T3,T4,T5,T6,T7,T8>(sharedState, actionName, scope, action, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8));
        }

	    /// <summary>
        /// Creates an Action that triggers reaction in all observables in the shared state.
        /// </summary>
		/// <param name="sharedState">The name of the shared state to use to create this action.</param>
        /// <param name="action">The action itself.</param>
        public static Action<T1,T2,T3,T4,T5,T6,T7,T8,T9> CreateAction<T1,T2,T3,T4,T5,T6,T7,T8,T9>(this ISharedState sharedState, Action<T1,T2,T3,T4,T5,T6,T7,T8,T9> action)
        {
            return CreateAction<T1,T2,T3,T4,T5,T6,T7,T8,T9>(sharedState, "<unnamed action>", null, action);
        }

	    /// <summary>
        /// Creates an Action that triggers reaction in all observables in the shared state.
        /// </summary>
		/// <param name="sharedState">The name of the shared state to use to create this action.</param>
        /// <param name="actionName">The name of this action.</param>
        /// <param name="action">The action itself.</param>
        public static Action<T1,T2,T3,T4,T5,T6,T7,T8,T9> CreateAction<T1,T2,T3,T4,T5,T6,T7,T8,T9>(this ISharedState sharedState, string actionName, Action<T1,T2,T3,T4,T5,T6,T7,T8,T9> action)
        {
            return CreateAction<T1,T2,T3,T4,T5,T6,T7,T8,T9>(sharedState, actionName, null, action);
        }

	    /// <summary>
        /// Creates an Action that triggers reaction in all observables in the shared state.
        /// </summary>
		/// <param name="sharedState">The name of the shared state to use to create this action.</param>
        /// <param name="actionName">The name of this action.</param>
        /// <param name="scope">The scope of this action.</param>
        /// <param name="action">The action itself.</param>
        public static Action<T1,T2,T3,T4,T5,T6,T7,T8,T9> CreateAction<T1,T2,T3,T4,T5,T6,T7,T8,T9>(this ISharedState sharedState, string actionName, object scope, Action<T1,T2,T3,T4,T5,T6,T7,T8,T9> action)
        {
			if (sharedState is null)
            {
                throw new ArgumentNullException(nameof(sharedState));
            }

            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

			if (string.IsNullOrEmpty(actionName))
            {
                Trace.WriteLine(Resources.ActionNameNull);
            }

            return new Action<T1,T2,T3,T4,T5,T6,T7,T8,T9>((T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9) => Core.ActionExtensions.ExecuteAction<T1,T2,T3,T4,T5,T6,T7,T8,T9>(sharedState, actionName, scope, action, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9));
        }

	    /// <summary>
        /// Creates an Action that triggers reaction in all observables in the shared state.
        /// </summary>
		/// <param name="sharedState">The name of the shared state to use to create this action.</param>
        /// <param name="action">The action itself.</param>
        public static Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10> CreateAction<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10>(this ISharedState sharedState, Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10> action)
        {
            return CreateAction<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10>(sharedState, "<unnamed action>", null, action);
        }

	    /// <summary>
        /// Creates an Action that triggers reaction in all observables in the shared state.
        /// </summary>
		/// <param name="sharedState">The name of the shared state to use to create this action.</param>
        /// <param name="actionName">The name of this action.</param>
        /// <param name="action">The action itself.</param>
        public static Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10> CreateAction<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10>(this ISharedState sharedState, string actionName, Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10> action)
        {
            return CreateAction<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10>(sharedState, actionName, null, action);
        }

	    /// <summary>
        /// Creates an Action that triggers reaction in all observables in the shared state.
        /// </summary>
		/// <param name="sharedState">The name of the shared state to use to create this action.</param>
        /// <param name="actionName">The name of this action.</param>
        /// <param name="scope">The scope of this action.</param>
        /// <param name="action">The action itself.</param>
        public static Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10> CreateAction<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10>(this ISharedState sharedState, string actionName, object scope, Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10> action)
        {
			if (sharedState is null)
            {
                throw new ArgumentNullException(nameof(sharedState));
            }

            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

			if (string.IsNullOrEmpty(actionName))
            {
                Trace.WriteLine(Resources.ActionNameNull);
            }

            return new Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10>((T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10) => Core.ActionExtensions.ExecuteAction<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10>(sharedState, actionName, scope, action, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10));
        }

	    /// <summary>
        /// Creates an Action that triggers reaction in all observables in the shared state.
        /// </summary>
		/// <param name="sharedState">The name of the shared state to use to create this action.</param>
        /// <param name="action">The action itself.</param>
        public static Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11> CreateAction<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11>(this ISharedState sharedState, Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11> action)
        {
            return CreateAction<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11>(sharedState, "<unnamed action>", null, action);
        }

	    /// <summary>
        /// Creates an Action that triggers reaction in all observables in the shared state.
        /// </summary>
		/// <param name="sharedState">The name of the shared state to use to create this action.</param>
        /// <param name="actionName">The name of this action.</param>
        /// <param name="action">The action itself.</param>
        public static Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11> CreateAction<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11>(this ISharedState sharedState, string actionName, Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11> action)
        {
            return CreateAction<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11>(sharedState, actionName, null, action);
        }

	    /// <summary>
        /// Creates an Action that triggers reaction in all observables in the shared state.
        /// </summary>
		/// <param name="sharedState">The name of the shared state to use to create this action.</param>
        /// <param name="actionName">The name of this action.</param>
        /// <param name="scope">The scope of this action.</param>
        /// <param name="action">The action itself.</param>
        public static Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11> CreateAction<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11>(this ISharedState sharedState, string actionName, object scope, Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11> action)
        {
			if (sharedState is null)
            {
                throw new ArgumentNullException(nameof(sharedState));
            }

            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

			if (string.IsNullOrEmpty(actionName))
            {
                Trace.WriteLine(Resources.ActionNameNull);
            }

            return new Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11>((T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11) => Core.ActionExtensions.ExecuteAction<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11>(sharedState, actionName, scope, action, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11));
        }

	    /// <summary>
        /// Creates an Action that triggers reaction in all observables in the shared state.
        /// </summary>
		/// <param name="sharedState">The name of the shared state to use to create this action.</param>
        /// <param name="action">The action itself.</param>
        public static Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12> CreateAction<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12>(this ISharedState sharedState, Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12> action)
        {
            return CreateAction<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12>(sharedState, "<unnamed action>", null, action);
        }

	    /// <summary>
        /// Creates an Action that triggers reaction in all observables in the shared state.
        /// </summary>
		/// <param name="sharedState">The name of the shared state to use to create this action.</param>
        /// <param name="actionName">The name of this action.</param>
        /// <param name="action">The action itself.</param>
        public static Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12> CreateAction<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12>(this ISharedState sharedState, string actionName, Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12> action)
        {
            return CreateAction<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12>(sharedState, actionName, null, action);
        }

	    /// <summary>
        /// Creates an Action that triggers reaction in all observables in the shared state.
        /// </summary>
		/// <param name="sharedState">The name of the shared state to use to create this action.</param>
        /// <param name="actionName">The name of this action.</param>
        /// <param name="scope">The scope of this action.</param>
        /// <param name="action">The action itself.</param>
        public static Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12> CreateAction<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12>(this ISharedState sharedState, string actionName, object scope, Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12> action)
        {
			if (sharedState is null)
            {
                throw new ArgumentNullException(nameof(sharedState));
            }

            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

			if (string.IsNullOrEmpty(actionName))
            {
                Trace.WriteLine(Resources.ActionNameNull);
            }

            return new Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12>((T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12) => Core.ActionExtensions.ExecuteAction<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12>(sharedState, actionName, scope, action, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12));
        }

	    /// <summary>
        /// Creates an Action that triggers reaction in all observables in the shared state.
        /// </summary>
		/// <param name="sharedState">The name of the shared state to use to create this action.</param>
        /// <param name="action">The action itself.</param>
        public static Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13> CreateAction<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13>(this ISharedState sharedState, Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13> action)
        {
            return CreateAction<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13>(sharedState, "<unnamed action>", null, action);
        }

	    /// <summary>
        /// Creates an Action that triggers reaction in all observables in the shared state.
        /// </summary>
		/// <param name="sharedState">The name of the shared state to use to create this action.</param>
        /// <param name="actionName">The name of this action.</param>
        /// <param name="action">The action itself.</param>
        public static Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13> CreateAction<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13>(this ISharedState sharedState, string actionName, Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13> action)
        {
            return CreateAction<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13>(sharedState, actionName, null, action);
        }

	    /// <summary>
        /// Creates an Action that triggers reaction in all observables in the shared state.
        /// </summary>
		/// <param name="sharedState">The name of the shared state to use to create this action.</param>
        /// <param name="actionName">The name of this action.</param>
        /// <param name="scope">The scope of this action.</param>
        /// <param name="action">The action itself.</param>
        public static Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13> CreateAction<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13>(this ISharedState sharedState, string actionName, object scope, Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13> action)
        {
			if (sharedState is null)
            {
                throw new ArgumentNullException(nameof(sharedState));
            }

            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

			if (string.IsNullOrEmpty(actionName))
            {
                Trace.WriteLine(Resources.ActionNameNull);
            }

            return new Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13>((T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13) => Core.ActionExtensions.ExecuteAction<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13>(sharedState, actionName, scope, action, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13));
        }

	    /// <summary>
        /// Creates an Action that triggers reaction in all observables in the shared state.
        /// </summary>
		/// <param name="sharedState">The name of the shared state to use to create this action.</param>
        /// <param name="action">The action itself.</param>
        public static Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14> CreateAction<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14>(this ISharedState sharedState, Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14> action)
        {
            return CreateAction<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14>(sharedState, "<unnamed action>", null, action);
        }

	    /// <summary>
        /// Creates an Action that triggers reaction in all observables in the shared state.
        /// </summary>
		/// <param name="sharedState">The name of the shared state to use to create this action.</param>
        /// <param name="actionName">The name of this action.</param>
        /// <param name="action">The action itself.</param>
        public static Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14> CreateAction<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14>(this ISharedState sharedState, string actionName, Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14> action)
        {
            return CreateAction<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14>(sharedState, actionName, null, action);
        }

	    /// <summary>
        /// Creates an Action that triggers reaction in all observables in the shared state.
        /// </summary>
		/// <param name="sharedState">The name of the shared state to use to create this action.</param>
        /// <param name="actionName">The name of this action.</param>
        /// <param name="scope">The scope of this action.</param>
        /// <param name="action">The action itself.</param>
        public static Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14> CreateAction<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14>(this ISharedState sharedState, string actionName, object scope, Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14> action)
        {
			if (sharedState is null)
            {
                throw new ArgumentNullException(nameof(sharedState));
            }

            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

			if (string.IsNullOrEmpty(actionName))
            {
                Trace.WriteLine(Resources.ActionNameNull);
            }

            return new Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14>((T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14) => Core.ActionExtensions.ExecuteAction<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14>(sharedState, actionName, scope, action, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14));
        }

	    /// <summary>
        /// Creates an Action that triggers reaction in all observables in the shared state.
        /// </summary>
		/// <param name="sharedState">The name of the shared state to use to create this action.</param>
        /// <param name="action">The action itself.</param>
        public static Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15> CreateAction<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15>(this ISharedState sharedState, Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15> action)
        {
            return CreateAction<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15>(sharedState, "<unnamed action>", null, action);
        }

	    /// <summary>
        /// Creates an Action that triggers reaction in all observables in the shared state.
        /// </summary>
		/// <param name="sharedState">The name of the shared state to use to create this action.</param>
        /// <param name="actionName">The name of this action.</param>
        /// <param name="action">The action itself.</param>
        public static Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15> CreateAction<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15>(this ISharedState sharedState, string actionName, Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15> action)
        {
            return CreateAction<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15>(sharedState, actionName, null, action);
        }

	    /// <summary>
        /// Creates an Action that triggers reaction in all observables in the shared state.
        /// </summary>
		/// <param name="sharedState">The name of the shared state to use to create this action.</param>
        /// <param name="actionName">The name of this action.</param>
        /// <param name="scope">The scope of this action.</param>
        /// <param name="action">The action itself.</param>
        public static Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15> CreateAction<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15>(this ISharedState sharedState, string actionName, object scope, Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15> action)
        {
			if (sharedState is null)
            {
                throw new ArgumentNullException(nameof(sharedState));
            }

            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

			if (string.IsNullOrEmpty(actionName))
            {
                Trace.WriteLine(Resources.ActionNameNull);
            }

            return new Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15>((T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15) => Core.ActionExtensions.ExecuteAction<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15>(sharedState, actionName, scope, action, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15));
        }

	    /// <summary>
        /// Creates an Action that triggers reaction in all observables in the shared state.
        /// </summary>
		/// <param name="sharedState">The name of the shared state to use to create this action.</param>
        /// <param name="action">The action itself.</param>
        public static Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16> CreateAction<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16>(this ISharedState sharedState, Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16> action)
        {
            return CreateAction<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16>(sharedState, "<unnamed action>", null, action);
        }

	    /// <summary>
        /// Creates an Action that triggers reaction in all observables in the shared state.
        /// </summary>
		/// <param name="sharedState">The name of the shared state to use to create this action.</param>
        /// <param name="actionName">The name of this action.</param>
        /// <param name="action">The action itself.</param>
        public static Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16> CreateAction<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16>(this ISharedState sharedState, string actionName, Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16> action)
        {
            return CreateAction<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16>(sharedState, actionName, null, action);
        }

	    /// <summary>
        /// Creates an Action that triggers reaction in all observables in the shared state.
        /// </summary>
		/// <param name="sharedState">The name of the shared state to use to create this action.</param>
        /// <param name="actionName">The name of this action.</param>
        /// <param name="scope">The scope of this action.</param>
        /// <param name="action">The action itself.</param>
        public static Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16> CreateAction<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16>(this ISharedState sharedState, string actionName, object scope, Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16> action)
        {
			if (sharedState is null)
            {
                throw new ArgumentNullException(nameof(sharedState));
            }

            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

			if (string.IsNullOrEmpty(actionName))
            {
                Trace.WriteLine(Resources.ActionNameNull);
            }

            return new Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16>((T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16) => Core.ActionExtensions.ExecuteAction<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16>(sharedState, actionName, scope, action, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16));
        }

	}
}