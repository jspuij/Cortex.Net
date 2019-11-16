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

Reactions can simply be created using the [`Autorun`](xref:Cortex.Net.Api.SharedStateReactionExtensions.Autorun(Cortex.Net.ISharedState,Action{Cortex.Net.Core.Reaction},Cortex.Net.AutorunOptions)), [`Reaction`](xref:Cortex.Net.Api.SharedStateReactionExtensions.Reaction``1(Cortex.Net.ISharedState,Func{Cortex.Net.Core.Reaction,``0},Action{``0,Cortex.Net.Core.Reaction},Cortex.Net.ReactionOptions{``0})) or [`when`](http://mobxjs.github.io/mobx/refguide/when.html) methods to fit your specific situations.

For example the following `Autorun` prints a log message each time `ActiveCount` changes:

```csharp
using Cortex.Net.Api;

sharedState.Autorun(() => {
    Console.WriteLine($"Tasks left: {todos.ActiveCount}");
});
```

### Blazor components

If you are using Blazor, you can turn your components into reactive components by simply adding the [[Observer]](xref:Cortex.Net.Blazor.ObserverAttribute) attribute from the `Cortex.Net.Blazor` nuget package onto them.

```razor
@using Cortex.Net.Blazor
@using Cortex.Net.BlazorTodo.Stores
@using Cortex.Net.BlazorTodo.Models

@attribute [Observer]
@inject TodoStore TodoStore

  <section class="main">
      <ul class="todo-list">
          @foreach (var todo in this.TodoStore.Todos)
          {
              <li>
                  <input
                      type="checkbox"
                      checked="@todo.Completed"
                      @onchange="(args) => Toggle(todo)"
                  />
                  @todo.Title
              </li>
          }            
      </ul>
      Tasks left: @TodoStore.Todos.ActiveCount
  </section>

@code 
{
    [Action]
    void Toggle(Todo todo)
    {
        todo.Completed != todo.Completed;
    }
}
```
