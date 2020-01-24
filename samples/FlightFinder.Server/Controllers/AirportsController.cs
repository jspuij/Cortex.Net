// <copyright file="AirportsController.cs" company=".NET Foundation">
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
    using System.Collections.Generic;
    using FlightFinder.Shared;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Web api controller for airports. Returns a sample set of Airports.
    /// </summary>
    [Route("api/[controller]")]
    public class AirportsController : Controller
    {
        /// <summary>
        /// Gets a list of airports from the Controller.
        /// </summary>
        /// <returns>A list of airports.</returns>
        public IEnumerable<Airport> Airports()
        {
            return SampleData.Airports;
        }
    }
}
