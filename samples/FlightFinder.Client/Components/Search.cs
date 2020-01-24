// <copyright file="Search.cs" company=".NET Foundation">
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
    using FlightFinder.Shared;
    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// Search Component.
    /// </summary>
    public partial class Search
    {
        /// <summary>
        /// The search criteria that will be returned.
        /// </summary>
        private readonly SearchCriteria criteria = new SearchCriteria("LHR", "SEA");

        /// <summary>
        /// Gets or sets the callback that will provide the search criteria for a search.
        /// </summary>
        [Parameter]
        public EventCallback<SearchCriteria> OnSearch { get; set; }
    }
}
