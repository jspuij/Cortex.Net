<img src="../logo.svg" alt="Cortex.Net" height=120 align="right">

# Cortex.Net

_State management like [MobX](https://mobx.js.org/README.html) for .NET_

## NuGet installation

To install the main library, install the the [Cortex.Net NuGet package](https://nuget.org/packages/Cortex.Net/). The main library allows you to compose observable reactive state yourself.

```powershell
PM> Install-Package Cortex.Net
```
If you want to install the Blazor bindings, they are in a separate package:

Install the [Cortex.Net.Blazor NuGet package](https://nuget.org/packages/Cortex.Net.Blazor/):

```powershell
PM> Install-Package Cortex.Net.Blazor
```

### Add to FodyWeavers.xml

To make life easier Cortex.Net supports weaving to create transparent observable state. To do this you need to create a FodyWeavers.xml file and add it to your project.
Add `<Cortex.Net />` to [FodyWeavers.xml](https://github.com/Fody/Home/blob/master/pages/usage.md#add-fodyweaversxml)

```xml
<Weavers>
  <Cortex.Net />
</Weavers>
```

## Introduction

Cortex.Net is a library that makes state management simple and scalable by transparently applying [functional reactive programming](https://en.wikipedia.org/wiki/Functional_reactive_programming) (TFRP). It is more or less a direct port of the excellent [MobX](https://mobx.js.org/README.html) library. As C# has Class-based inheritance versus the Prototype-based inheritance model of JavaScript, porting the library introduced some unique challenges. These are mostly solved by [Weaving](https://github.com/Fody/Fody) your library of state objects.

The philosophy behind Cortex.Net is very simple:

_Anything that can be derived from the application state, should be derived. Automatically._

which includes the UI, data serialization, server communication, etc.

[Blazor](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor) and Cortex.Net together are a powerful combination. Blazor renders the application state by providing mechanisms to translate it into a tree of renderable components. Cortex.Net provides the mechanism to store and update the application state that Blazor then uses.

<img alt="Cortex.Net unidirectional flow" src="https://github.com/mobxjs/mobx/raw/master/docs/assets/flow.png" align="center" />

Both Blazor and Cortex.Net provide optimal and unique solutions to common problems in application development. Blazor provides mechanisms to optimally render UI by using a virtual DOM that reduces the number of costly DOM mutations. Cortex.Net provides mechanisms to optimally synchronize application state with your Blazor components by using a reactive virtual dependency state graph that is only updated when strictly needed and is never stale.

## Core concepts

Cortex.Net has only a few core concepts. They will be illustrated in the snippets below:

### Observable state

Cortex.Net adds observable capabilities to existing data structures like objects, collections and class instances.
This can simply be done by annotating your auto-generated properties with the [[Observable]](xref:Cortex.Net.Api.ObservableAttribute) attribute (On a platform that supports weaving).

```csharp
using Cortex.Net.Api;

public class Todo
{
    [Observable]
    public string Title { get; set; }

    [Observable]
    public bool Completed { get; set; }
}
```

Using `observable` is like turning a property of an object into a spreadsheet cell.
But unlike spreadsheets, these values can be not only primitive values, but also references, objects and arrays.

If your environment doesn't support weaving, don't worry, as Cortex.Net can be used fine without attributes, by inheriting from or encapsulating [ObservableObject](xref:Cortex.Net.Types.ObservableObject).
Many Cortex.Net users do prefer the atrribute syntax though, as it is less boilerblate.

```csharp
using Cortex.Net.Types;

public class Todo : ObservableObject
{
    public string Title
    { 
        get => this.Read<string>(nameof(Title)); 
        set => this.Write(nameof(Title), value); 
    }

    public bool Completed
    { 
        get => this.Read<bool>(nameof(Completed)); 
        set => this.Write(nameof(Completed), value); 
    }
}
```
### Computed values

With Cortex.Net you can define values that will be derived automatically when relevant data is modified.
By using the [[Computed]](xref:Cortex.Net.Api.ComputedAttribute) attribute or by using methods or property getters (or even setters) when using [ObservableObject](xref:Cortex.Net.Types.ObservableObject).

```csharp
using Cortex.Net.Api;

[Observable]
public class TodoStore
{
    // The IList<Todo> property is automatically assigned 
    // with an observable collection during weaving.
    public IList<Todo> Todos { get; private set; }

    [Computed]
    public int ActiveCount => this.Todos.Count(x => !x.Completed);    
}
```

Cortex.Net will ensure that `ActiveCount` is updated automatically when a todo is added or when one of the `Completed` properties is modified. Computations like these resemble formulas in spreadsheet programs like MS Excel. They update automatically and only when required.

### Reactions

Reactions are similar to a computed value, but instead of producing a new value, a reaction produces a side effect for things like printing to the console, making network requests, incrementally updating the React component tree to patch the DOM, etc.
In short, reactions bridge [reactive](https://en.wikipedia.org/wiki/Reactive_programming) and [imperative](https://en.wikipedia.org/wiki/Imperative_programming) programming.

Reactions can simply be created using the [Autorun](xref:Cortex.Net.Api.SharedStateReactionExtensions.Autorun(Cortex.Net.ISharedState,Action{Cortex.Net.Core.Reaction},Cortex.Net.AutorunOptions)), [Reaction](xref:Cortex.Net.Api.SharedStateReactionExtensions.Reaction``1(Cortex.Net.ISharedState,Func{Cortex.Net.Core.Reaction,``0},Action{``0,Cortex.Net.Core.Reaction},Cortex.Net.ReactionOptions{``0})) or [when](http://mobxjs.github.io/mobx/refguide/when.html) methods to fit your specific situations.

For example the following `Autorun` prints a log message each time `ActiveCount` changes:

```csharp
using Cortex.Net.Api;

sharedState.Autorun(() => {
    Console.WriteLine($"Tasks left: {todos.ActiveCount}");
});
```

### Blazor components

If you are using Blazor, you can turn your components into reactive components by simply adding the [[Observer]](xref:Cortex.Net.Blazor.ObserverAttribute) attribute from the `Cortex.Net.Blazor` nuget package onto them.

`TodoListView.razor:`
```cshtml
@using Cortex.Net.Blazor
@using Cortex.Net.BlazorTodo.Stores
@using Cortex.Net.BlazorTodo.Models

@attribute [Observer]
@inject TodoStore TodoStore

  <section class="main">
      <ul class="todo-list">
          @foreach (var todo in this.TodoStore.Todos)
          {
             <TodoItem Todo="todo"/> 
          }            
      </ul>
      Tasks left: @TodoStore.Todos.ActiveCount
  </section>
```

`TodoItem.razor:`
```cshtml
  
@using Cortex.Net.Blazor
@using Cortex.Net.Api
@using Cortex.Net.BlazorTodo.Stores
@using Cortex.Net.BlazorTodo.Models

@attribute [Observer]

<li>
    <input
        type="checkbox"
        checked="@Todo.Completed"
        @onchange="Toggle"
    />
    @todo.Title
</li>

@code 
{
    [Parameter]
    public Todo Todo { get; set; }

    [Action]
    void Toggle(ChangeEventArgs args)
    {
        Todo.Completed != Todo.Completed;
    }
}
```

`[Observer]` turns Blazor components into derivations of the data they render. Cortex.Net will make sure the components are always re-rendered whenever needed, but also no more than that. So the `onChange` handler in the above example will force the proper `TodoItem` to render, and it will cause the `TodoListView` to render if the number of unfinished tasks has changed.
However, if you would remove the `Tasks left` line (or put it into a separate component), the `TodoListView` will no longer re-render when ticking a box.

### What will Cortex.Net react to?

Why does a new message get printed or the Blazor component rerendered each time the `ActiveCount` is changed? The answer is this rule of thumb:

Cortex.Net reacts to any existing observable property that is read during the execution of a tracked function._

For an in-depth explanation about how Cortex.Net determines to which observables needs to be reacted, check [understanding what Cortex.Net reacts to](react.md).

### Actions

Unlike many flux frameworks, Cortex.Net is unopinionated about how user events should be handled.

-   This can be done in a Flux like manner.
-   Or by processing events using RxJS.
-   Or by simply handling events in the most straightforward way possible, as demonstrated in the above `onChanged` handler.

In the end it all boils down to: Somehow the state should be updated.

After updating the state Cortex.Net will take care of the rest in an efficient, glitch-free manner. So simple statements, like below, are enough to automatically update the user interface.

There is no technical need for firing events, calling a dispatcher or what more. A Blazor component in the end is nothing more than a fancy representation of your state. A derivation that will be managed by Cortex.Net.

```csharp
todos.Add(new Todo() { Title = "Get Coffee" });
todos.Add(new Todo() { Title = "Write Code" });
todos[0].Completed = true;
```

Nonetheless, Cortex.Net has an optional built-in concept of [`actions`](action.md).
Read this section as well if you want to know more about writing asynchronous actions. It's easy!
Use them to your advantage; they will help you to structure your code better and make wise decisions about when and where state should be modified. The default configuration of Cortex.Net will throw exceptions when observed data is modified outside an action to make sure that you are conscise and do not trigger too many reactions.

```csharp

var myAction = sharedState.Action(() => {
    todos.Add(new Todo() { Title = "Get Coffee" });
    todos.Add(new Todo() { Title = "Write Code" });
    todos[0].Completed = true;
});

myAction();

```

Or with an [[Action]](xref:Cortex.Net.Blazor.ActionAttribute) attribute:

```csharp

[Action]
public void MyAction()
{
todos.Add(new Todo() { Title = "Get Coffee" });
    todos.Add(new Todo() { Title = "Write Code" });
    todos[0].Completed = true;
}

MyAction();
```
## Cortex.Net: Simple and scalable

Cortex.Net is one of the least obtrusive libraries you can use for state management. That makes the `Cortex.Net` approach not just simple, but very scalable as well:

### Using classes and real references

With Cortex.Net you don't need to normalize your data. This makes the library very suitable for very complex domain models.

### Referential integrity guaranteed.

Since data doesn't need to be normalized, and Cortex.Net automatically tracks the relations between state and derivations, you get referential integrity for free. Rendering something that is accessed through three levels of indirection?

No problem, Cortex.Net will track them and re-render whenever one of the references changes. As a result staleness bugs are a thing of the past. As a programmer you might forget that changing some data might influence a seemingly unrelated component in a corner case. Cortex.Net won't forget.

### Simpler actions are easier to maintain

As demonstrated above, modifying state when using Cortex.Net is very straightforward. You simply write down your intentions. Cortex.Net will take care of the rest.

### Fine grained observability is efficient

Cortex.Net builds a graph of all the derivations in your application to find the least number of re-computations that is needed to prevent staleness. "Derive everything" might sound expensive, Cortex.Net builds a virtual derivation graph to minimize the number of recomputations needed to keep derivations in sync with the state.

Secondly Cortex.Net sees the causality between derivations so it can order them in such a way that no derivation has to run twice or introduces a glitch.

How that works? See this [in-depth explanation of MobX](https://medium.com/@mweststrate/becoming-fully-reactive-an-in-depth-explanation-of-mobservable-55995262a254).

### Easy interoperability

Cortex.Net works with POCO objects. Due to its unobtrusiveness it works with most libraries out of the box, without needing Cortex.Net specific library flavors.

For the same reason you can use it out of the box both server and client side, in isomorphic applications and with any IU framework. As long as the runtime supports netstandard2.0, you are good.

The result of this is that you often need to learn less new concepts when using Cortex.Net in comparison to other state management solutions.

---

## Credits

Credit where credit is due and Cortex.Net is entirely based on MobX.

MobX is inspired by reactive programming principles as found in spreadsheets. It is inspired by MVVM frameworks like in MeteorJS tracker, knockout and Vue.js. But MobX brings Transparent Functional Reactive Programming to the next level and provides a stand alone implementation. It implements TFRP in a glitch-free, synchronous, predictable and efficient manner.

A ton of credits for [Mendix](https://github.com/mendix), for providing the flexibility and support to maintain MobX and the chance to proof the philosophy of MobX in a real, complex, performance critical applications.

And finally kudos for all the people that believed in, tried, validated and even [sponsored](https://github.com/mobxjs/mobx/blob/master/sponsors.md) MobX.

To make Cortex.Net possible in .NET in an unobtrusive and transparent way we use IL-weaving from [Fody](https://github.com/Fody/Home). 

## Contributing

-   Feel free to send small pull requests. Please discuss new features or big changes in a GitHub issue first.
-   Use `dotnet test` to run the basic test suite, and make sure your pull request is covered by tests.

