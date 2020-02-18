# Autorun

[Autorun](xref:Cortex.Net.Api.SharedStateReactionExtensions.Autorun(Cortex.Net.ISharedState,Action{Cortex.Net.Core.Reaction},Cortex.Net.AutorunOptions))
can be used in those cases where you want to create a reactive function that will never have observers itself. This is
usually the case when you need to bridge from reactive to imperative code, for example for logging, persistence, or
UI-updating code. When the `Autorun` extension method on `ISharedState` is used, the provided function will always be
triggered once immediately and then again each time one of its dependencies changes. In contrast,
[Computed](computed.md) creates functions that only re-evaluate if it has observers on its own, otherwise its value is
considered to be irrelevant. As a rule of thumb: use `Autorun` if you have a function that should run automatically but
that doesn't result in a new value.
Use `Computed` for everything else. Autoruns are about initiating _effects_, not about producing new values.
You can pass an instance of [AutorunOptions](xref:Cortex.Net.AutorunOptions) as an argument to `Autorun`, it will be
used to set options like a debug name or a delay.

The return value from autorun is an `IDisposable` instance, which can be used to dispose of the autorun when you no
longer need it. The reaction itself will also be passed as the only argument to the function given to autorun, which
allows you to manipulate it from within the autorun function. This means there are two ways you can dispose of the
reaction when you no longer need it:

```csharp
using Cortex.Net.Api;
using Cortex.Net;

var sharedState = SharedState.GlobalState;

var disposable = sharedState.Autorun(reaction =>
{
    /* do some stuff */
});

disposable.Dispose();

// or

sharedState.Autorun(reaction =>
{
    /* do some stuff */
    reaction.Dispose();
});
```

Just like the [`Observer` attribute](observer.md), `Autorun` will only observe data that is used during the execution
of the provided function.

```csharp
using Cortex.Net.Api;
using Cortex.Net;

var sharedState = SharedState.GlobalState;

var numbers = sharedState.Collection(new[] { 1, 2, 3 });

var sum = sharedState.Computed(() => numbers.Sum());

// prints '6'
var disposable = sharedState.Autorun(() => Console.WriteLine(sum.Value));

// prints '10'
numbers.Add(4);

disposable.Dispose();

// won't print anything, nor is `sum` re-evaluated
numbers.Add(5);
```

## Options

Autorun accepts as the second argument an [AutorunOptions](xref:Cortex.Net.AutorunOptions) instance with the following
options:

-   `Delay`: Number in milliseconds that can be used to debounce the effect delegate. If zero (the default), no
    debouncing will happen.
-   `Name`: String that is used as name for this reaction in for example [`Spy`](spy.md) events.
-   `ErrorHandler`: delegate that will handle the errors of this reaction, rather then propagating them.
-   `Scheduler`: Set a custom scheduler to determine how re-running the autorun function should be scheduled. It takes
    a async delegate that should be invoked at some point in the future, for example:
    `{ Scheduler: async () => { await Task.Delay(1000); }}`

## The `Delay` option

```csharp
using Cortex.Net.Api;
using Cortex.Net;
using System.Text.Json;

var sharedState = SharedState.GlobalState;

sharedState.Autorun(
    r => {
        // Assuming that profile is an observable object,
        // send it to the server each time it is changed, 
        // but await at least 300 milliseconds before sending it.
        // When sent, the latest value of profile will be used.
        sendProfileToServer(JsonSerializer.Serialize(profile));
    },
    new AutorunOptions() { Delay: 300 }
);
```

## The `ErrorHandler` option

Exceptions thrown in autorun and all other types reactions are caught and logged to the console or debugger, but not
propagated back to the original causing code. This is to make sure that a reaction in one exception does not prevent the
scheduled execution of other, possibly unrelated, reactions. This also allows reactions to recover from exceptions;
throwing an exception does not break the tracking done by Cortex.Net, so as subsequent run of a reaction might complete
normally again if the cause for the exception is removed.

It is possible to override the default logging behavior of Reactions by providing the `ErrorHandler` option
Example:

```csharp
using Cortex.Net.Api;
using Cortex.Net;

var sharedState = SharedState.GlobalState;

const age = sharedState.Box(10);

const dispose = sharedState.Autorun(
    r => 
    {
        if (age.Value < 0)
        {
            throw new Exception("Age should not be negative");
        } 
        Console.WriteLine($"Age: {age.Value}");
    },
    new AutorunOptions() {
        ErrorHandler = (r, e) => {
            Debug.WriteLine($"Invalid age in reaction. Exception: {e.Message}");
        }
    }
);
```

A global onError handler on the shared state can be set as well, use
[ISharedState.UnhandledReactionException](xref:Cortex.Net.ISharedState.UnhandledReactionException). This can be useful
in tests or for client side error monitoring.
