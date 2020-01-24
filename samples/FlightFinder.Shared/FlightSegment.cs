// <copyright file="FlightSegment.cs" company=".NET Foundation">
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
    using System;

    /// <summary>
    /// Defines a Flight Segment Between two Airports.
    /// </summary>
    public class FlightSegment
    {
        /// <summary>
        /// Gets or sets the Airline.
        /// </summary>
        public string Airline { get; set; }

        /// <summary>
        /// Gets or sets the IATA code for the departure Airport.
        /// </summary>
        public string FromAirportCode { get; set; }

        /// <summary>
        /// Gets or sets the IATA code for the arrival Airport.
        /// </summary>
        public string ToAirportCode { get; set; }

        /// <summary>
        /// Gets or sets the departure time.
        /// </summary>
        public DateTime DepartureTime { get; set; }

        /// <summary>
        /// Gets or sets the return time.
        /// </summary>
        public DateTime ReturnTime { get; set; }

        /// <summary>
        /// Gets or sets the duration in hours.
        /// </summary>
        public double DurationHours { get; set; }

        /// <summary>
        /// Gets or sets the ticket class.
        /// </summary>
        public TicketClass TicketClass { get; set; }
    }
}
