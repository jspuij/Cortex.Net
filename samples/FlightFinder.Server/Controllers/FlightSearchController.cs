// <copyright file="FlightSearchController.cs" company=".NET Foundation">
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

namespace FlightFinder.Server.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using FlightFinder.Shared;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Web api controller for flights.
    /// </summary>
    [Route("api/[controller]")]
    public class FlightSearchController
    {
        /// <summary>
        /// Emulates a search for a set of flights using the specified Search criteria.
        /// </summary>
        /// <param name="criteria">The search criteria.</param>
        /// <returns>An Itinerary with a random set of flights.</returns>
        public async Task<IEnumerable<Itinerary>> Search([FromBody] SearchCriteria criteria)
        {
            await Task.Delay(500); // Gotta look busy...

            var rng = new Random();
            return Enumerable.Range(0, rng.Next(1, 5)).Select(_ => new Itinerary
            {
                Price = rng.Next(100, 2000),
                Outbound = new FlightSegment
                {
                    Airline = RandomAirline(),
                    FromAirportCode = criteria.FromAirport,
                    ToAirportCode = criteria.ToAirport,
                    DepartureTime = criteria.OutboundDate.AddHours(rng.Next(24)).AddMinutes(5 * rng.Next(12)),
                    ReturnTime = criteria.OutboundDate.AddHours(rng.Next(24)).AddMinutes(5 * rng.Next(12)),
                    DurationHours = 2 + rng.Next(10),
                    TicketClass = criteria.TicketClass,
                },
                Return = new FlightSegment
                {
                    Airline = RandomAirline(),
                    FromAirportCode = criteria.ToAirport,
                    ToAirportCode = criteria.FromAirport,
                    DepartureTime = criteria.ReturnDate.AddHours(rng.Next(24)).AddMinutes(5 * rng.Next(12)),
                    ReturnTime = criteria.ReturnDate.AddHours(rng.Next(24)).AddMinutes(5 * rng.Next(12)),
                    DurationHours = 2 + rng.Next(10),
                    TicketClass = criteria.TicketClass,
                },
            });
        }

        /// <summary>
        /// Gets a random airline from the set of sample data.
        /// </summary>
        /// <returns>The name of a random Airline.</returns>
        private static string RandomAirline()
            => SampleData.Airlines[new Random().Next(SampleData.Airlines.Length)];
    }
}
