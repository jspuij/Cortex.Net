// <copyright file="Neuron.cs" company="Jan-Willem Spuij">
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

namespace Cortex.Net
{
    using System;
    using System.Collections.Generic;
    using System.Reactive.Subjects;
    using System.Text;

    /// <summary>
    /// Smallest atomic observable for state of type T.
    /// </summary>
    /// <typeparam name="T">The type of the State to hold.</typeparam>
    public sealed class Neuron<T> : IObservable<T>
    {
        private readonly IObservable<T> observable;

        public Neuron()
        {

        }

        public Neuron(IObservable<T> observable)
        {
            this.observable = observable ?? throw new ArgumentNullException(nameof(observable));
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            return this.observable.Subscribe(observer);
        }
    }
}
