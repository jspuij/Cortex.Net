// <copyright file="TickerStore.cs" company="Jan-Willem Spuij">
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

namespace Cortex.Net.BitcoinTicker.Store
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using Cortex.Net.Api;
    using Cortex.Net.BitcoinTicker.Models;
    using Cortex.Net.BitcoinTicker.Services;
    using Cortex.Net.DynamicData;
    using Cortex.Net.Types;
    using global::DynamicData;

    /// <summary>
    /// An observable store of ticker items.
    /// </summary>
    public sealed class TickerStore : IDisposable
    {
        private IDisposable subscription;

        /// <summary>
        /// Initializes a new instance of the <see cref="TickerStore"/> class.
        /// </summary>
        /// <param name="tickerService">The ticker service to use.</param>
        /// <param name="sharedState">The shared state.</param>
        public TickerStore(TickerService tickerService, ISharedState sharedState)
        {
            var set = tickerService.Ticker
                .Retry(3)
                .Catch((Exception ex) =>
                {
                    sharedState.RunInAction(() => this.ErrorMessage = $"Could not get exchange rates. The error message is: {ex.Message}");
                    return Observable.Empty<IDictionary<string, TickerService.ExchangeRate>>();
                })
                .ToObservableChangeSet(
                x => x.Key, expireAfter: item => TimeSpan.FromHours(4));

            this.subscription = set.Transform(kvp => sharedState.Observable(() => new ExchangeRate()
            {
                Name = kvp.Key,
                Symbol = kvp.Value.Symbol,
                Value = kvp.Value.Last,
                Max = kvp.Value.Last,
                Min = kvp.Value.Last,
            }))
            .Select(x => CalculateMinMax(x))
            .CortexBind((ObservableDictionary<string, ExchangeRate>)this.ExchangeRates)
            .Subscribe();
        }

        /// <summary>
        /// Gets the exchange rates from the store.
        /// </summary>
        [Observable]
        public IDictionary<string, ExchangeRate> ExchangeRates { get; private set; }

        /// <summary>
        /// Gets an error message indicating why the exchange rate could not be fetched.
        /// </summary>
        [Observable]
        public string ErrorMessage { get; private set; }

        /// <summary>
        /// Disposes of managed and unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (this.subscription != null)
            {
                this.subscription.Dispose();
                this.subscription = null;
            }
        }

        /// <summary>
        /// Calculates the min and max based on the changeset.
        /// </summary>
        /// <param name="changeSet">The changeset to process.</param>
        /// <returns>The changeset.</returns>
        private static IChangeSet<ExchangeRate, string> CalculateMinMax(IChangeSet<ExchangeRate, string> changeSet)
        {
            foreach (var change in changeSet)
            {
                if (change.Reason == ChangeReason.Update)
                {
                    change.Current.Min = Math.Min(change.Current.Value, change.Previous.Value.Min);
                    change.Current.Max = Math.Max(change.Current.Value, change.Previous.Value.Max);
                }
            }

            return changeSet;
        }
    }
}
