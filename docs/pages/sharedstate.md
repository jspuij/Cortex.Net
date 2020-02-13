# Shared state

To be able to track observables and generate a dependency graph, Cortex.Net has to
keep its own internal state somewhere. It does this by maintaining a reference to an
[ISharedState](xref:Cortex.Net.ISharedState) instance in every Observable and Derivation.
In most applications with a UI this will be a single instance for the entire application.
However other scenarios are entirely possible. Two distict possibilities are described below:

## A single global state

This is the most common scenario where one instance is shared among all Observables and
Derivations. It's also the default scenario; calling a constructor on an Observable or
referencing [Cortex.Net.SharedState.GlobalState](xref:Cortex.Net.SharedState.GlobalState)
will automatically and lazily create a global instance of shared state:

```csharp
using Cortex.Net;

// this will lazily create a single global shared state instance.
var sharedState = SharedState.GlobalState;
```

```csharp
using Cortex.Net;

// this would create a single global shared state instance as well
// if the Person class (or a property) was decorated with the [Observable] attribute.
var person = new Person();
```

This pattern is suitable for applications with a UI where the UI must be updated
on the main thread. It implies that the state must be read and updated on the main thread
as well. This does not mean however that you cannot use multiple threads or 
[Asynchronous Programming](https://en.wikipedia.org/wiki/Async/await). More details about
it are in the [Threading chapter](threading.md).

Please note that once you have obtained a reference (explicitly or implicitly) to the
Global State, Cortex.Net will not allow you to create more [SharedState](xref:Cortex.Net.SharedState)
instances to prevent you from mixing the two scenarios:

```csharp
using Cortex.Net;

// This will lazily create SharedState.GlobalState;
var person = new Person();

// this will throw as there is already a single SharedState.
ISharedState sharedState = new SharedState();
```

## Multiple shared state instances.

This pattern is suitable for applications without a UI that are running multiple concurrent
threads or asynchronous execution contexts. For instance a web application where you could have
one `SharedState` per web request or a calculation engine where there is one shared state per
calculation thread. 

To attach observables to the right shared state in this case, you can make use of the
[Observable&lt;T&gt;](xref:Cortex.Net.Api.SharedStateObservableExtensions.Observable*) extension
method on the correct shared state to scope your observable creation:

```csharp
using Cortex.Net.Api;
using Cortex.Net;

var firstSharedState = new SharedState();
var secondSharedState = new SharedState();

// This is the first person that is created with a reference to firstSharedState
var person1 = firstSharedState.Observable(() => new Person());

// This is the second person that is created with a reference to secondSharedState
var person2 = secondSharedState.Observable(() => new Person());

// this will throw as there are already non global shared states defined.
var sharedState = SharedState.GlobalState;

// this will throw as well as there is no shared state context for person3.
var person3 = new Person();

```

### Integrate with a DI container

Most DI containers provide a way to retrieve scoped instances from the container
that are tied or can be tied to something like the thread context, async context
or a webrequest et cetera. For instance the [DI container from microsoft](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection)
makes it possible to tie a service to a web request like this:

```csharp
using Cortex.Net.Api;
using Cortex.Net;

public void ConfigureServices(IServiceCollection services)
{
    // Add the shared state to the DI container.
    services.AddScoped<ISharedState, SharedState>();

    // Add a PeopleStore to the DI container.
    services.AddScoped<PeopleStore>(s =>
    {
        // resolve the scoped shared state.
        var sharedState = s.GetService<ISharedState>();

        // create a new people store and return it.
        return sharedState.Observable(() => new PeopleStore());
    });
}
```

## ISharedState as a starting point for extension methods.

Cortex.Net defines several extension methods on `ISharedState` that
allow you to create `Observables`, `Actions`, `Reactions`, `Autorun` and `Atoms`.
This ties the instance to that shared state immediately.
For instance, to create an `Autorun` on a person:

```csharp
using Cortex.Net.Api;
using Cortex.Net;

// create a shared state
var sharedState = new SharedState();

// create a new Person observable, attached to the state.
var person = sharedState.Observable(() => new Person());

// autorun on Person changes (and also now for the first time).
sharedState.Autorun(r => Console.WriteLine($"Person changed: {person.FirstName} {person.LastName}"));

// update first and last name in an transaction. Will fire Autorun once again.
sharedState.RunInAction(() =>
{
    person.FirstName = "Cortext";
    person.LastName = "Net";
});
```

### Explicit shared state references in weaved models.

Normally shared state is implicitly passed around as long as you use either
[Cortex.Net.SharedState.GlobalState](xref:Cortex.Net.SharedState.GlobalState) or
create your observables with [Observable&lt;T&gt;](xref:Cortex.Net.Api.SharedStateObservableExtensions.Observable*).

To use constructor arguments in your models to pass around [ISharedState](xref:Cortex.Net.ISharedState) instances,
you can implement [IReactiveObject](xref:Cortex.Net.IReactiveObject) yourself. Do it exactly like this with an
auto-generated public getter and private setter. The weaver will then append the setter with the correct code to
handle Shared State assignment. You must must assign `this.SharedState` in the constructor or you will get 
NullReferenceExceptions.

```csharp
using Cortex.Net.Api;
using Cortex.Net;

[Observable]
public class PersonWeave : IReactiveObject
{
    public PersonWeave(ISharedState sharedState)
    {
        this.SharedState = sharedState;
    }

    public ISharedState SharedState { get; private set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

}
```

Generally explicit constructor arguments are nice for DI, and make your dependencies clear, but it
may give problems with serialization libraries or ui components that require parameterless constructors.
It also saves you a call to [Observable&lt;T&gt;](xref:Cortex.Net.Api.SharedStateObservableExtensions.Observable*)
to implicitly pass the shared state around. Choose wisely.