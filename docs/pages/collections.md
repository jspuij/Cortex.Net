# Collections

Cortex.Net provides several observable implementations of collection interfaces that are interchangable with standard .Net generic collection types. The most important implementations are:

* [ObservableCollection&lt;T&gt;](xref:Cortex.Net.Types.ObservableCollection`1) which implements most collection interfaces, e.g. `IList<T>` and `ICollection<T>`. 
* [ObservableSet&lt;T&gt;](xref:Cortex.Net.Types.ObservableSet`1) which implements `ISet<T>`.
* [ObservableDictionary&lt;TKey, TValue&gt;](xref:Cortex.Net.Types.ObservableDictionary`2) which implements `IDictionary<TKey, TValue>`.

Collections trigger when items are added, removed or replaced. They report observed when items are dereferenced through indexers, enumerators or when e.g. a count is requested. Similarly the `ContainsKey` on a dictionary creates observables, so that adding an item with a certain key can be observed as well.

## Collections

A new observable collection can be created by calling the [Collection&lt;T&gt;](xref:Cortex.Net.Api.SharedStateObservableExtensions.Collection``1(Cortex.Net.ISharedState,IEnumerable{``0},System.String,Cortex.Net.IEnhancer)) method on the shared state. An example:

```csharp
using Cortex.Net.Api;
using Cortex.Net;

var sharedState = SharedState.GlobalState;

// we create the observable collection.
ICollection<int> observableCollection = sharedState.Collection();

// define a method that automatically runs when the Count property or the sum of numbers changes.
// will print "The sum of 0 items is: 0"
sharedState.AutoRun(r => 
{
    Console.WriteLine($"The sum of {observableCollection.Count} items is: {observableCollection.Sum()}");
});

// will print "The sum of 1 items is: 5"
observableCollection.Add(5);

// will print "The sum of 2 items is: 15"
observableCollection.Add(10);

// will print "The sum of 2 items is: 25"
observableCollection[1] = 20;
```

But you can also have a Property on an observable object automatically converted to a collection. This is done through
[weaving](weaving.md). E.g. the following object and collection will be observable:

```
using Cortex.Net.Api;
using Cortex.Net;

[Observable]
public class IntegerStore
{
    public ICollection<int> Integers { get; set; }
}

var store = new IntegerStore();

// define a method that automatically runs when the Count property or the sum of numbers changes.
// will print "The sum of 0 items is: 0"
sharedState.AutoRun(r => 
{
    Console.WriteLine($"The sum of {store.Integers.Count} items is: {store.Integers.Sum()}");
});

// will print "The sum of 1 items is: 5"
store.Integers.Add(5);

// will print "The sum of 2 items is: 15"
store.Integers.Add(10);

// will print "The sum of 2 items is: 25"
store.Integers[1] = 20;

```

Note that any collection property on an observable object that implements one of the interfaces of
[ObservableCollection&lt;T&gt;](xref:Cortex.Net.Types.ObservableCollection`1) will be automatically
weaved to be implemented by `ObservableCollection`. No need to allocate the property, this will be
done automatically in the constructor by the weaver.

## Sets and Dictionaries.

Like observable collections, it's also possible to create observable sets and observable dictionaries.
Observable sets are created by calling the [Set&lt;T&gt;](xref:Cortex.Net.Api.SharedStateObservableExtensions.Set``1(Cortex.Net.ISharedState,IEnumerable{``0},System.String,Cortex.Net.IEnhancer))
method on the shared state. It's also possible to create an `ISet<T>` Property on a class and decorate
the property or class with the [[Observable]](xref:Cortex.Net.Api.ObservableAttribute) attribute.

Finally to use an observable dictionary, create one by calling the 
[Dictionary&lt;TKey, TValue&gt;](xref:Cortex.Net.Api.SharedStateObservableExtensions.Dictionary``2(Cortex.Net.ISharedState,IDictionary{``0,``1},System.String,Cortex.Net.IEnhancer))
extension method. Use an `IDictionary<TKey, TValue>` Property and decorate the property or class
with the [[Observable]](xref:Cortex.Net.Api.ObservableAttribute) attribute.

The following class has all collections initialized as observables by just the attribute:

```
using Cortex.Net.Api;
using Cortex.Net;

[Observable]
public class ObservableCollections
{
    public ICollection<int> Integers { get; set; }

    public ISet<DayOfWeek> DaysOfTheWeek { get; set;}

    public IDictionary<int,string> SomeKeyValueDictionary { get; set; }
}

```
