// <copyright file="AppState.cs" company=".NET Foundation">
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
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using FlightFinder.Shared;
    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// A State bag for the Application.
    /// </summary>
    public class AppState
    {
        /// <summary>
        /// The HttpClient to do calls on.
        /// </summary>
        private readonly HttpClient httpClient;

        /// <summary>
        /// The short list with Itineraries.
        /// </summary>
        private readonly List<Itinerary> shortlist = new List<Itinerary>();

        /// <summary>
        /// Initializes a new instance of the <see cref="AppState"/> class.
        /// </summary>
        /// <param name="httpClient">The HttpClient to use to make requests.</param>
        public AppState(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        /// <summary>
        /// Lets components receive change notifications
        ///  Could have whatever granularity you want (more events, hierarchy...)
        /// </summary>
        public event Action OnChange;

        /// <summary>
        /// Gets the search results.
        /// </summary>
        public IReadOnlyList<Itinerary> SearchResults { get; private set; }

        /// <summary>
        /// Gets a value indicating whether search is in progress.
        /// </summary>
        public bool SearchInProgress { get; private set; }

        /// <summary>
        ///  Gets the short list with Itineraries.
        /// </summary>
        public IReadOnlyList<Itinerary> Shortlist => this.shortlist;

        /// <summary>
        /// Searches on the Web Api controller using the specified search criteria.
        /// </summary>
        /// <param name="criteria">The search criteria to use.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task Search(SearchCriteria criteria)
        {
            this.SearchInProgress = true;
            this.NotifyStateChanged();

            this.SearchResults = await this.httpClient.PostJsonAsync<Itinerary[]>("/api/flightsearch", criteria);
            this.SearchInProgress = false;
            this.NotifyStateChanged();
        }

        /// <summary>
        /// Add the itinerary to the short list.
        /// </summary>
        /// <param name="itinerary">The itinerary to add.</param>
        public void AddToShortlist(Itinerary itinerary)
        {
            this.shortlist.Add(itinerary);
            this.NotifyStateChanged();
        }

        /// <summary>
        /// Removes the itinerary from the short list.
        /// </summary>
        /// <param name="itinerary">The itinerary to remove.</param>
        public void RemoveFromShortlist(Itinerary itinerary)
        {
            this.shortlist.Remove(itinerary);
            this.NotifyStateChanged();
        }

        /// <summary>
        /// Notifies the Main Component of a state change.
        /// </summary>
        private void NotifyStateChanged() => this.OnChange?.Invoke();
    }
}
