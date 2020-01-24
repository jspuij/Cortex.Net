// <copyright file="AirportsList.cs" company=".NET Foundation">
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

namespace FlightFinder.Client.Components
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using FlightFinder.Shared;
    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// A component that displays a list of airpoirts.
    /// </summary>
    public partial class AirportsList
    {
        /// <summary>
        /// An array of airports.
        /// </summary>
        private Airport[] airports = Array.Empty<Airport>();

        /// <summary>
        /// Gets or Sets The Http Client to use.
        /// </summary>
        [Inject]
        public HttpClient Http { get; set; }

        /// <summary>
        /// Initializes this component. Gets the list of airports from the api.
        /// </summary>
        /// <returns>The list of airports.</returns>
        protected override async Task OnInitializedAsync()
        {
            this.airports = await this.Http.GetJsonAsync<Airport[]>("api/airports");
        }
    }
}
