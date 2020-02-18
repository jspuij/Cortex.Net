# Observable

Observability in Cortex.Net means that some value can report that it has changed to an observer. 
It can also report that it became observed, or that it lost al observers.
Observables can be properties, objects, and collections like lists, dictionaries, sets.
To fully understand how Cortex.Net will achieve observability, we will first create an observable object by hand:

```csharp
using Cortex.Net.Types;

public class Person : ObservableObject
{
    public Person()
    {
        this.AddObservableProperty<string>(nameof(FirstName));
        this.AddObservableProperty<string>(nameof(LastName));
	}

    public string FirstName
    {
        get => this.Read<string>(nameof(FirstName));
        set => this.Write(nameof(FirstName), value);
	}
    public string LastName
    {
        get => this.Read<string>(nameof(LastName));
        set => this.Write(nameof(LastName), value);
	}
}
```

Notice that it inherits from [ObservableObject](xref:Cortex.Net.Types.ObservableObject). This will give your
object observable superpowers like the ability to report changes to observers and to report that it is being
observed during tracking. To implement properties on your object, you do not read from, or write to a backing
field directly, but you use the [Read](xref:Cortex.Net.Types.ObservableObject.Read``1(System.String)) or
[Write](xref:Cortex.Net.Types.ObservableObject.Write``1(System.String,``0)) methods.

However this is a lot of boilerplate code. And it will become even worse if you want to favor [composition
over inheritance](https://en.wikipedia.org/wiki/Composition_over_inheritance) and don't want all your
objects to inherit from [ObservableObject](xref:Cortex.Net.Types.ObservableObject).

Luckily Cortex.Net can come to the rescue and convert all your objects to observable ones during build.
After compilation Cortex.Net will transform the code below to more or less the code above (Cortex.Net uses an
encapsulated, inner [ObservableObject](xref:Cortex.Net.Types.ObservableObject) instance and forwards
the [IReactiveObject](xref:Cortex.Net.IReactiveObject) interface):

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

The [[Observable]](xref:Cortex.Net.Api.ObservableAttribute) attribute is applied to an auto-implemented property to
signal Cortex.Net that it should be converted to an observable value. If you want all your auto-implemented properties
to be converted to observable values, you can apply the [[Observable]](xref:Cortex.Net.Api.ObservableAttribute)
attribute to a class as well. The final code thus will be:

```csharp
using Cortex.Net.Api;

[Observable]
public class Person
{
    public string FirstName { get; set; }

    public string LastName { get; set; }
}
```

Not bad for all that functionality right?

## Conventions

There are a few simple rules to remember while working with the [[Observable]](xref:Cortex.Net.Api.ObservableAttribute)
attribute:

- It either applies to an 
  [auto-implemented property](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/auto-implemented-properties)
  or a class. In the latter case all auto-implemented properties will be converted to observables.
- It will convert IList&lt;T&gt;, ICollection&lt;T&gt;, IReadonlyCollection&lt;T&gt; interface properties to
  [ObservableCollection&lt;T&gt;](xref:Cortex.Net.Types.ObservableCollection`1).
- It will convert ISet&lt;T&gt; interface properties to [ObservableSet](xref:Cortex.Net.Types.ObservableSet).
- It will convert IDictionary&lt;TKey,TValue&gt; interface properties to
  [ObservableDictionary](xref:Cortex.Net.Types.ObservableDictionary).
- It cannot convert arrays and it will not convert collection properties that are declared as a concrete type instead
  of an instance.

These rules might seem complicated at first sight, but you will notice that in practice they are very intuitive to work with.
Some notes:

- For more information about observable collections see [Collections](collections.md).
- By default the observable will carry the same name as the name of the property. You can override the name using
  the name parameter on the [[Observable]](xref:Cortex.Net.Api.ObservableAttribute) attribute.
- By default, making a datastructure observable is _infective_; that means that [[Observable]](xref:Cortex.Net.Api.ObservableAttribute)
  attribute is applied automatically to any complex type that is encountered on a property.
  This behavior can be changed by specifying another IEnhancer implementation as parameter to the 
  [[Observable]](xref:Cortex.Net.Api.ObservableAttribute). 
  Notably [DeepEnhancer](xref:Cortex.Net.Types.DeepEnhancer), [ShallowEnhancer](xref:Cortex.Net.Types.ShallowEnhancer),
  and [ReferenceEnhancer](xref:Cortex.Net.Types.ReferenceEnhancer) can be specified. 
