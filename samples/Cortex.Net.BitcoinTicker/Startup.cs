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

namespace Cortex.Net.BitcoinTicker
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Cortex.Net.Api;

    using Cortex.Net.BitcoinTicker.Services;
    using Cortex.Net.BitcoinTicker.Store;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.HttpsPolicy;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    /// <summary>
    /// Startup class. Configures Services and the application itself.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">The configuration that is passed to the startup class.</param>
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        /// <summary>
        /// Gets the configuration for the web application.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Configures the services by adding them to the container.
        /// </summary>
        /// <param name="services">The collection of services to add to.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient("blockchaininfo", c =>
            {
                c.BaseAddress = new Uri("https://blockchain.info/");
            });

            services.AddRazorPages();
            services.AddServerSideBlazor();

            // singleton. One TicketService for the entire application.
            services.AddSingleton<TickerService>();
            services.AddSingleton<TransactionService>();

            services.AddScoped<ISharedState>(s =>
            {
                var configuration = new CortexConfiguration()
                {
                    // autoschedule actions on the render thread.
                    AutoscheduleActions = true,
                    EnforceActions = EnforceAction.Never,
                    TaskScheduler = TaskScheduler.FromCurrentSynchronizationContext(),
                };

                return new SharedState(configuration);
            });

            // Store. Specific to the circuit.
            services.AddScoped<TickerStore>(s =>
            {
                var sharedState = s.GetRequiredService<ISharedState>();
                return sharedState.Observable(() => new TickerStore(s.GetRequiredService<TickerService>(), sharedState));
            });
            services.AddScoped<TransactionStore>(s =>
            {
                var sharedState = s.GetRequiredService<ISharedState>();
                return sharedState.Observable(() => new TransactionStore(s.GetRequiredService<TransactionService>(), sharedState));
            });
        }

        /// <summary>
        /// Configures the application and the Http Request Pipeline.
        /// </summary>
        /// <param name="app">The applicationbuilder instance.</param>
        /// <param name="env">The webhost environment.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");

                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
