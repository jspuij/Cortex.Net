// <copyright file="Itinerary.cs" company=".NET Foundation">
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

namespace FlightFinder.Shared
{
    using Cortex.Net.Api;

    /// <summary>
    /// Defines an Itinerary with an Outbound and Return Segment.
    /// </summary>
    [Observable]
    public class Itinerary
    {
        /// <summary>
        /// Gets or sets the outbound Flight.
        /// </summary>
        public FlightSegment Outbound { get; set; }

        /// <summary>
        /// Gets or sets the return Flight.
        /// </summary>
        public FlightSegment Return { get; set; }

        /// <summary>
        /// Gets or sets the price of the Flight.
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Gets the total duration in hours.
        /// </summary>
        [Computed]
        public double TotalDurationHours
            => this.Outbound.DurationHours + this.Return.DurationHours;

        /// <summary>
        /// Gets the airline name.
        /// </summary>
        [Computed]
        public string AirlineName
            => (this.Outbound.Airline == this.Return.Airline) ? this.Outbound.Airline : "Multiple airlines";
    }
}
