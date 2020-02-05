// <copyright file="Program.cs" company=".NET Foundation">
// Copyright (c) .NET Foundation. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use
// these files except in compliance with the License. You may obtain a copy of the
// License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, either express or implied. See the License for the
// specific language governing permissions and limitations under the License.
// </copyright>

namespace FlightFinder.Client
{
    using System.Threading.Tasks;
    using Cortex.Net;
    using FlightFinder.Client.Services;
    using Microsoft.AspNetCore.Blazor.Hosting;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// The program class. Bootstraps the WASM application.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main entry point for the WASM application.
        /// </summary>
        /// <param name="args">The arguments for the application.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            // Blazor is single threaded for now but does not provide a Task Scheduler when FromCurrentSynchronizationContext();
            // is called. However TaskScheduler.Current is available and at least is able to Schedule tasks.
            SharedState.GlobalState.Configuration.TaskScheduler = TaskScheduler.Current;

            // Do not enforce actions. This is to allow observable properties to be bound using normal bind directives.
            SharedState.GlobalState.Configuration.EnforceActions = EnforceAction.Never;

            // Add the Shared state to the DI container.
            builder.Services.AddSingleton(x => SharedState.GlobalState);

            // Add application state to the DI container.
            builder.Services.AddSingleton<ShortListState>();
            builder.Services.AddSingleton<SearchState>();

            builder.RootComponents.Add<Main>("body");

            await builder.Build().RunAsync();
        }
    }
}
