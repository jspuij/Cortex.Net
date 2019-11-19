// <copyright file="TodoStore.cs" company="Jan-Willem Spuij">
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

namespace Cortex.Net.BlazorTodo.Stores
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Cortex.Net.Api;
    using Cortex.Net.BlazorTodo.Models;

    /// <summary>
    /// Store of Todo items.
    /// </summary>
    [Observable]
    public class TodoStore
    {
        /// <summary>
        /// Gets the Todo items.
        /// </summary>
        public IList<Todo> Todos { get; private set; }

        /// <summary>
        /// Gets the number of active Todos.
        /// </summary>
        [Computed]
        public int ActiveCount => this.Todos.Count(x => !x.Completed);

        /// <summary>
        /// Gets the number of active Todos.
        /// </summary>
        [Computed]
        public int CompletedCount => this.Todos.Count - this.ActiveCount;

        /// <summary>
        /// Adds a todo item to the Store.
        /// </summary>
        /// <param name="title">The title of the new Todo item.</param>
        [Action]
        public void AddTodo(string title)
        {
            var newTodo = new Todo()
            {
                Id = Guid.NewGuid(),
                Title = title,
                Completed = false,
                Store = this,
            };
            this.Todos.Add(newTodo);
        }

        /// <summary>
        /// Toggles all items to the new completed state.
        /// </summary>
        /// <param name="completed">Whether the todo item is completed.</param>
        [Action]
        public void ToggleAll(bool completed)
        {
            foreach (var todo in this.Todos)
            {
                todo.Completed = completed;
            }
        }

        /// <summary>
        /// Clears the store of the completed items.
        /// </summary>
        [Action]
        public void ClearCompleted()
        {
            foreach (var completedItem in this.Todos.Where(x => x.Completed).ToList())
            {
                this.Todos.Remove(completedItem);
            }
        }
    }
}
