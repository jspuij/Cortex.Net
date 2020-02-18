# Threading

A common myth, like for any event system is that Cortex.Net is multithreaded. It is not, computed values are
updated on access, or at the end of an action, after which reactions are executed. This all happens synchronously.

To maintain integrity of the shared state, reads and updates of observable values should happen sequentially, which
means either on the same thread or with a proper locking mechanism to guard against simultaneous access.
In most applications that have a UI it makes sense to do this on the UI thread.

# Scheduling actions on the UI thread.

Scheduling actions on the UI thread can be done automatically by Cortex.Net or manually. Automatic scheduling
can be achieved by setting the [AutoscheduleActions property](xref:Cortex.Net.CortexConfiguration.AutoscheduleActions)
and a valid [TaskScheduler](xref:Cortex.Net.CortexConfiguration.TaskScheduler) on the 
[CortexConfiguration](xref:Cortex.Net.CortexConfiguration) instance.

An example:

```csharp
using Cortex.Net;
using System.Threading.Tasks;

var sharedState = SharedState.GlobalState;

// Auto schedule actions, run on the UI TaskScheduler for the platform.
sharedState.Configuration.AutoscheduleActions = true;
sharedState.Configuration.TaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();


var buttonName = sharedState.Box("A threaded test");

sharedState.Autorun(r =>
{
	// set the button name.
	this.button1.Text = buttonName;
});

// a convoluted way to update the button from another thread and run it back on the UI thread.
Task.Run(() => sharedState.RunInAction(() =>
{
	buttonName = "Updated";
}));
```

Note that the platform needs to be able to provide a Task Scheduler from the current Synchronization Context.
Not all platforms support this. In case the platform does not support it, you need to assign an
instance of a TaskScheduler to the [TaskScheduler property](xref:Cortex.Net.CortexConfiguration.TaskScheduler).

## Scheduling in a free threading environment.

Free threading environments (like console apps or services) don't ship with a default Synchronization Context
to schedule asynchronous operations on a single threads. To allow synchronization on these kinds of platforms,
you can use Stephen Cleary's [AsyncContext](https://github.com/StephenCleary/AsyncEx/wiki/AsyncContext).

## Scheduling your actions manually.

If a platform does not provide a Scheduler, it might provide some other dispatch functionality to Invoke something
on the UI or render thread. Server side blazor is such an example. To execute something on the Render thread can
be done like this:

```cshtml-razor
@using  Cortex.Net.Blazor
@using  Cortex.Net.Api

// this component is an observer.
@attribute [Observer]

<span>Hello: @(this.Name) </span>

@code
{
    [Observable]
    string Name { get; set;} = "World";

    protected override void OnInitialized()
    {
        Task.Run(async () => 
        {
            // wait a few seconds.
            await Task.Delay(5000);

            // Invoke to make sure we are running on the UI thread.
            await InvokeAsync(Blazor);
        } );
    }

    [Action]
    void Blazor()
    {
        // look ma, no StateHasChanged Required!
        Name = "Blazor";
    }
}
```