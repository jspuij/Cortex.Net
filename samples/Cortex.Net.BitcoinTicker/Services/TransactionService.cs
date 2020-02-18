// <copyright file="TransactionService.cs" company="Jan-Willem Spuij">
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
    using System.Linq;
    using System.Net.Http;
    using System.Reactive.Linq;
    using Microsoft.AspNetCore.Components;

    /// <summary>
    /// An observable service that provides access to Bitcoin transactions.
    /// </summary>
    public class TransactionService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionService"/> class.
        /// </summary>
        /// <param name="httpClientFactory">The Http Client factory to use.</param>
        public TransactionService(IHttpClientFactory httpClientFactory)
        {
            this.Transactions = Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(15))
                .SelectMany(_ => Observable.FromAsync(async () =>
                    await httpClientFactory.CreateClient("blockchaininfo").GetJsonAsync<TransactionSet>("/unconfirmed-transactions?format=json")));
        }

        /// <summary>
        /// Gets the transactions observable.
        /// </summary>
        public IObservable<TransactionSet> Transactions { get; }

        /// <summary>
        /// A transaction set.
        /// </summary>
        public class TransactionSet
        {
            /// <summary>
            /// Gets or sets an array of transactions.
            /// </summary>
            public Transaction[] Txs { get; set; }
        }

        /// <summary>
        /// A transaction.
        /// </summary>
        public class Transaction
        {
            /// <summary>
            /// Gets or sets the Hash.
            /// </summary>
            public string Hash { get; set; }

            /// <summary>
            /// Gets or sets an array of outputs.
            /// </summary>
            public Output[] Out { get; set; }
        }

        /// <summary>
        /// Represents a transaction output.
        /// </summary>
        public class Output
        {
            /// <summary>
            /// Gets or sets the value of the transaction.
            /// </summary>
            public decimal Value { get; set; }
        }
    }
}
