# when
Declaration
```csharp
public static IDisposable When(this ISharedState sharedState, Func<bool> predicate, Action effect, WhenOptions whenOptions = null)
```

[When](xref:Cortex.Net.Api.SharedStateWhenExtensions.When(Cortex.Net.ISharedState,Func{System.Boolean},Action,Cortex.Net.WhenOptions)) 
observes & runs the given `predicate` until it returns true. Once that happens, the given `effect` is executed and the
autorunner is disposed. The function returns a disposer to cancel the autorunner prematurely.

This function is really useful to dispose or cancel stuff in a reactive way.
For example:

```csharp
using Cortex.Net;
using Cortex.Net.Api;

var sharedState = SharedState.GlobalState;

[Observable]
sealed class MyResource : IDisposable {
    MyResource() {
        sharedState.When(
            // once...
            () => !this.Visible,
            // ... then
            () => this.Dispose()
        )
    }

    [Computed]
    Visible =>
    {
        // indicate whether this item is visible
    }

    override void Dispose() {
        // dispose
    }
}
```

## async when

If there is no `effect` delegate provided, `when` will return a `Task`. This combines nicely with `async / await`

```csharp
async Task MyFunction() {
	await sharedState.When(() => that.isVisible)
	// etc..
}
```
