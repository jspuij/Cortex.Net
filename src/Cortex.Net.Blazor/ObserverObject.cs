// <copyright file="ObserverObject.cs" company="Jan-Willem Spuij">
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

namespace Cortex.Net.Blazor
{
    using System;
    using Cortex.Net;
    using Cortex.Net.Core;
    using Microsoft.AspNetCore.Components.Rendering;

    /// <summary>
    /// Observer object that encapsulates a reaction to automatically track dependencies
    /// and rerender when any of the dependencies change.
    /// </summary>
    public sealed class ObserverObject : IDisposable
    {
        /// <summary>
        /// The shared state this observer is connected to.
        /// </summary>
        private readonly ISharedState sharedState;

        /// <summary>
        /// The name of the observer.
        /// </summary>
        private readonly string name;

        /// <summary>
        /// The action that should be executed to build the render tree.
        /// </summary>
        private readonly Action<RenderTreeBuilder> buildRenderTreeAction;

        /// <summary>
        /// The action that is used to force StateHasChanged for this Component.
        /// </summary>
        private readonly Action stateChangedAction;

        /// <summary>
        /// The current reaction. The reaction is automatically disposed when an exception occurs and recreated on the next rendering.
        /// </summary>
        private Reaction current;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObserverObject"/> class.
        /// </summary>
        /// <param name="sharedState">The shared state this observer is connected to.</param>
        /// <param name="name">The name of the observer.</param>
        /// <param name="buildRenderTreeAction">The action that should be executed to build the render tree.</param>
        /// <param name="stateChangedAction">The action that is used to force StateHasChanged for this Component.</param>
        public ObserverObject(ISharedState sharedState, string name, Action<RenderTreeBuilder> buildRenderTreeAction, Action stateChangedAction)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            this.sharedState = sharedState ?? throw new ArgumentNullException(nameof(sharedState));
            this.name = name;
            this.buildRenderTreeAction = buildRenderTreeAction ?? throw new ArgumentNullException(nameof(buildRenderTreeAction));
            this.stateChangedAction = stateChangedAction ?? throw new ArgumentNullException(nameof(stateChangedAction));
        }

        /// <summary>
        /// Gets or sets a property that automatically manages the current reaction.
        /// </summary>
        private Reaction Current
        {
            get
            {
                // auto create the current reaction if it does not exist.
                if (this.current == null)
                {
                    this.current = new Reaction(this.sharedState, $"Observer({this.name})", this.stateChangedAction, null);
                }

                return this.current;
            }

            set
            {
                // dispose any previous reaction automatically.
                if (this.current != null && !this.current.IsDisposed && this.current != value)
                {
                    this.current.Dispose();
                }

                this.current = value;
            }
        }

        /// <summary>
        /// Disposes the internal reaction that is used to track dependenies.
        /// </summary>
        public void Dispose()
        {
            // dispose any previous reaction automatically.
            if (this.current != null && !this.current.IsDisposed)
            {
                this.current.Dispose();
            }

            this.current = null;
        }

        /// <summary>
        /// Builds the render tree for the Component and automatically tracks dependencies.
        /// </summary>
        /// <param name="renderTreeBuilder">The render tree builder to use.</param>
        public void BuildRenderTree(RenderTreeBuilder renderTreeBuilder)
        {
            Exception exception = null;

            this.Current.Track(() =>
            {
                try
                {
                    this.buildRenderTreeAction(renderTreeBuilder);
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    exception = ex;
                }
            });

            if (exception != null)
            {
                this.Current = null;
                throw exception;
            }
        }
    }
}
