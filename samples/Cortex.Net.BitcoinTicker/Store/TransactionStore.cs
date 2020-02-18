// <copyright file="TransactionStore.cs" company="Jan-Willem Spuij">
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
    using System.Globalization;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using Cortex.Net.Api;
    using Cortex.Net.BitcoinTicker.Models;
    using Cortex.Net.BitcoinTicker.Services;
    using Cortex.Net.DynamicData;
    using Cortex.Net.Types;
    using global::DynamicData;

    /// <summary>
    /// An observable store for unconfirmed bitcoin.info transactions.
    /// </summary>
    public sealed class TransactionStore : IDisposable
    {
        private IDisposable subscription;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionStore"/> class.
        /// </summary>
        /// <param name="transactionService">The transaction service to use.</param>
        /// <param name="sharedState">The shared state to use.</param>
        public TransactionStore(TransactionService transactionService, ISharedState sharedState)
        {
            var set = transactionService.Transactions
               .Retry(3)
               .Catch((Exception ex) =>
               {
                   sharedState.RunInAction(() => this.ErrorMessage = $"Could not get unconfirmed transactions. The error message is: {ex.Message}");
                   return Observable.Empty<TransactionService.TransactionSet>();
               })
               .ToObservableChangeSet(10);

            this.subscription = set.TransformMany(set => set.Txs.Select(x => sharedState.Observable(() => new Transaction()
            {
               Hash = x.Hash,
               Time = DateTime.Now,
               Amount = x.Out.Select(o => o.Value / 100000000m).Sum(),
            })))
           .CortexBind((ObservableCollection<Transaction>)this.Transactions)
           .Subscribe();
        }

        /// <summary>
        /// Gets the exchange rates from the store.
        /// </summary>
        [Observable]
        public IList<Transaction> Transactions { get; private set; }

        /// <summary>
        /// Gets an error message indicating why the transactions could not be fetched.
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
    }
}
