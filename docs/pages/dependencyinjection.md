# Dependency Injection.

Cortex.Net is a [DI friendly library](https://blog.ploeh.dk/2014/05/19/di-friendly-library/). This means that a lot of code is written to
interfaces, not implementations of those interfaces. It works with any DI Container. There are some tips and tricks to get the most out
of your DI container.

## Register ISharedState in the container.

To be able to access [ISharedState](xref:Cortex.Net.ISharedState), and to be able to pass it as an argument to other objects that take it
on as a dependency, it is customary to register an implementation of [ISharedState](xref:Cortex.Net.ISharedState) inside the container.

This can either be a single container registered as singleton, or multiple containers with the appropriate scope, as [described here](sharedstate.md).
An example of a registration of ISharedState in the [DI container from microsoft](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection) is below:

```csharp
using Cortex.Net.Api;
using Cortex.Net;

public void ConfigureServices(IServiceCollection services)
{
    // Add the shared state to the DI container as a singleton.
    services.AddSingleton<ISharedState>(sp => 
    {
        // configure
        var configuration = new CortexConfiguration()
        {
            // enforce that state mutation always happens inside an action.
            EnforceActions = EnforceAction.Always;
		};

        // create an instance using the configuration
        return new SharedState(configuration);
    });
}
```

## Add your stores to the DI container to share them.

Group your observables in observable stores that can be shared across components in your application. These stores
can be registered in the DI Container to share them across all components and leverage observability:

```csharp
using Cortex.Net.Api;
using Cortex.Net;

public void ConfigureServices(IServiceCollection services)
{

    // Add a store as singleton to the DI container and make sure that it is tied to the correct
    // shared state.
    services.AddSingleton<PeopleStore>(sp =>
    {
        // resolve the scoped shared state.
        var sharedState = s.GetService<ISharedState>();

        // create a new people store and return it.
        return sharedState.Observable(() => new PeopleStore());
    });
}
```