// <copyright file="SearchResults.cs" company=".NET Foundation">
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
    using System.Collections.Generic;
    using System.Linq;
    using FlightFinder.Shared;
    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// A component that displays a search result.
    /// </summary>
    public partial class SearchResults
    {
        /// <summary>
        /// The sort order enumeration.
        /// </summary>
        private enum SortOrder
        {
            /// <summary>
            /// Sort the Itineraries on price.
            /// </summary>
            Price,

            /// <summary>
            /// Sort the Itineraries on duration.
            /// </summary>
            Duration,
        }

        /// <summary>
        /// Gets or sets the Itinerary.
        /// </summary>
        // Parameters
        [Parameter]
        public IReadOnlyList<Itinerary> Itineraries { get; set; }

        /// <summary>
        /// Gets or sets a callback that is called when the Itinerary is added.
        /// </summary>
        [Parameter]
        public EventCallback<Itinerary> OnAddItinerary { get; set; }

        /// <summary>
        /// Gets or sets the sport order of the Search Results.
        /// </summary>
        private SortOrder ChosenSortOder { get; set; }

        /// <summary>
        /// Gets the list of sorted Itineraries.
        /// </summary>
        private IEnumerable<Itinerary> SortedItineraries
            => this.ChosenSortOder == SortOrder.Price
            ? this.Itineraries.OrderBy(x => x.Price)
            : this.Itineraries.OrderBy(x => x.TotalDurationHours);
    }
}
