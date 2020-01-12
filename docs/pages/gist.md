# The gist of Cortex.Net

So far it all might sound a bit fancy, but making an app reactive using Cortex.Net boils down to just these three steps:

## 1. Define your state and make it observable

Store state in any data structure you like; objects, collections, classes.
Cyclic data structures, references, it doesn't matter.
Just make sure that all properties that you want to change over time are marked by the [[Observable]](xref:Cortex.Net.Api.ObservableAttribute) attribute to make them observable.

```csharp
using Cortex.Net.Api;

[Observable]
public class AppState
{
    public int Timer { get; }
}
```


## 2. Create a view that responds to changes in the State

We didn't make our `AppState` observable for nothing;
you can now create views that automatically update whenever relevant data in the `AppState` changes.
Cortex.Net will find the minimal way to update your views.
This single fact saves you tons of boilerplate and is wickedly efficient.

Generally speaking any Action/Func/Method can become a reactive view that observes its data, and Cortex.Net can be applied in any .NET netstandard environment.
But here is an example of a view in the form of a Blazor component.

```cshtml-razor
@using Cortex.Net.Blazor

@attribute [Observer]
@inject AppState AppState

<button @onClick="ResetTimer">
    Seconds passed: {this.AppState.Timer}
</button>

@code
{
    void ResetTimer(MouseEventArgs args)
    {
        this.AppState.ResetTimer();
    }
}

```

(For the implementation of `ResetTimer` function see the next section)

## 3. Modify the State

The third thing to do is to modify the state.
That is what your app is all about after all.
Unlike many other frameworks, Cortex.Net doesn't dictate how you do this.
There are best practices, but the key thing to remember is:
**_Cortex.Net helps you do things in a simple straightforward way_**.

The following code will alter your data every second, and the UI will update automatically when needed.
No explicit relations are defined in either the controller functions that _change_ the state or in the views that should _update_.
Decorating your _state_ and _views_ with [[Observable]](xref:Cortex.Net.Api.ObservableAttribute) is enough for Cortex.Net to detect all relationships.
Here are two examples of changing the state:

```csharp

using Cortex.Net.Api;

[Observable]
public class AppState
{
    public int Timer { get; }

    [Action]
    public void ResetTimer()
    {
        Timer = 0;
    }

    [Action]
    public async Run()
    {
        while(true)
        {
            Timer += 1;
            await Task.Delay(1000);
        }
    }
}

```

The `Action` attribute is only neccessary when using Cortex.Net enforces modification through reactions.
It is recommended to use action though as it will help you to better structure applications and expresses the intention of a function to modify state.
It automatically applies transactions for optimal performance as well.