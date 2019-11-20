// <copyright file="Startup.cs" company="Jan-Willem Spuij">
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
    using Blazored.LocalStorage;
    using Cortex.Net.Api;
    using Cortex.Net.BlazorTodo.Stores;
    using Microsoft.AspNetCore.Components.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using System.Threading.Tasks;

    /// <summary>
    /// Startup class that configures services.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Configures services for dependency injection.
        /// </summary>
        /// <param name="services">A collection of services.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Startup convention.")]
        public void ConfigureServices(IServiceCollection services)
        {
            // Blazor is single threaded for now but does not provide a Task Scheduler when FromCurrentSynchronizationContext();
            // is called. However TaskScheduler.Current is available and at least is able to Schedule tasks.
            SharedState.GlobalState.Configuration.TaskScheduler = TaskScheduler.Current;

            // add local storage support.
            services.AddBlazoredLocalStorage();

            // Add the Shared state to the DI container.
            services.AddSingleton(x => SharedState.GlobalState);

            // Add a singleton ViewStore to the DI container.
            services.AddSingleton<ViewStore>();

            // Add a singleton TodoStore to the DI container.
            services.AddSingleton<TodoStore>();
        }

        /// <summary>
        /// Configures the application and defines the root component.
        /// </summary>
        /// <param name="app">The application builder to use.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Startup convention.")]
        public void Configure(IComponentsApplicationBuilder app)
        {
            app.AddComponent<App>("app");
        }
    }
}
