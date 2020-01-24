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
    using Microsoft.AspNetCore.Blazor.Hosting;

    /// <summary>
    /// The program class. Bootstraps the WASM application.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main entry point for the WASM application.
        /// </summary>
        /// <param name="args">The arguments for the application.</param>
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        /// <summary>
        /// Creates a Host builder for the application using the <see cref="Startup" /> class.
        /// </summary>
        /// <param name="args">The arguments for the application.</param>
        /// <returns>A webhost builder.</returns>
        public static IWebAssemblyHostBuilder CreateHostBuilder(string[] args) =>
            BlazorWebAssemblyHost.CreateDefaultBuilder()
                .UseBlazorStartup<Startup>();
    }
}
