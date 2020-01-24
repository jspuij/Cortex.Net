// <copyright file="SearchState.cs" company=".NET Foundation">
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

namespace FlightFinder.Client.Services
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Cortex.Net.Api;
    using FlightFinder.Shared;
    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// State service for searches.
    /// </summary>
    [Observable]
    public class SearchState
    {
        /// <summary>
        /// The HttpClient to do calls on.
        /// </summary>
        private readonly HttpClient httpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchState"/> class.
        /// </summary>
        /// <param name="httpClient">The HttpClient to use to make requests.</param>
        public SearchState(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        /// <summary>
        /// Gets the search results.
        /// </summary>
        public IReadOnlyList<Itinerary> SearchResults { get; private set; }

        /// <summary>
        /// Gets a value indicating whether search is in progress.
        /// </summary>
        public bool SearchInProgress { get; private set; }

        /// <summary>
        /// Searches on the Web Api controller using the specified search criteria.
        /// </summary>
        /// <param name="criteria">The search criteria to use.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <remarks>
        /// This method is divided into two actions by the compiler.
        /// The first runs to the await statement and rerenders then.
        /// The second part runs after the await statement and rerenders after the method has completed.
        /// </remarks>
        [Action]
        public async Task Search(SearchCriteria criteria)
        {
            this.SearchInProgress = true;
            var searchResults = await this.httpClient.PostJsonAsync<Itinerary[]>("/api/flightsearch", criteria);
            this.SearchResults.Refill(searchResults);
            this.SearchInProgress = false;
        }
    }
}
