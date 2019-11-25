// <copyright file="ActionTests.cs" company="Michel Weststrate, Jan-Willem Spuij">
// Copyright 2019 Michel Weststrate, Jan-Willem Spuij
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

namespace Cortex.Net.Test.Base
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Cortex.Net.Api;
    using Cortex.Net.Core;
    using Cortex.Net.Spy;
    using Cortex.Net.Types;
    using Xunit;

    /// <summary>
    /// Unit tests for Actions.
    /// </summary>
    public class ActionTests
    {
        private readonly ISharedState sharedState;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionTests"/> class.
        /// </summary>
        public ActionTests()
        {
            this.sharedState = new SharedState(new CortexConfiguration()
            {
                EnforceActions = EnforceAction.Never,
            });
        }

        /// <summary>
        /// Action sould wrap in an transaction.
        /// </summary>
        [Fact]
        public void ActionShouldWrapInTransaction()
        {
            var values = new List<int>();

            var observable = new ObservableValue<int>(this.sharedState, "int", this.sharedState.ReferenceEnhancer(), 0);

            this.sharedState.Autorun((r) => values.Add(observable.Value));

            var increment = this.sharedState.CreateAction<int>("increment", (amount) =>
            {
                observable.Value = observable.Value + (amount * 2);
                observable.Value = observable.Value - amount; // oops
            });

            increment(7);

            Assert.Equal(new[] { 0, 7 }, values);
        }

        /// <summary>
        /// Action modifications should be picked up 1.
        /// </summary>
        [Fact]
        public void ActionModificationsShouldBePickedUp1()
        {
            var a = new ObservableValue<int>(this.sharedState, "int", this.sharedState.ReferenceEnhancer(), 1);

            var i = 3;
            var b = 0;

            this.sharedState.Autorun((r) =>
            {
                b = a.Value * 2;
            });

            Assert.Equal(2, b);

            var action = this.sharedState.CreateAction(() =>
            {
                a.Value = ++i;
            });

            action();

            Assert.Equal(8, b);

            action();

            Assert.Equal(10, b);
        }

        /// <summary>
        /// Action modifications should be picked up 2.
        /// </summary>
        [Fact]
        public void ActionModificationsShouldBePickedUp2()
        {
            var a = new ObservableValue<int>(this.sharedState, "int", this.sharedState.ReferenceEnhancer(), 1);

            var b = 0;

            this.sharedState.Autorun((r) =>
            {
                b = a.Value * 2;
            });

            Assert.Equal(2, b);

            var action = this.sharedState.CreateAction(() =>
            {
                a.Value += 1; // ha, no loop!
            });

            action();

            Assert.Equal(4, b);

            action();

            Assert.Equal(6, b);
        }

        /// <summary>
        /// Action modifications should be picked up 3.
        /// </summary>
        [Fact]
        public void ActionModificationsShouldBePickedUp3()
        {
            var a = new ObservableValue<int>(this.sharedState, "int", this.sharedState.ReferenceEnhancer(), 1);

            var b = 0;

            var doubler = new ComputedValue<int>(this.sharedState, new ComputedValueOptions<int>(
            () => a.Value * 2, "doubler"));

            doubler.Observe(
                (s, e) =>
            {
                b = doubler.Value;
            }, true);

            var action = this.sharedState.CreateAction(() =>
            {
                a.Value += 1; // ha, no loop!
            });

            action();

            Assert.Equal(4, b);

            action();

            Assert.Equal(6, b);
        }

        /// <summary>
        /// Action should be untracked.
        /// </summary>
        [Fact]
        public void ActionShouldBeUntracked()
        {
            var a = new ObservableValue<int>(this.sharedState, "int", this.sharedState.ReferenceEnhancer(), 3);
            var b = new ObservableValue<int>(this.sharedState, "int", this.sharedState.ReferenceEnhancer(), 4);

            var latest = 0;
            var runs = 0;

            var action = this.sharedState.CreateAction((int baseValue) =>
            {
                b.Value = baseValue * 2;
                latest = b.Value;
            });

            var d = this.sharedState.Autorun((r) =>
            {
                runs++;
                var current = a.Value;
                action(current);
            });

            Assert.Equal(6, b.Value);
            Assert.Equal(6, latest);

            a.Value = 7;

            Assert.Equal(14, b.Value);
            Assert.Equal(14, latest);

            a.Value = 8;

            Assert.Equal(16, b.Value);
            Assert.Equal(16, latest);

            b.Value = 7; // should have no effect

            Assert.Equal(8, a.Value);
            Assert.Equal(7, b.Value);
            Assert.Equal(16, latest); // effect not triggered

            a.Value = 3;

            Assert.Equal(6, b.Value);
            Assert.Equal(6, latest);

            Assert.Equal(4, runs);

            d.Dispose();
        }

        /// <summary>
        /// Tests that it should be possible to create an autorun inside an action.
        /// </summary>
        [Fact]
        public void ShouldBePossibleToCreateAutorunInAction()
        {
            var a = new ObservableValue<int>(this.sharedState, "int", this.sharedState.ReferenceEnhancer(), 1);
            var values = new List<int>();

            var adder = this.CreateAction<int, IDisposable>(increment =>
            {
                return this.sharedState.Autorun(r =>
                {
                    values.Add(a.Value + increment);
                });
            });

            var d1 = adder(2);
            a.Value = 3;
            var d2 = adder(17);
            a.Value = 24;
            d1.Dispose();
            a.Value = 11;
            d2.Dispose();
            a.Value = 100;

            Assert.Equal(new[] { 3, 5, 20, 26, 41, 28 }, values);
        }

        /// <summary>
        /// Should be possible to change unobserved state in an action called from computed.
        /// </summary>
        [Fact]
        public void ShouldBePossibleToChangeUnobservedStateInActionCalledFromComputed()
        {
            var a = new ObservableValue<int>(this.sharedState, "int", this.sharedState.ReferenceEnhancer(), 2);

            var testAction = this.sharedState.CreateAction(() =>
            {
                a.Value = 3;
            });

            var c = new ComputedValue<int>(this.sharedState, new ComputedValueOptions<int>(
            () =>
            {
                testAction();
                return 0;
            }, "computed"));

            this.sharedState.Autorun(r =>
            {
                int i = c.Value; // should not throw
            });

            Assert.Equal(3, a.Value);
        }

        /// <summary>
        /// should be possible to change observed state in an action
        /// called from computed if run inside _allowStateChangesInsideComputed.
        /// </summary>
        [Fact]
        public void ShouldBePossibleToChangeObservedStateInActionCalledFromComputedIfRunInsideAllowStateChange()
        {
            var a = new ObservableValue<int>(this.sharedState, "int", this.sharedState.ReferenceEnhancer(), 2);

            var d = this.sharedState.Autorun(r =>
            {
                int i = a.Value;
            });

            ComputedValue<int> c = null;
            ComputedValue<int> c2 = null;

            var testAction = this.sharedState.CreateAction(() =>
            {
                this.sharedState.AllowStateChangesInsideComputed(() =>
                {
                    a.Value = 3;
                    Assert.Throws<InvalidOperationException>(() =>
                    {
                        // Computed values are not allowed to cause side effects by
                        // changing observables that are already being observed
                        int i = c2.Value;
                    });
                    return 0;
                });
                Assert.Equal(3, a.Value);
                Assert.Throws<InvalidOperationException>(() =>
                {
                    // Computed values are not allowed to cause side effects by
                    // changing observables that are already being observed/
                    a.Value = 4;
                });
            });

            c = new ComputedValue<int>(this.sharedState, new ComputedValueOptions<int>(
            () =>
            {
                testAction();
                return a.Value;
            }, "computed"));

            c2 = new ComputedValue<int>(this.sharedState, new ComputedValueOptions<int>(
           () =>
           {
               a.Value = 6;
               return a.Value;
           }, "computed"));

            int j = c.Value;

            d.Dispose();
        }

        /// <summary>
        /// Tests that it should not be possible to change observed state in an action called from computed.
        /// </summary>
        [Fact]
        public void ShouldNotBePossibleToChangeObservedStateInAnActionCalledFromComputed()
        {
            var a = new ObservableValue<int>(this.sharedState, "int", this.sharedState.ReferenceEnhancer(), 2);

            var d = this.sharedState.Autorun(r =>
            {
                int i = a.Value;
            });

            var testAction = this.sharedState.CreateAction(() =>
            {
                a.Value = 3;
            });

            var c = new ComputedValue<int>(this.sharedState, new ComputedValueOptions<int>(
            () =>
            {
                testAction();
                return a.Value;
            }, "computed"));

            Assert.Throws<InvalidOperationException>(() =>
            {
                int i = c.Value;
            });

            d.Dispose();
        }

        /// <summary>
        /// Tests that action in autorun should be untracked.
        /// </summary>
        [Fact]
        public void ActionInAutorunShouldBeUntracked()
        {
            var a = new ObservableValue<int>(this.sharedState, "a", this.sharedState.ReferenceEnhancer(), 2);
            var b = new ObservableValue<int>(this.sharedState, "b", this.sharedState.ReferenceEnhancer(), 3);

            var values = new List<int>();

            var multiplier = this.CreateAction<int, int>(val => val * b.Value);

            var d = this.sharedState.Autorun(r =>
            {
                values.Add(multiplier(a.Value));
            });

            a.Value = 3;
            b.Value = 4;
            a.Value = 5;

            d.Dispose();

            a.Value = 6;

            Assert.Equal(new[] { 6, 9, 20 }, values);
        }

        /// <summary>
        /// Tests #286 exceptions in actions should not affect global state.
        /// </summary>
        [Fact]
        public void ExceptionsInActionsShouldNotAffectGlobalState()
        {
            int autoRunTimes = 0;
            var count = new ObservableValue<int>(this.sharedState, "a", this.sharedState.ReferenceEnhancer(), 2);

            var add = this.sharedState.CreateAction(() =>
            {
                count.Value++;
                if (count.Value == 2)
                {
                    throw new InvalidOperationException("An Action Error!");
                }
            });

            var d = this.sharedState.Autorun(r =>
            {
                autoRunTimes++;
                int i = count.Value;
            });

            try
            {
                add();
                Assert.Equal(2, autoRunTimes);
                add();
            }
            catch (InvalidOperationException)
            {
                Assert.Equal(3, autoRunTimes);
                add();
                Assert.Equal(4, autoRunTimes);
            }
        }

        /// <summary>
        /// Tests run in action.
        /// </summary>
        [Fact]
        public void RunInAction()
        {
            this.sharedState.Configuration.EnforceActions = EnforceAction.Observed;

            var values = new List<int>();
            var events = new List<ActionStartSpyEventArgs>();

            this.sharedState.SpyEvent += (s, e) =>
            {
                if (e is ActionStartSpyEventArgs spyEventArgs)
                {
                    events.Add(spyEventArgs);
                }
            };

            var observable = new ObservableValue<int>(this.sharedState, "a", this.sharedState.ReferenceEnhancer(), 0);

            var d = this.sharedState.Autorun(r =>
            {
                values.Add(observable.Value);
            });

            int res = 0;

            this.sharedState.RunInAction("increment", () =>
            {
                observable.Value = observable.Value + (6 * 2);
                observable.Value = observable.Value - 3; // oops
                res = 2;
            });

            Assert.Equal(2, res);
            Assert.Equal(new[] { 0, 9 }, values);

            this.sharedState.RunInAction(() =>
            {
                observable.Value = observable.Value + (5 * 2);
                observable.Value = observable.Value - 4; // oops
                res = 3;
            });

            Assert.Equal(3, res);
            Assert.Equal(new[] { 0, 9, 15 }, values);
            Assert.Collection(
                events,
                evt =>
            {
                Assert.Equal(Array.Empty<object>(), evt.Arguments);
                Assert.Equal("increment", evt.Name);
            },
                evt =>
            {
                Assert.Equal(Array.Empty<object>(), evt.Arguments);
                Assert.Equal("<unnamed action>", evt.Name);
            });

            d.Dispose();
        }

        /// <summary>
        /// Tests that an action in autorun does not keep / make computed values alive.
        /// </summary>
        [Fact]
        public void ActionInAutoRunDoesNotKeepComputedValuesAlive()
        {
            var calls = 0;
            var computed = new ComputedValue<int>(this.sharedState, new ComputedValueOptions<int>(
            () =>
            {
               return calls++;
            }, "computed"));

            Action callComputedTwice = () =>
            {
                int i = computed.Value;
                int j = computed.Value;
            };

            Action<Action> runWithMemoizing = (act) =>
            {
                this.sharedState.Autorun(r => act()).Dispose();
            };

            callComputedTwice();
            Assert.Equal(2, calls);

            runWithMemoizing(callComputedTwice);
            Assert.Equal(3, calls);

            callComputedTwice();
            Assert.Equal(5, calls);

            runWithMemoizing(() =>
            {
                this.sharedState.RunInAction(callComputedTwice);
            });

            Assert.Equal(6, calls);

            callComputedTwice();
            Assert.Equal(8, calls);
        }

        /// <summary>
        /// Tests computed values and actions.
        /// </summary>
        [Fact]
        public void ComputedValuesAndActions()
        {
            var calls = 0;

            var number = new ObservableValue<int>(this.sharedState, "a", this.sharedState.ReferenceEnhancer(), 1);
            var squared = new ComputedValue<int>(this.sharedState, new ComputedValueOptions<int>(
            () =>
            {
                calls++;
                return number.Value * number.Value;
            }, "squared"));

            var changeNumber10Times = this.sharedState.CreateAction(() =>
            {
                int i = squared.Value;
                i = squared.Value;

                for (i = 0; i < 10; i++)
                {
                    number.Value += 1;
                }
            });

            changeNumber10Times();
            Assert.Equal(1, calls);

            var d = this.sharedState.Autorun(r =>
             {
                 changeNumber10Times();
                 Assert.Equal(2, calls);
             });

            d.Dispose();
            Assert.Equal(2, calls);

            changeNumber10Times();
            Assert.Equal(3, calls);
        }

        /// <summary>
        /// Tests out of order startactions / endactions.
        /// </summary>
        [Fact]
        public void OutOfOrderStartActionEndAction()
        {
            var a1 = Core.ActionExtensions.StartAction(this.sharedState, "a1", this, Array.Empty<object>());
            var a2 = Core.ActionExtensions.StartAction(this.sharedState, "a1", this, Array.Empty<object>());

            Assert.Throws<InvalidOperationException>(() => Core.ActionExtensions.EndAction(a1));

            Core.ActionExtensions.EndAction(a2);

            // double finishing
            Assert.Throws<InvalidOperationException>(() => Core.ActionExtensions.EndAction(a2));

            Core.ActionExtensions.EndAction(a1);
        }

        /// <summary>
        /// Helper to have an action that returns something.
        /// </summary>
        /// <typeparam name="T1">The type of the first argument.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="func">The func to run.</param>
        /// <returns>A function.</returns>
        private Func<T1, TResult> CreateAction<T1, TResult>(Func<T1, TResult> func)
        {
            return (T1 arg1) =>
            {
                TResult result = default;
                this.sharedState.RunInAction(() =>
                {
                    result = func(arg1);
                });
                return result;
            };
        }
    }
}
