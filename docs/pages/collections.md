# Collections

Cortex.Net provides several observable implementations of collection interfaces that are interchangable with standard .Net generic collection types. The most important implementations are:

* [ObservableCollection&lt;T&gt;](xref:Cortex.Net.Types.ObservableCollection`1) which implements most collection interfaces, e.g. `IList&lt;T&gt;` and `ICollection&lt;T&gt;`. 
* [ObservableSet&lt;T&gt;](xref:Cortex.Net.Types.ObservableSet`1) which implements `ISet&lt;T&gt;`.
* [ObservableDictionary&lt;TKey, TValue&gt;](xref:Cortex.Net.Types.ObservableDictionary`2) which implements `IDictionary&lt;TKey, TValue&gt;`.

Collections trigger when items are added, removed or replaced. They report observed when items are dereferenced through indexers, enumerators or when e.g. a count is requested. Similarly the `ContainsKey` on a dictionary creates observables, so that adding an item with a certain key can be observed as well.

## CreateCollection

A new observable collection can be created by calling the [Collection<&lt;T&gt;>](xref:Cortex.Net.Api.SharedStateObservableExtensions.Collection``1(Cortex.Net.ISharedState,IEnumerable{``0},System.String,Cortex.Net.IEnhancer)) method on the shared state. An example:

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


