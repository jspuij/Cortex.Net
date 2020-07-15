# computed

Computed values are values that can be derived from the existing state or other computed values.
Conceptually, they are very similar to formulas in spreadsheets.
Computed values can't be underestimated, as they help you to make your actual modifiable state as small as possible.
Besides that they are highly optimized, so use them wherever possible.

Don't confuse `computed` with `autorun`. They are both reactively invoked expressions,
but use `computed` if you want to reactively produce a _value_ that can be used by other observers and
`autorun` if you don't want to produce a new value but rather want to achieve an _effect_.
For example imperative side effects like logging, making network requests etc.

Computed values are automatically derived from your state if any value that affects them changes.
Computed values can be optimized away in many cases by Cortex.Net as they are assumed to be pure.
For example, a computed property won't re-run if none of the data used in the previous computation changed.
Nor will a computed property re-run if it is not in use by some other computed property or reaction.
In such cases it will be suspended.

This automatic suspension is very convenient. If a computed value is no longer observed, for example the UI
in which it was used no longer exists, Cortex.Net can automatically garbage collect it. 
This differs from `autorun`'s values where you must dispose of them yourself.
It sometimes confuses people new to Cortex.Net, that if you create a computed property but don't use it anywhere in a 
reaction, it will not cache its value or recompute unnecessarily.
However, in real life situations this is by far the best default, and you can always forcefully keep a
computed value awake if you need to, by using either [`observe`](observer.md) or the
[`keepAlive`](xref:Cortex.Net.ComputedValueOptions`1.KeepAlive) option.

Note that `computed` properties are not enumerable. Nor can they be overwritten in an inheritance chain.

## `ComputedAttribute`

You can use the [ComputedAttribute](xref:Cortex.Net.Api.ComputedAttribute) on any getter of a class property to
declaratively create computed properties.

```csharp
using Cortex.Net.Api;
using Cortex.Net;

public class OrderLine
{
    [Observable]
    public Decimal Price { get; set;} = 0.0m;

    [Observable]
    public Decimal Amount { get; set;} = 1.0m;

    [Computed]
    public Decimal Total => Price * Amount;
}
```

## Setters for computed values

It is possible to define a setter for computed values as well. Note that these setters cannot be used to alter the value
of the computed property directly, but they can be used as 'inverse' of the derivation. For example:

```csharp
using Cortex.Net.Api;
using Cortex.Net;

public class OrderLine
{
    [Observable]
    public Decimal Price { get; set;} = 0.0m;

    [Observable]
    public Decimal Amount { get; set;} = 1.0m;

    [Computed]
    public Decimal Total 
    {
        get => Price * Amount;
        set => Price = value / Amount; // infer price from total
    }
}
```

And similarly

```csharp
using Cortex.Net.Api;
using Cortex.Net;


public class Foo {

    [Observable]
    public Double Length { get; set;} = 2d;

    [Computed]
    public Double Squared
    {
        get => Length * Length;

        //this is automatically an action, no annotation necessary
        set => Length = Math.Sqrt(value);
	}
}
```

## `Computed(expression)` as method.

[Computed](xref:Cortex.Net.Api.SharedStateObservableExtensions.Computed``1(Cortex.Net.ISharedState,Func{``0},System.String))
can also be invoked directly as an extension method to ISharedState. Use [`.Value`](xref:Cortex.Net.IValue`1.Value) on
the returned object to get the current value of the computation, or
[`.Observe()`](xref:Cortex.Net.IComputedValue`1.Observe(EventHandler{Cortex.Net.Types.ValueChangedEventArgs{`0}},System.Boolean))
to observe its changes. This form of `computed` is not used very often, but in some cases where you need to pass a
computed value around it might prove useful.

Example:

```csharp
using Cortex.Net.Api;
using Cortex.Net;

var sharedState = SharedState.GlobalState;

var name = sharedState.Box("John");

var upperCaseName = sharedState.Computed(() => name.Value.ToUpperCase());

var disposer = upperCaseName.Observe(change => Console.WriteLine(change.NewValue));

// prints: 'DAVE'
name.Value = "Dave";
```
