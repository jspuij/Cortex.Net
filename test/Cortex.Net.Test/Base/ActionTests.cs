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
            () => a.Value + 2, "doubler"));

            this.sharedState.Autorun((r) =>
            {
                b = a.Value * 2;
            });

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

        /*

               test("runInAction", () => {
                   mobx.configure({ enforceActions: "observed" })
                   const values = []
                   const events = []
                   const spyDisposer = mobx.spy(ev => {
                       if (ev.type === "action")
                           events.push({
                               name: ev.name,
                               arguments: ev.arguments
                           })
                   })

                   const observable = mobx.observable.box(0)
                   const d = mobx.autorun(() => values.push(observable.get()))

                   let res = mobx.runInAction("increment", () => {
                       observable.set(observable.get() + 6 * 2)
                       observable.set(observable.get() - 3) // oops
                       return 2
                   })

                   expect(res).toBe(2)
                   expect(values).toEqual([0, 9])

                   res = mobx.runInAction(() => {
                       observable.set(observable.get() + 5 * 2)
                       observable.set(observable.get() - 4) // oops
                       return 3
                   })

                   expect(res).toBe(3)
                   expect(values).toEqual([0, 9, 15])
                   expect(events).toEqual([
                       { arguments: [], name: "increment" },
                       { arguments: [], name: "<unnamed action>" }
                   ])

                   mobx.configure({ enforceActions: "never" })
                   spyDisposer()

                   d()
               })

               test("action in autorun does not keep / make computed values alive", () => {
                   let calls = 0
                   const myComputed = mobx.computed(() => calls++)
                   const callComputedTwice = () => {
                       myComputed.get()
                       myComputed.get()
                   }

                   const runWithMemoizing = fun => {
                       mobx.autorun(fun)()
                   }

                   callComputedTwice()
                   expect(calls).toBe(2)

                   runWithMemoizing(callComputedTwice)
                   expect(calls).toBe(3)

                   callComputedTwice()
                   expect(calls).toBe(5)

                   runWithMemoizing(function() {
                       mobx.runInAction(callComputedTwice)
                   })
                   expect(calls).toBe(6)

                   callComputedTwice()
                   expect(calls).toBe(8)
               })

               test("computed values and actions", () => {
                   let calls = 0

                   const number = mobx.observable.box(1)
                   const squared = mobx.computed(() => {
                       calls++
                       return number.get() * number.get()
                   })
                   const changeNumber10Times = mobx.action(() => {
                       squared.get()
                       squared.get()
                       for (let i = 0; i < 10; i++) number.set(number.get() + 1)
                   })

                   changeNumber10Times()
                   expect(calls).toBe(1)

                   mobx.autorun(() => {
                       changeNumber10Times()
                       expect(calls).toBe(2)
                   })()
                   expect(calls).toBe(2)

                   changeNumber10Times()
                   expect(calls).toBe(3)
               })

               test("extendObservable respects action decorators", () => {
                   const x = mobx.observable(
                       {
                           a1() {
                               return this
                           },
                           a2() {
                               return this
                           },
                           a3() {
                               return this
                           }
                       },
                       {
                           a1: mobx.action,
                           a2: mobx.action.bound
                       }
                   )
                   expect(mobx.isAction(x.a1)).toBe(true)
                   expect(mobx.isAction(x.a2)).toBe(true)
                   expect(mobx.isAction(x.a3)).toBe(false)

                   // const global = (function() {
                   //     return this
                   // })()

                   const { a1, a2, a3 } = x
                   expect(a1.call(x)).toBe(x)
                   // expect(a1()).toBe(global)
                   expect(a2.call(x)).toBeTruthy() // it is not this! proxies :) see test in proxies.js
                   expect(a2()).toBeTruthy()
                   expect(a3.call(x)).toBe(x)
                   // expect(a3()).toBe(global)
               })

               test("expect warning for invalid decorator", () => {
                   expect(() => {
                       mobx.observable({ x: 1 }, { x: undefined })
                   }).toThrow(/Not a valid decorator for 'x', got: undefined/)
               })

               test("expect warning superfluos decorator", () => {
                   expect(() => {
                       mobx.observable({ x() {} }, { y: mobx.action })
                   }).toThrow(/Trying to declare a decorator for unspecified property 'y'/)
               })

               test("bound actions bind", () => {
                   let called = 0
                   const x = mobx.observable(
                       {
                           y: 0,
                           z: function(v) {
                               this.y += v
                               this.y += v
                           },
                           get yValue() {
                               called++
                               return this.y
                           }
                       },
                       {
                           z: mobx.action.bound
                       }
                   )

                   const d = mobx.autorun(() => {
                       x.yValue
                   })
                   const events = []
                   const d2 = mobx.spy(e => events.push(e))

                   const runner = x.z
                   runner(3)
                   expect(x.yValue).toBe(6)
                   expect(called).toBe(2)

                   expect(events.filter(e => e.type === "action").map(e => e.name)).toEqual(["z"])
                   expect(Object.keys(x)).toEqual(["y"])

                   d()
                   d2()
               })

               test("Fix #1367", () => {
                   const x = mobx.extendObservable(
                       {},
                       {
                           method() {}
                       },
                       {
                           method: mobx.action
                       }
                   )
                   expect(mobx.isAction(x.method)).toBe(true)
               })

               test("error logging, #1836 - 1", () => {
                   const messages = utils.supressConsole(() => {
                       try {
                           const a = mobx.observable.box(3)
                           mobx.autorun(() => {
                               if (a.get() === 4) throw new Error("Reaction error")
                           })

                           mobx.action(() => {
                               a.set(4)
                               throw new Error("Action error")
                           })()
                       } catch (e) {
                           expect(e.toString()).toEqual("Error: Action error")
                           console.error(e)
                       }
                   })

                   expect(messages).toMatchSnapshot()
               })

               test("error logging, #1836 - 2", () => {
                   const messages = utils.supressConsole(() => {
                       try {
                           const a = mobx.observable.box(3)
                           mobx.autorun(() => {
                               if (a.get() === 4) throw new Error("Reaction error")
                           })

                           mobx.action(() => {
                               a.set(4)
                           })()
                       } catch (e) {
                           expect(e.toString()).toEqual("Error: Action error")
                           console.error(e)
                       }
                   })

                   expect(messages).toMatchSnapshot()
               })

               test("out of order startAction / endAction", () => {
                   const a1 = mobx._startAction("a1")
                   const a2 = mobx._startAction("a2")

                   expect(() => mobx._endAction(a1)).toThrow("invalid action stack")

                   mobx._endAction(a2)

                   // double finishing
                   expect(() => mobx._endAction(a2)).toThrow("invalid action stack")

                   mobx._endAction(a1)
               })
                        */

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
