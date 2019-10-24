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
    using Cortex.Net.Properties;
    using Cortex.Net.Spy;

    /// <summary>
    /// Extension methods for <see cref="Action"/> delegates.
    /// </summary>
    public static partial class ActionExtensions
    {
        /// <summary>
        /// Starts an action that changes state and recomputes the state tree.
        /// </summary>
        /// <param name="sharedState">The shared state instance to use.</param>
        /// <param name="actionName">The name of the action.</param>
        /// <param name="scope">The scope of the action.</param>
        /// <param name="arguments">The arguments to the action.</param>
        /// <returns>An <see cref="ActionRunInfo"/> instance containing the information on the currently running action.</returns>
        private static ActionRunInfo StartAction(ISharedState sharedState, string actionName, object scope, object[] arguments)
        {
            var notifySpy = !string.IsNullOrEmpty(actionName);
            var previousDerivation = sharedState.StartUntracked();
            sharedState.StartBatch();
            var previousAllowStateChanges = sharedState.StartAllowStateChanges(true);
            var previousAllowStateReads = sharedState.StartAllowStateReads(true);

            var actionRunInfo = new ActionRunInfo()
            {
                SharedState = sharedState,
                Name = string.IsNullOrEmpty(actionName) ? $"Action@{sharedState.NextActionId}" : actionName,
                PreviousDerivation = previousDerivation,
                PreviousAllowStateChanges = previousAllowStateChanges,
                PreviousAllowStateReads = previousAllowStateReads,
                NotifySpy = notifySpy,
                StartDateTime = DateTime.UtcNow,
                ActionId = sharedState.NextActionId++,
                ParentActionId = sharedState.CurrentActionId,
            };

            if (notifySpy)
            {
                sharedState.OnSpy(actionRunInfo, new ActionStartSpyEventArgs()
                {
                    Name = actionRunInfo.Name,
                    ActionId = actionRunInfo.ActionId,
                    Context = scope,
                    Arguments = arguments,
                    StartTime = actionRunInfo.StartDateTime,
                });
            }

            sharedState.CurrentActionId = actionRunInfo.ActionId;
            return actionRunInfo;
        }

        /// <summary>
        /// Ends an action using the specified <see cref="ActionRunInfo"/> instance.
        /// </summary>
        /// <param name="actionRunInfo">The run info about the action.</param>
        private static void EndAction(ActionRunInfo actionRunInfo)
        {
            if (actionRunInfo.SharedState.CurrentActionId != actionRunInfo.ActionId)
            {
                throw new InvalidOperationException(Resources.InvalidActionStack);
            }

            actionRunInfo.SharedState.CurrentActionId = actionRunInfo.ParentActionId;

            if (actionRunInfo.Exception != null)
            {
                actionRunInfo.SharedState.SuppressReactionErrors = true;
            }

            actionRunInfo.SharedState.EndAllowStateChanges(actionRunInfo.PreviousAllowStateChanges);
            actionRunInfo.SharedState.EndAllowStateReads(actionRunInfo.PreviousAllowStateReads);
            actionRunInfo.SharedState.EndBatch();
            actionRunInfo.SharedState.EndTracking(actionRunInfo.PreviousDerivation);

            if (actionRunInfo.NotifySpy)
            {
                actionRunInfo.SharedState.OnSpy(actionRunInfo, new ActionEndSpyEventArgs()
                {
                    ActionId = actionRunInfo.ActionId,
                    Name = actionRunInfo.Name,
                    EndTime = DateTime.UtcNow,
                });
            }

            actionRunInfo.SharedState.SuppressReactionErrors = false;
}
    }
}
