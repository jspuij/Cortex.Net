// <copyright file="ActionExtensions.cs" company="Michel Weststrate, Jan-Willem Spuij">
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
    /// Extension methods for <see cref="Action"/> delegates.
    /// </summary>
    public static partial class ActionExtensions
	{
	    /// <summary>
        /// Executes an Action that triggers reaction in all observables in the shared state.
        /// </summary>
        /// <param name="sharedState">The <see cref="ISharedState"/> instance to use.</param>
        /// <param name="actionName">The name of this action.</param>
        /// <param name="scope">The scope of this action.</param>
        /// <param name="action">The action itself.</param>

        internal static void ExecuteAction(ISharedState sharedState, string actionName, object scope, Action action)
        {
            ActionRunInfo actionRunInfo = StartAction(sharedState, actionName, scope, Array.Empty<object>());

            try
            {
                action.Invoke();
            }
            catch (Exception exception)
            {
                actionRunInfo.Exception = exception;
                throw;
            }
            finally
            {
                EndAction(actionRunInfo);
            }
        }

	    /// <summary>
        /// Executes an Action that triggers reaction in all observables in the shared state.
        /// </summary>
        /// <param name="sharedState">The <see cref="ISharedState"/> instance to use.</param>
        /// <param name="actionName">The name of this action.</param>
        /// <param name="scope">The scope of this action.</param>
        /// <param name="action">The action itself.</param>
        /// <param name="arg1">Argument nr. 1.</param>
        internal static void ExecuteAction<T1>(ISharedState sharedState, string actionName, object scope, Action<T1> action, T1 arg1)
        {
            ActionRunInfo actionRunInfo = StartAction(sharedState, actionName, scope, new object[] {arg1});

            try
            {
                action.Invoke(arg1);
            }
            catch (Exception exception)
            {
                actionRunInfo.Exception = exception;
                throw;
            }
            finally
            {
                EndAction(actionRunInfo);
            }
        }

	    /// <summary>
        /// Executes an Action that triggers reaction in all observables in the shared state.
        /// </summary>
        /// <param name="sharedState">The <see cref="ISharedState"/> instance to use.</param>
        /// <param name="actionName">The name of this action.</param>
        /// <param name="scope">The scope of this action.</param>
        /// <param name="action">The action itself.</param>
        /// <param name="arg1">Argument nr. 1.</param>
        /// <param name="arg2">Argument nr. 2.</param>
        internal static void ExecuteAction<T1,T2>(ISharedState sharedState, string actionName, object scope, Action<T1,T2> action, T1 arg1, T2 arg2)
        {
            ActionRunInfo actionRunInfo = StartAction(sharedState, actionName, scope, new object[] {arg1, arg2});

            try
            {
                action.Invoke(arg1, arg2);
            }
            catch (Exception exception)
            {
                actionRunInfo.Exception = exception;
                throw;
            }
            finally
            {
                EndAction(actionRunInfo);
            }
        }

	    /// <summary>
        /// Executes an Action that triggers reaction in all observables in the shared state.
        /// </summary>
        /// <param name="sharedState">The <see cref="ISharedState"/> instance to use.</param>
        /// <param name="actionName">The name of this action.</param>
        /// <param name="scope">The scope of this action.</param>
        /// <param name="action">The action itself.</param>
        /// <param name="arg1">Argument nr. 1.</param>
        /// <param name="arg2">Argument nr. 2.</param>
        /// <param name="arg3">Argument nr. 3.</param>
        internal static void ExecuteAction<T1,T2,T3>(ISharedState sharedState, string actionName, object scope, Action<T1,T2,T3> action, T1 arg1, T2 arg2, T3 arg3)
        {
            ActionRunInfo actionRunInfo = StartAction(sharedState, actionName, scope, new object[] {arg1, arg2, arg3});

            try
            {
                action.Invoke(arg1, arg2, arg3);
            }
            catch (Exception exception)
            {
                actionRunInfo.Exception = exception;
                throw;
            }
            finally
            {
                EndAction(actionRunInfo);
            }
        }

	    /// <summary>
        /// Executes an Action that triggers reaction in all observables in the shared state.
        /// </summary>
        /// <param name="sharedState">The <see cref="ISharedState"/> instance to use.</param>
        /// <param name="actionName">The name of this action.</param>
        /// <param name="scope">The scope of this action.</param>
        /// <param name="action">The action itself.</param>
        /// <param name="arg1">Argument nr. 1.</param>
        /// <param name="arg2">Argument nr. 2.</param>
        /// <param name="arg3">Argument nr. 3.</param>
        /// <param name="arg4">Argument nr. 4.</param>
        internal static void ExecuteAction<T1,T2,T3,T4>(ISharedState sharedState, string actionName, object scope, Action<T1,T2,T3,T4> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            ActionRunInfo actionRunInfo = StartAction(sharedState, actionName, scope, new object[] {arg1, arg2, arg3, arg4});

            try
            {
                action.Invoke(arg1, arg2, arg3, arg4);
            }
            catch (Exception exception)
            {
                actionRunInfo.Exception = exception;
                throw;
            }
            finally
            {
                EndAction(actionRunInfo);
            }
        }

	    /// <summary>
        /// Executes an Action that triggers reaction in all observables in the shared state.
        /// </summary>
        /// <param name="sharedState">The <see cref="ISharedState"/> instance to use.</param>
        /// <param name="actionName">The name of this action.</param>
        /// <param name="scope">The scope of this action.</param>
        /// <param name="action">The action itself.</param>
        /// <param name="arg1">Argument nr. 1.</param>
        /// <param name="arg2">Argument nr. 2.</param>
        /// <param name="arg3">Argument nr. 3.</param>
        /// <param name="arg4">Argument nr. 4.</param>
        /// <param name="arg5">Argument nr. 5.</param>
        internal static void ExecuteAction<T1,T2,T3,T4,T5>(ISharedState sharedState, string actionName, object scope, Action<T1,T2,T3,T4,T5> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            ActionRunInfo actionRunInfo = StartAction(sharedState, actionName, scope, new object[] {arg1, arg2, arg3, arg4, arg5});

            try
            {
                action.Invoke(arg1, arg2, arg3, arg4, arg5);
            }
            catch (Exception exception)
            {
                actionRunInfo.Exception = exception;
                throw;
            }
            finally
            {
                EndAction(actionRunInfo);
            }
        }

	    /// <summary>
        /// Executes an Action that triggers reaction in all observables in the shared state.
        /// </summary>
        /// <param name="sharedState">The <see cref="ISharedState"/> instance to use.</param>
        /// <param name="actionName">The name of this action.</param>
        /// <param name="scope">The scope of this action.</param>
        /// <param name="action">The action itself.</param>
        /// <param name="arg1">Argument nr. 1.</param>
        /// <param name="arg2">Argument nr. 2.</param>
        /// <param name="arg3">Argument nr. 3.</param>
        /// <param name="arg4">Argument nr. 4.</param>
        /// <param name="arg5">Argument nr. 5.</param>
        /// <param name="arg6">Argument nr. 6.</param>
        internal static void ExecuteAction<T1,T2,T3,T4,T5,T6>(ISharedState sharedState, string actionName, object scope, Action<T1,T2,T3,T4,T5,T6> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            ActionRunInfo actionRunInfo = StartAction(sharedState, actionName, scope, new object[] {arg1, arg2, arg3, arg4, arg5, arg6});

            try
            {
                action.Invoke(arg1, arg2, arg3, arg4, arg5, arg6);
            }
            catch (Exception exception)
            {
                actionRunInfo.Exception = exception;
                throw;
            }
            finally
            {
                EndAction(actionRunInfo);
            }
        }

	    /// <summary>
        /// Executes an Action that triggers reaction in all observables in the shared state.
        /// </summary>
        /// <param name="sharedState">The <see cref="ISharedState"/> instance to use.</param>
        /// <param name="actionName">The name of this action.</param>
        /// <param name="scope">The scope of this action.</param>
        /// <param name="action">The action itself.</param>
        /// <param name="arg1">Argument nr. 1.</param>
        /// <param name="arg2">Argument nr. 2.</param>
        /// <param name="arg3">Argument nr. 3.</param>
        /// <param name="arg4">Argument nr. 4.</param>
        /// <param name="arg5">Argument nr. 5.</param>
        /// <param name="arg6">Argument nr. 6.</param>
        /// <param name="arg7">Argument nr. 7.</param>
        internal static void ExecuteAction<T1,T2,T3,T4,T5,T6,T7>(ISharedState sharedState, string actionName, object scope, Action<T1,T2,T3,T4,T5,T6,T7> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
        {
            ActionRunInfo actionRunInfo = StartAction(sharedState, actionName, scope, new object[] {arg1, arg2, arg3, arg4, arg5, arg6, arg7});

            try
            {
                action.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7);
            }
            catch (Exception exception)
            {
                actionRunInfo.Exception = exception;
                throw;
            }
            finally
            {
                EndAction(actionRunInfo);
            }
        }

	    /// <summary>
        /// Executes an Action that triggers reaction in all observables in the shared state.
        /// </summary>
        /// <param name="sharedState">The <see cref="ISharedState"/> instance to use.</param>
        /// <param name="actionName">The name of this action.</param>
        /// <param name="scope">The scope of this action.</param>
        /// <param name="action">The action itself.</param>
        /// <param name="arg1">Argument nr. 1.</param>
        /// <param name="arg2">Argument nr. 2.</param>
        /// <param name="arg3">Argument nr. 3.</param>
        /// <param name="arg4">Argument nr. 4.</param>
        /// <param name="arg5">Argument nr. 5.</param>
        /// <param name="arg6">Argument nr. 6.</param>
        /// <param name="arg7">Argument nr. 7.</param>
        /// <param name="arg8">Argument nr. 8.</param>
        internal static void ExecuteAction<T1,T2,T3,T4,T5,T6,T7,T8>(ISharedState sharedState, string actionName, object scope, Action<T1,T2,T3,T4,T5,T6,T7,T8> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
        {
            ActionRunInfo actionRunInfo = StartAction(sharedState, actionName, scope, new object[] {arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8});

            try
            {
                action.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
            }
            catch (Exception exception)
            {
                actionRunInfo.Exception = exception;
                throw;
            }
            finally
            {
                EndAction(actionRunInfo);
            }
        }

	    /// <summary>
        /// Executes an Action that triggers reaction in all observables in the shared state.
        /// </summary>
        /// <param name="sharedState">The <see cref="ISharedState"/> instance to use.</param>
        /// <param name="actionName">The name of this action.</param>
        /// <param name="scope">The scope of this action.</param>
        /// <param name="action">The action itself.</param>
        /// <param name="arg1">Argument nr. 1.</param>
        /// <param name="arg2">Argument nr. 2.</param>
        /// <param name="arg3">Argument nr. 3.</param>
        /// <param name="arg4">Argument nr. 4.</param>
        /// <param name="arg5">Argument nr. 5.</param>
        /// <param name="arg6">Argument nr. 6.</param>
        /// <param name="arg7">Argument nr. 7.</param>
        /// <param name="arg8">Argument nr. 8.</param>
        /// <param name="arg9">Argument nr. 9.</param>
        internal static void ExecuteAction<T1,T2,T3,T4,T5,T6,T7,T8,T9>(ISharedState sharedState, string actionName, object scope, Action<T1,T2,T3,T4,T5,T6,T7,T8,T9> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
        {
            ActionRunInfo actionRunInfo = StartAction(sharedState, actionName, scope, new object[] {arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9});

            try
            {
                action.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
            }
            catch (Exception exception)
            {
                actionRunInfo.Exception = exception;
                throw;
            }
            finally
            {
                EndAction(actionRunInfo);
            }
        }

	    /// <summary>
        /// Executes an Action that triggers reaction in all observables in the shared state.
        /// </summary>
        /// <param name="sharedState">The <see cref="ISharedState"/> instance to use.</param>
        /// <param name="actionName">The name of this action.</param>
        /// <param name="scope">The scope of this action.</param>
        /// <param name="action">The action itself.</param>
        /// <param name="arg1">Argument nr. 1.</param>
        /// <param name="arg2">Argument nr. 2.</param>
        /// <param name="arg3">Argument nr. 3.</param>
        /// <param name="arg4">Argument nr. 4.</param>
        /// <param name="arg5">Argument nr. 5.</param>
        /// <param name="arg6">Argument nr. 6.</param>
        /// <param name="arg7">Argument nr. 7.</param>
        /// <param name="arg8">Argument nr. 8.</param>
        /// <param name="arg9">Argument nr. 9.</param>
        /// <param name="arg10">Argument nr. 10.</param>
        internal static void ExecuteAction<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10>(ISharedState sharedState, string actionName, object scope, Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10)
        {
            ActionRunInfo actionRunInfo = StartAction(sharedState, actionName, scope, new object[] {arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10});

            try
            {
                action.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
            }
            catch (Exception exception)
            {
                actionRunInfo.Exception = exception;
                throw;
            }
            finally
            {
                EndAction(actionRunInfo);
            }
        }

	    /// <summary>
        /// Executes an Action that triggers reaction in all observables in the shared state.
        /// </summary>
        /// <param name="sharedState">The <see cref="ISharedState"/> instance to use.</param>
        /// <param name="actionName">The name of this action.</param>
        /// <param name="scope">The scope of this action.</param>
        /// <param name="action">The action itself.</param>
        /// <param name="arg1">Argument nr. 1.</param>
        /// <param name="arg2">Argument nr. 2.</param>
        /// <param name="arg3">Argument nr. 3.</param>
        /// <param name="arg4">Argument nr. 4.</param>
        /// <param name="arg5">Argument nr. 5.</param>
        /// <param name="arg6">Argument nr. 6.</param>
        /// <param name="arg7">Argument nr. 7.</param>
        /// <param name="arg8">Argument nr. 8.</param>
        /// <param name="arg9">Argument nr. 9.</param>
        /// <param name="arg10">Argument nr. 10.</param>
        /// <param name="arg11">Argument nr. 11.</param>
        internal static void ExecuteAction<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11>(ISharedState sharedState, string actionName, object scope, Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11)
        {
            ActionRunInfo actionRunInfo = StartAction(sharedState, actionName, scope, new object[] {arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11});

            try
            {
                action.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
            }
            catch (Exception exception)
            {
                actionRunInfo.Exception = exception;
                throw;
            }
            finally
            {
                EndAction(actionRunInfo);
            }
        }

	    /// <summary>
        /// Executes an Action that triggers reaction in all observables in the shared state.
        /// </summary>
        /// <param name="sharedState">The <see cref="ISharedState"/> instance to use.</param>
        /// <param name="actionName">The name of this action.</param>
        /// <param name="scope">The scope of this action.</param>
        /// <param name="action">The action itself.</param>
        /// <param name="arg1">Argument nr. 1.</param>
        /// <param name="arg2">Argument nr. 2.</param>
        /// <param name="arg3">Argument nr. 3.</param>
        /// <param name="arg4">Argument nr. 4.</param>
        /// <param name="arg5">Argument nr. 5.</param>
        /// <param name="arg6">Argument nr. 6.</param>
        /// <param name="arg7">Argument nr. 7.</param>
        /// <param name="arg8">Argument nr. 8.</param>
        /// <param name="arg9">Argument nr. 9.</param>
        /// <param name="arg10">Argument nr. 10.</param>
        /// <param name="arg11">Argument nr. 11.</param>
        /// <param name="arg12">Argument nr. 12.</param>
        internal static void ExecuteAction<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12>(ISharedState sharedState, string actionName, object scope, Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12)
        {
            ActionRunInfo actionRunInfo = StartAction(sharedState, actionName, scope, new object[] {arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12});

            try
            {
                action.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
            }
            catch (Exception exception)
            {
                actionRunInfo.Exception = exception;
                throw;
            }
            finally
            {
                EndAction(actionRunInfo);
            }
        }

	    /// <summary>
        /// Executes an Action that triggers reaction in all observables in the shared state.
        /// </summary>
        /// <param name="sharedState">The <see cref="ISharedState"/> instance to use.</param>
        /// <param name="actionName">The name of this action.</param>
        /// <param name="scope">The scope of this action.</param>
        /// <param name="action">The action itself.</param>
        /// <param name="arg1">Argument nr. 1.</param>
        /// <param name="arg2">Argument nr. 2.</param>
        /// <param name="arg3">Argument nr. 3.</param>
        /// <param name="arg4">Argument nr. 4.</param>
        /// <param name="arg5">Argument nr. 5.</param>
        /// <param name="arg6">Argument nr. 6.</param>
        /// <param name="arg7">Argument nr. 7.</param>
        /// <param name="arg8">Argument nr. 8.</param>
        /// <param name="arg9">Argument nr. 9.</param>
        /// <param name="arg10">Argument nr. 10.</param>
        /// <param name="arg11">Argument nr. 11.</param>
        /// <param name="arg12">Argument nr. 12.</param>
        /// <param name="arg13">Argument nr. 13.</param>
        internal static void ExecuteAction<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13>(ISharedState sharedState, string actionName, object scope, Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13)
        {
            ActionRunInfo actionRunInfo = StartAction(sharedState, actionName, scope, new object[] {arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13});

            try
            {
                action.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13);
            }
            catch (Exception exception)
            {
                actionRunInfo.Exception = exception;
                throw;
            }
            finally
            {
                EndAction(actionRunInfo);
            }
        }

	    /// <summary>
        /// Executes an Action that triggers reaction in all observables in the shared state.
        /// </summary>
        /// <param name="sharedState">The <see cref="ISharedState"/> instance to use.</param>
        /// <param name="actionName">The name of this action.</param>
        /// <param name="scope">The scope of this action.</param>
        /// <param name="action">The action itself.</param>
        /// <param name="arg1">Argument nr. 1.</param>
        /// <param name="arg2">Argument nr. 2.</param>
        /// <param name="arg3">Argument nr. 3.</param>
        /// <param name="arg4">Argument nr. 4.</param>
        /// <param name="arg5">Argument nr. 5.</param>
        /// <param name="arg6">Argument nr. 6.</param>
        /// <param name="arg7">Argument nr. 7.</param>
        /// <param name="arg8">Argument nr. 8.</param>
        /// <param name="arg9">Argument nr. 9.</param>
        /// <param name="arg10">Argument nr. 10.</param>
        /// <param name="arg11">Argument nr. 11.</param>
        /// <param name="arg12">Argument nr. 12.</param>
        /// <param name="arg13">Argument nr. 13.</param>
        /// <param name="arg14">Argument nr. 14.</param>
        internal static void ExecuteAction<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14>(ISharedState sharedState, string actionName, object scope, Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14)
        {
            ActionRunInfo actionRunInfo = StartAction(sharedState, actionName, scope, new object[] {arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14});

            try
            {
                action.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14);
            }
            catch (Exception exception)
            {
                actionRunInfo.Exception = exception;
                throw;
            }
            finally
            {
                EndAction(actionRunInfo);
            }
        }

	    /// <summary>
        /// Executes an Action that triggers reaction in all observables in the shared state.
        /// </summary>
        /// <param name="sharedState">The <see cref="ISharedState"/> instance to use.</param>
        /// <param name="actionName">The name of this action.</param>
        /// <param name="scope">The scope of this action.</param>
        /// <param name="action">The action itself.</param>
        /// <param name="arg1">Argument nr. 1.</param>
        /// <param name="arg2">Argument nr. 2.</param>
        /// <param name="arg3">Argument nr. 3.</param>
        /// <param name="arg4">Argument nr. 4.</param>
        /// <param name="arg5">Argument nr. 5.</param>
        /// <param name="arg6">Argument nr. 6.</param>
        /// <param name="arg7">Argument nr. 7.</param>
        /// <param name="arg8">Argument nr. 8.</param>
        /// <param name="arg9">Argument nr. 9.</param>
        /// <param name="arg10">Argument nr. 10.</param>
        /// <param name="arg11">Argument nr. 11.</param>
        /// <param name="arg12">Argument nr. 12.</param>
        /// <param name="arg13">Argument nr. 13.</param>
        /// <param name="arg14">Argument nr. 14.</param>
        /// <param name="arg15">Argument nr. 15.</param>
        internal static void ExecuteAction<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15>(ISharedState sharedState, string actionName, object scope, Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15)
        {
            ActionRunInfo actionRunInfo = StartAction(sharedState, actionName, scope, new object[] {arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15});

            try
            {
                action.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15);
            }
            catch (Exception exception)
            {
                actionRunInfo.Exception = exception;
                throw;
            }
            finally
            {
                EndAction(actionRunInfo);
            }
        }

	    /// <summary>
        /// Executes an Action that triggers reaction in all observables in the shared state.
        /// </summary>
        /// <param name="sharedState">The <see cref="ISharedState"/> instance to use.</param>
        /// <param name="actionName">The name of this action.</param>
        /// <param name="scope">The scope of this action.</param>
        /// <param name="action">The action itself.</param>
        /// <param name="arg1">Argument nr. 1.</param>
        /// <param name="arg2">Argument nr. 2.</param>
        /// <param name="arg3">Argument nr. 3.</param>
        /// <param name="arg4">Argument nr. 4.</param>
        /// <param name="arg5">Argument nr. 5.</param>
        /// <param name="arg6">Argument nr. 6.</param>
        /// <param name="arg7">Argument nr. 7.</param>
        /// <param name="arg8">Argument nr. 8.</param>
        /// <param name="arg9">Argument nr. 9.</param>
        /// <param name="arg10">Argument nr. 10.</param>
        /// <param name="arg11">Argument nr. 11.</param>
        /// <param name="arg12">Argument nr. 12.</param>
        /// <param name="arg13">Argument nr. 13.</param>
        /// <param name="arg14">Argument nr. 14.</param>
        /// <param name="arg15">Argument nr. 15.</param>
        /// <param name="arg16">Argument nr. 16.</param>
        internal static void ExecuteAction<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16>(ISharedState sharedState, string actionName, object scope, Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16)
        {
            ActionRunInfo actionRunInfo = StartAction(sharedState, actionName, scope, new object[] {arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16});

            try
            {
                action.Invoke(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16);
            }
            catch (Exception exception)
            {
                actionRunInfo.Exception = exception;
                throw;
            }
            finally
            {
                EndAction(actionRunInfo);
            }
        }

	}
}