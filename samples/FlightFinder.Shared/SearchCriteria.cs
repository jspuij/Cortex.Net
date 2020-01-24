// <copyright file="SearchCriteria.cs" company=".NET Foundation">
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
    /// The search criteria that the user put in.
    /// </summary>
    public class SearchCriteria
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SearchCriteria"/> class.
        /// </summary>
        public SearchCriteria()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchCriteria"/> class.
        /// </summary>
        /// <param name="fromAirport">The airport to depart from.</param>
        /// <param name="toAirport">The airport to go to.</param>
        public SearchCriteria(string fromAirport, string toAirport)
            : this()
        {
            this.FromAirport = fromAirport;
            this.ToAirport = toAirport;
            this.OutboundDate = DateTime.Now.Date;
            this.ReturnDate = this.OutboundDate.AddDays(7);
        }

        /// <summary>
        /// Gets or sets the airport to depart from.
        /// </summary>
        public string FromAirport { get; set; }

        /// <summary>
        /// Gets or sets the airport to go to.
        /// </summary>
        public string ToAirport { get; set; }

        /// <summary>
        /// Gets or sets the outbound travel date.
        /// </summary>
        public DateTime OutboundDate { get; set; }

        /// <summary>
        /// Gets or sets the return travel date.
        /// </summary>
        public DateTime ReturnDate { get; set; }

        /// <summary>
        /// Gets or sets the ticket class.
        /// </summary>
        public TicketClass TicketClass { get; set; }
    }
}
