# Weaving

Using a state library like Cortex.Net, or any other for that matter, will provide you with the benefits of that library,
but it comes at a cost. It will force you to write your state models in a certain way and add boilerplate. The
boilerplate of [INotifyPropertyChanged](https://www.google.com/search?q=inotifypropertychanged+boilerplate) or 
[Redux](https://github.com/reduxjs/redux/issues/2295) is well documented for example.

So the goal is to maximize effectiveness with a minimal amount of code. In the [Introduction](index.md) we have already
seen that we can manually implement observable properties by inheriting from or encapsulating
[ObservableObject](xref:Cortex.Net.Types.ObservableObject). Writing the code for observability for a lot of properties
becomes tedious though:

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

Likewise implementing [actions](action.md) as delegates isn't always what we want. What if we could ask the computer
to write this boilerplate for us? It is impossible to improve upon the pattern above with the standard language features
because it is a [cross cutting concern](https://en.wikipedia.org/wiki/Cross-cutting_concern). We observe the repetition
but it is tied to the name and type of the property, so imposible to further separate or reduce.

This is where [Aspect oriented Programming](https://en.wikipedia.org/wiki/Aspect-oriented_programming) comes into play.
AOP allows you to add code (an advice) at certain locations (pointcuts) at runtime or at compile time. The combination
of advice and pointcut is called an Aspect.

Cortex.Net uses Fody to reduce the boilerplate. Fody is an extensible tool for weaving .NET assemblies.
Fody itself doesn’t do much to the code, it mostly integrates itself into the MSBuild pipeline, but it has a collection
of plugins to actually change it. Cortex.Net comes with its own Fody plugin built-in.

Cortex.Net will add aspects as a post-build-step to the code that will handle tracking property access and executing 
actions transparently. Adding the nuget reference to Cortex.Net will also add Fody and the Cortex.Net plugin as a
post-build step. To enable weaving the FodyWeavers.xml file that is automatically created in the project root needs to
be modified:

## Add to FodyWeavers.xml

To make life easier Cortex.Net supports weaving to create transparent observable state. To do this you need to create a 
FodyWeavers.xml file and add it to your project. Add `<Cortex.Net />` to
[FodyWeavers.xml](https://github.com/Fody/Home/blob/master/pages/usage.md#add-fodyweaversxml)

```xml
<Weavers>
  <Cortex.Net />
</Weavers>
```

## Controlling the pointcuts with attributes.

Pointcuts can be automatically applied, but Cortex.Net has chosen to make this explicit by requiring attributes to be
added to the code at the location where weaving needs to be added:

### Adding observability.

[Observability](observable.md) can be applied by applying the [[Observable]](xref:Cortex.Net.Api.ObservableAttribute)
attribute to auto implemented properties or to classes.

```csharp
using Cortex.Net.Api;

public class Person
{
    [Observable]
    public string FirstName { get; set;}

    [Observable]
    public string LastName { get; set; }
}
```

Applying the attribute to an auto property that implements a collection interface will instantiate the interface with
an observable version of the collection:

```
using Cortex.Net.Api;
using Cortex.Net;

public class IntegerStore
{
    [Observable]
    public ICollection<int> Integers { get; set; }
}

### Creating an action.

Applying the [[Action]](xref:Cortex.Net.Api.ActionAttribute) attribute on any method that modifies observables will
batch the modifications and runs the reactions at the end of the method:

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

## Caveats

Aspect oriented programming has disadvantages: Control flow is obscured, debugging is harder, point cuts of different
aspects might interfere with each other and so in. Make sure the advantages outweigh the disadvantages when using it.
It is perfectly possible to mix Fody enhanced models with manually implemented IReactiveObject / ObservableObject
instances.