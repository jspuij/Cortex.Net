// <copyright file="TicketClassExtensions.cs" company=".NET Foundation">
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
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Extension methods for the <see cref="TicketClass" /> enumeration.
    /// </summary>
    public static class TicketClassExtensions
    {
        /// <summary>
        /// Displays the Ticket class enumeration.
        /// </summary>
        /// <param name="ticketClass">The ticket class.</param>
        /// <returns>A string with the textual representation of the ticket class.</returns>
        /// <exception cref="ArgumentException">Thrown when an unknown ticketclass is encountered.</exception>
        public static string ToDisplayString(this TicketClass ticketClass)
        {
            return ticketClass switch
            {
                TicketClass.Economy => "Economy",
                TicketClass.PremiumEconomy => "Premium Economy",
                TicketClass.Business => "Business",
                TicketClass.First => "First",
                _ => throw new ArgumentException($"Unknown ticket class: {ticketClass}"),
            };
        }
    }
}
