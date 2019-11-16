# Concepts & Principles

## Concepts

Cortex.Net distinguishes the following concepts in your application. You saw them in the previous gist, but let's dive into them in a bit more detail.

### 1. State

_State_ is the data that drives your application.
Usually there is _domain specific state_ like a list of todo items and there is _view state_ such as the currently selected element.
Remember, state is like spreadsheets cells that hold a value.

### 2. Derivations

_Anything_ that can be derived from the _state_ without any further interaction is a derivation.
Derivations exist in many forms:

-   The _user interface_.
-   _Derived data_, such as the number of todos left.
-   _Backend integrations_ like sending changes to the server.

Cortex.Net distinguishes two kind of derivations:

-   _Computed values_. These are values that can always be derived from the current observable state using a pure function. A pure function is a function where its result is dependent on its arguments and does not produce side effects.
-   _Reactions_. Reactions are side effects that need to happen automatically if the state changes. These are needed as a bridge between imperative and reactive programming. Or to make it more clear, they are ultimately needed to achieve I/O
People starting with Cortex.Net tend to use reactions too often.
The golden rule is: if you want to create a value based on the current state, use `computed`.

Back to the spreadsheet analogy, formulas are derivations that _compute_ a value. But for you as a user to be able to see it on the screen a _reaction_ is needed that repaints part of the GUI.

### 3. Actions

An _action_ is any piece of code that changes the _state_. User events, backend data pushes, scheduled events, etc.
An action is like a user that enters a new value in a spreadsheet cell.

Actions can be defined explicitly in Cortex.Net to help you to structure code more clearly.
If Cortex.Net is used in [strict mode](xref:Cortex.Net.CortexConfiguration.EnforceActions), Cortex.Net will enforce that no state can be modified outside actions.

## Principles

Cortex.Net supports a uni-directional data flow where _actions_ change the _state_, which in turn updates all affected _views_.

![Action, State, View](../images/action-state-view.png)

All _Derivations_ are updated **automatically** and **atomically** when the _state_ changes. As a result it is never possible to observe intermediate values.

All _Derivations_ are updated **synchronously** by default. This means that, for example, _actions_ can safely inspect a computed value directly after altering the _state_.

_Computed values_ are updated **lazily**. Any computed value that is not actively in use will not be updated until it is needed for a side effect (I/O).
If a view is no longer in use it will be garbage collected automatically.

All _Computed values_ should be **pure**. They are not supposed to change _state_.

## Illustration

The following listing illustrates the above concepts and principles:

```csharp
using Cortex.Net.Api;

[Observable]
public class TodoStore
{
    public IList<Todo> Todos { get; private set; }

    [Computed]
    public int CompletedCount => this.Todos.Count(x => x.Completed);    

    public TodoStore()
    {
        /* a delegate that observes the state */
        sharedState.Autorun(() => {
            Console.WriteLine($"Completed {todos.ActiveCount} of {todos.Count} items.");
        });
    }

    /* ..and some actions that modify the state */
    [Action]
    public void First()
    {
        Todos.Add(new Todo()
        {
            Title = "Take a walk",
            Completed = false,
        });
        // -> synchronously prints 'Completed 0 of 1 items.'
    }
    
    [Action]
    public void Second()
    {
        Todo.Todos[0].Completed = true;
        // -> synchronously prints 'Completed 1 of 1 items.'
    }
}
```
