// <copyright file="ShortListState.cs" company=".NET Foundation">
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
    using Cortex.Net.Api;
    using FlightFinder.Shared;

    /// <summary>
    /// A State bag for the Short list..
    /// </summary>
    [Observable]
    public class ShortListState
    {
        /// <summary>
        ///  Gets the short list with Itineraries.
        /// </summary>
        public ICollection<Itinerary> Shortlist { get; private set; }

        /// <summary>
        /// Add the itinerary to the short list.
        /// </summary>
        /// <param name="itinerary">The itinerary to add.</param>
        [Action]
        public void AddToShortlist(Itinerary itinerary)
        {
            this.Shortlist.Add(itinerary);
        }

        /// <summary>
        /// Removes the itinerary from the short list.
        /// </summary>
        /// <param name="itinerary">The itinerary to remove.</param>
        [Action]
        public void RemoveFromShortlist(Itinerary itinerary)
        {
            this.Shortlist.Remove(itinerary);
        }
    }
}
