# Reaction

Declaration: `IDisposable Reaction<T>(this ISharedState sharedState, Func<Reaction, T> expression, Action<T, Reaction>
effect, ReactionOptions<T> reactionOptions = null);`.

A variation on [Autorun](autorun.md) that gives more fine grained control on which observables will be tracked.
It takes two delegates, the first one (the _expression_ function) is tracked and returns data that is used as input for
the second one, the _effect_ function.
Unlike `Autorun` the side effect won't be run directly when created, but only after the data expression returns a new
value for the first time. Any observables that are accessed while executing the side effect will not be tracked.

`Reaction` returns an IDisposable instance.

The second function (the _effect_ function) passed to `Reaction` will receive two arguments when invoked.
The first argument is the value returned by the _expression_ function. The second argument is the current reaction,
which can be used to dispose the `Reaction` during execution.

It is important to notice that the side effect will _only_ react to data that was _accessed_ in the data expression,
which might be less than the data that is actually used in the effect.
Also, the side effect will only be triggered when the data returned by the expression has changed.
In other words: reaction requires you to produce the things you need in your side effect.

## Options

Reaction accepts a third argument as an [ReactionOptions&lt;T&gt;](xref:Cortex.Net.ReactionOptions-1) with the following
optional options:

-   `FireImmediately`: Boolean that indicates that the effect function should immediately be triggered after the first
    run of the data function. `false` by default.
-   `Delay`: Number in milliseconds that can be used to debounce the effect function. If zero (the default), no
    debouncing will happen.
-   `EqualityComparer`: If specified, this comparer function will be used to compare the previous and next values
    produced by the _expression_ function. The _effect_ function will only be invoked if this function returns false.
    If specified, this will override the default `Equals()` Comparison.
-   `Name`: String that is used as name for this reaction in for example [`Spy`](spy.md) events.
-   `ErrorHandler`: delegate that will handle the errors of this reaction, rather then propagating them.
-   `Scheduler`: Set a custom scheduler to determine how running the effect function should be scheduled. It takes a
    async delegate that should be invoked at some point in the future, for example: `{ Scheduler: async () =>
    { await Task.Delay(1000); }}`

## Example

In the following example both `reaction1`, `reaction2` and `autorun1` will react to the addition, removal or replacement
of todo's in the `todos` collection. But only `reaction2` and `autorun` will react to the change of a `Title` in one of
the todo items, because `Title` is used in the data expression of reaction 2, while it isn't in the data expression of
reaction 1. `Autorun` tracks the complete side effect, hence it will always trigger correctly, but is also more
suspectible to accidentally accessing unrelevant data. See also [what will Cortex.NET React to?](react.md).

```csharp
using Cortex.Net.Api;
using Cortex.Net;

[Observable]
class Todo
{
    string Title { get; set; }

    bool Done { get;set; }
}

var sharedState = SharedState.GlobalState;

var todos = sharedState.Collection(new Todo[]
    {
        Title = "Make coffee",
        Done = true
    },
    {
        title = "Find biscuit",
        Done = false
    }
]);

// wrong use of reaction: reacts to count changes, but not to title changes!
var reaction1 = sharedState.Reaction(
    r => todos.Count,
    (count, r) => Console.WriteLine("reaction 1:", string.Join(", ", todos.Select(todo => todo.Title)))
);

// correct use of reaction: reacts to count and title changes
var reaction2 = sharedState.Reaction(
    r => todos.Select(todo => todo.Title),
    (titles, r) => Console.WriteLine("reaction 2:", string.Join(", ", titles))
);

// autorun reacts to just everything that is used in its delegate
var autorun1 = sharedState.Autorun(r => Console.WriteLine("autorun 1:", string.Join(", ",
todos.Select(todo => todo.Title))));

todos.Add(new Todo { Title = "explain reactions", Done = false });

// prints:
// reaction 1: Make coffee, find biscuit, explain reactions
// reaction 2: Make coffee, find biscuit, explain reactions
// autorun 1: Make coffee, find biscuit, explain reactions

todos[0].Title = "Make tea";
// prints:
// reaction 2: Make tea, find biscuit, explain reactions
// autorun 1: Make tea, find biscuit, explain reactions
```

In the following example `reaction3`, will react to the change in the `counter` count.
When invoked `Reaction`, second argument can use as disposer.
The following example shows a `Reaction` that is invoked only once.

```csharp
using Cortex.Net.Api;
using Cortex.Net;

[Observable]
class Counter
{
    int Count { get; set; }
}

var sharedState = SharedState.GlobalState;

var counter = new Counter();

// invoke once of and dispose reaction: reacts to observable value.
var reaction3 = sharedState.Reaction(
    r => counter.Count,
    (count, reaction) => {
        Console.WriteLine($"reaction 3: invoked. counter.count = {count}");
        reaction.Dispose();
    }
);

counter.Count = 1;
// prints:
// reaction 3: invoked. counter.count = 1

counter.Count = 2;
// prints nothing because the reaction is disposed.

Console.WriteLine(counter.Count);
// prints:
// 2

```
