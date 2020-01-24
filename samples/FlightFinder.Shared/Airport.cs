﻿// <copyright file="Airport.cs" company=".NET Foundation">
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
    /// <summary>
    /// Defines an Airport.
    /// </summary>
    public class Airport
    {
        /// <summary>
        /// Gets or sets the IATA Code for the Airport.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets the display name for the Airport.
        /// </summary>
        public string DisplayName { get; set; }
    }
}
