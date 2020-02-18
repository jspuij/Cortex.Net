// <copyright file="TickerService.cs" company="Jan-Willem Spuij">
// Copyright 2019 Jan-Willem Spuij
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy,
// modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom
// the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
// ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>

namespace Cortex.Net.BitcoinTicker.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;
    using Cortex.Net.BitcoinTicker.Models;
    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// An observable service that provides access to a Bitcoin ticker.
    /// </summary>
    public class TickerService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TickerService"/> class.
        /// </summary>
        /// <param name="httpClientFactory">The Http Client factory to use.</param>
        public TickerService(IHttpClientFactory httpClientFactory)
        {
            this.Ticker = Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(15))
                .SelectMany(_ => Observable.FromAsync(async () =>
                    await httpClientFactory.CreateClient("blockchaininfo").GetJsonAsync<IDictionary<string, ExchangeRate>>("/ticker")));
        }

        /// <summary>
        /// Gets the ticker observable.
        /// </summary>
        public IObservable<IDictionary<string, ExchangeRate>> Ticker { get; }

        /// <summary>
        /// A model for exchange rates.
        /// </summary>
        public class ExchangeRate
        {
            /// <summary>
            /// Gets or sets fifteen minute delayed value.
            /// </summary>
            [JsonPropertyName("15m")]
            public decimal FifteenMinutes { get; set; }

            /// <summary>
            /// Gets or sets last value.
            /// </summary>
            public decimal Last { get; set; }

            /// <summary>
            /// Gets or sets buy value.
            /// </summary>
            public decimal Buy { get; set; }

            /// <summary>
            /// Gets or sets sell value.
            /// </summary>
            public decimal Sell { get; set; }

            /// <summary>
            /// Gets or sets symbol value.
            /// </summary>
            public string Symbol { get; set; }
        }
    }
}
