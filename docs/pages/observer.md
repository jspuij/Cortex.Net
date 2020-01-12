# Observer

The `Observer` attribute is only implemented in Blazor for now. It is possible to integrate the Attribute in other UI frameworks easily. Create an issue if your favorite UI framework is not supported yet. Feel free to implement it and create a pull request if you want to do it yourself.

## Blazor

The [Observer](xref:Cortex.Net.Blazor.ObserverAttribute) attribute subscribes Blazor components automatically to _any observables_ that are used _during render_.
As a result, components will automatically re-render when relevant observables change.
But it also makes sure that components don't re-render when there are _no relevant_ changes.
As a result Cortex.NET applications are in practice much better optimized than Redux-like or vanilla Blazor applications are out of the box.

- `Observer` is provided through the separate [`Cortex.Net.Blazor` package](https://www.nuget.org/packages/Cortex.Net.Blazor/).

## `Observer` automatically tracks observables used during render

Using `Observer` is pretty straight forward:

```cshtml-razor
@using  Cortex.Net.Blazor
@using  Cortex.Net.Api

// this component is an observer.
@attribute [Observer]

<span>Seconds passed: @(this.SecondsPassed) </span>

@code
{
    [Observable]
    int SecondsPassed { get; set;} = 0;

    protected override void OnInitialized()
    {
        Task.Run(async () => 
        {
            // setup a loop that runs every second but allows other tasks
            // to run as it is async.
            while (true)
            {
                // tick every second.
                await Task.Delay(1000);
                // Invoke to make sure we are running on the UI thread.
                Invoke(Tick);
            }
        } );
    }

    [Action]
    void Tick()
    {
        // look ma, no StateHasChanged Required!
        SecondsPassed += 1;
    }
}

```

Because `Observer` automatically tracks any observables that are used (and none more), the `Timer` component above will automatically re-render whever `SecondsPassed` is updated, since it is declared as an observable.

Note that `Observer` _only_ subscribes to observables used during the _own_ render of the component. So if observables are passed to child components, those have to be marked as `Observer` as well. This also holds for any callback based components.

## When to apply `Observer`?

The simple rule of thumb is: _all components that render observable data_. That observable data might exist on the component itself, but of course can also come from an injected service or any other place that has the [[Observable]](xref:Cortex.Net.Api.ObservableAttribute) attribute applied.

## Characteristics of observer components

-   Observer enables your components to interact with state that is not managed by Blazor, and still update as efficiently as possible. This is great for decoupling.
-   Observers only subscribe to the data structures that were actively used during the last render. This means that you cannot under-subscribe or over-subscribe. You can even use data in your rendering that will only be available at later moment in time. This is ideal for asynchronously loading data.
-   You are not required to declare what data a component will use. Instead, dependencies are determined at runtime and tracked in a very fine-grained manner.
-   `Observer` calls 'StateHasChanged` automatically so that children are not re-rendered unnecessary.
-   `Observer` based components sideways load data; parent components won't re-render unnecessarily even when child components will.

## Tips

#### Use the `<Observer>` component in cases where you can't use observer

Sometimes it is hard to apply `Observer` to a part of the rendering, for example because you are rendering inside a RenderFragment, and you don't want to extract a new component to be able to mark it as `observer`.
In those cases [`<Observer />`](xref:TBD) comes in handy. It takes a child content that is automatically re-rendered if any referenced observables change:


## Troubleshooting

1. Make sure you didn't forget `Observer` (yes, this is the most common mistake)
2. Make sure you grok how tracking works in general: [what will Cortex.Net react to](breact.md)
3. Read the [common mistakes](pitfalls.md) section
4. Use [Trace](trace.md) to verify that you are subscribing to the right things or check what MobX is doing in general using [Spy](spy.md).
