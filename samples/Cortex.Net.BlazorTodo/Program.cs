// <copyright file="Program.cs" company="Jan-Willem Spuij">
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

namespace Cortex.Net.BlazorTodo
{
    using System.Threading.Tasks;
    using Blazored.LocalStorage;
    using Cortex.Net.BlazorTodo.Stores;
    using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Entry point for the Web Assembly application.
    /// </summary>
    public sealed class Program
    {
        /// <summary>
        /// Main entry point for the Web assembly application.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
#pragma warning disable CA1801 // parameter args never used.
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.Services.AddBaseAddressHttpClient();

            // Blazor is single threaded for now but does not provide a Task Scheduler when FromCurrentSynchronizationContext();
            // is called. However TaskScheduler.Current is available and at least is able to Schedule tasks.
            SharedState.GlobalState.Configuration.TaskScheduler = TaskScheduler.Current;

            // add local storage support.
            builder.Services.AddBlazoredLocalStorage();

            // Add the Shared state to the DI container.
            builder.Services.AddSingleton(x => SharedState.GlobalState);

            // Add a singleton ViewStore to the DI container.
            builder.Services.AddSingleton<ViewStore>();

            // Add a singleton TodoStore to the DI container.
            builder.Services.AddSingleton<TodoStore>();

            builder.RootComponents.Add<App>("app");

            await builder.Build().RunAsync().ConfigureAwait(true);
        }
#pragma warning restore CA1801 // parameter args never used.
    }
}
