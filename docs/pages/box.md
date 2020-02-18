# Box

## Built in types and references

All built-in types in C# don't have properties and hence per definition not observable.
Usually that is fine, as Cortex.NET usually can just make the _property_ that contains the value observable.
See also [observables](observable.md).
In rare cases it can be convenient to have an observable "variable" that is not owned by an object.
For these cases it is possible to create an observable box that manages such a variable.

### ISharedState.Box(value)

So [ISharedState.Box(value)](xref:Cortex.Net.Api.SharedStateObservableExtensions.Box``1(Cortex.Net.ISharedState,``0,System.String,Cortex.Net.IEnhancer))
accepts any value and stores it inside a box. The current value can be accessed through `.Value` property and updated
using `.Value =`.

Furthermore you can register a callback using its `.Observe` method to listen to changes on the stored value.
But since Cortex.Net tracks changes to boxes automatically, in most cases it is better to use a reaction like
[autorun](autorun.md) instead.

So the signature of object returned by `observable.box(scalar)` is:

-   `.Value` Returns the current value.
-   `.Value = ` Replaces the currently stored value. Notifies all observers.
-   `.Change`. Event that can be used to intercept changes before they are applied.
-   `.Changed`. Event that will fire each time the stored value is replaced.

### Example

```csharp
using Cortex.Net.Api;
using Cortex.Net;

var sharedState = SharedState.GlobalState;

var cityName = sharedState.Box("Vienna");

Console.WriteLine(cityName.Value);
// prints 'Vienna'

cityName.Observe((sender, eventArgs) => 
{
    Console.WriteLine($"{eventArgs.OldValue} -> {eventArgs.NewValue}");
}, false); // do not fire immediately.

cityName.Value = Amsterdam;

// prints 'Vienna -> Amsterdam'
```

## ISharedState.Box(value, name)

The `name` parameter can be used to give the box a friendly debug name, to be used in for example `spy` or the React
dev tools.
