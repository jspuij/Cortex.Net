# Blazor Components / inject

Cortex.Net observable models / stores can be easily shared by multiple UI Components by injecting a store into the
component using the [DI container](https://docs.microsoft.com/en-us/aspnet/core/blazor/dependency-injection) provided
by Blazor and the Inject attribute.

## Example:

Define a store like this:

```csharp
    using Cortex.Net.Api;

    /// <summary>
    /// Store of Todo items.
    /// </summary>
    [Observable]
    public class TodoStore
    {
        /// <summary>
        /// Gets the Todo items.
        /// </summary>
        public IList<Todo> Todos { get; private set; }

    }
```

And subsequently inject the model into one or more Blazor Components:

```cshtml
@using  Cortex.Net.Blazor

@attribute [Observer]
@inject TodoStore TodoStore

<p>There are @(this.TodoStore.Count) todo items in the store</p>
```

The component will automatically rerender when the Count property changes.

Do not forget to register the store with the DI container.
For client side Blazor this would be:

```csharp
public static async Task Main(string[] args)
{
    var builder = WebAssemblyHostBuilder.CreateDefault(args);

    // Add the Shared state to the DI container.
    builder.Services.AddSingleton(x => SharedState.GlobalState);

    // Add a singleton TodoStore to the DI container.
    builder.Services.AddSingleton<TodoStore>();

    builder.RootComponents.Add<App>("app");

    await builder.Build().RunAsync().ConfigureAwait(true);
}
```
