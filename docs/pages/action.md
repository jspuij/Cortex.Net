# action

Any application has actions. Actions are anything that modify the state.
With Cortex.Net you can make it explicit in your code where your actions live by marking them.
Actions help you to structure your code better. Actions in Cortex.Net achieve three things:

 * They batch mutations and only run reactions after the action finishes.
 * They stop tracking of relations for the duration of the action.
 * They allow mutation of the state only inside the action (or more importantly prevent mutation outside an
 action) when [EnforceActions](xref:Cortex.Net.CortexConfiguration.EnforceActions) is set to `Always` or `Observed`. 

## action as a delegate

It takes a delegate and returns a delegate with the same signature, but wrapped with [`Transaction`](xref:Cortex.Net.Api.SharedStateTransactionExtensions.Transaction(Cortex.Net.ISharedState,Action)), [`Untracked`](xref:Cortex.Net.Core.ActionExtensions.Untracked``1(Cortex.Net.ISharedState,Func{``0})), and [`AllowStateChanges`](xref:Cortex.Net.Core.ActionExtensions.Untracked(Cortex.Net.ISharedState,Action)).
Especially the fact that `Transaction` is applied automatically yields great performance benefits;
actions will batch mutations and only notify computed values and reactions after the (outer most) action has finished.
This makes sure intermediate or incomplete values produced during an action are not visible to the rest of the application until the action has finished.

Example:

```csharp

var myAction = sharedState.Action(() => {
    todos.Add(new Todo() { Title = "Get Coffee" });
    todos.Add(new Todo() { Title = "Write Code" });
    todos[0].Completed = true;
});

myAction();

```

## action as an atrribute

It is advised to use an [[Action]](xref:Cortex.Net.Api.ActionAttribute) attribute on any method that modifies observables or has side effects.

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

Using the `Action` attributes with property setters is not supported; however, setters of [computed properties are automatically actions](computed.md).

Note: using `Action` is mandatory when Cortex.Net is configured to require actions to make state changes, see [EnforceActions](xref:Cortex.Net.CortexConfiguration.EnforceActions).

## When to use actions?

Actions should only, and always, be used on methods that _modify_ state.
Methods that just perform look-ups, filters etc should _not_ be marked as actions; to allow Cortex.Net to track their invocations.

[EnforceActions](xref:Cortex.Net.CortexConfiguration.EnforceActions) enforces that all state modifications are done by an action. This is a useful best practice in larger, long term code bases.

## RunInAction

[RunInAction](xref:Cortex.Net.Api.ActionExtensions.RunInAction(Cortex.Net.ISharedState,System.String,System.Object,Action))is a simple utility that takes an code block and executes in an (anonymous) action. This is useful to create and execute actions on the fly, for example inside an asynchronous process. `RunInAction(f)` is sugar for `Action(f)()`
