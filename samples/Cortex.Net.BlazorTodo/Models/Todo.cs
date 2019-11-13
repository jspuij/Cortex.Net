// <copyright file="Todo.cs" company="Jan-Willem Spuij">
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

namespace Cortex.Net.BlazorTodo.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Cortex.Net.Api;
    using Cortex.Net.BlazorTodo.Stores;

    /// <summary>
    /// Represents a model of a Todo.
    /// </summary>
    public class Todo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Todo"/> class.
        /// </summary>
        /// <param name="store">The store this todo is connected to.</param>
        /// <param name="id">The Id of the Todo item.</param>
        public Todo(TodoStore store, Guid id)
        {
            this.Store = store;
            this.Id = id;
        }

        /// <summary>
        /// Gets the store.
        /// </summary>
        public TodoStore Store { get; private set; }

        /// <summary>
        /// Gets the Id.
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets or sets the Title.
        /// </summary>
        [Observable]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Todo item is completed.
        /// </summary>
        [Observable]
        public bool Completed { get; set; }

        /// <summary>
        /// Toggles this item for completion.
        /// </summary>
        [Action]
        public void Toggle()
        {
            this.Completed = !this.Completed;
        }

        /// <summary>
        /// Destroys this todo by removing it from the Store.
        /// </summary>
        public void Destroy()
        {
            this.Store.Todos.Remove(this);
        }
    }
}
