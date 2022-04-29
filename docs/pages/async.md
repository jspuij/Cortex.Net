# Writing asynchronous actions

The [Action method](action.md) only works on synchronous methods. For asynchronous methods it does not work. To
understand why, we have to dive a bit deeper into asynchronous methods and how the compiler generates them.
An asynchronous method in C# is split on the await calls into multiple synchronous parts by the compiler. These
synchronous parts are put into a state machine. The method continues after an await with the next part based on the
state in the state machine. The transformation by the compiler is described in detail in
[this blog post by Sergey Tepliakov](https://devblogs.microsoft.com/premier-developer/dissecting-the-async-methods-in-c/).

Now it's important to understand that as opposed to a normal method the async method has multiple entry and exit points
and that the [Action method](action.md) wound only wrap the first "part" of the async method.

This means that if you have an `async` method, and in that method some more state is changed, those callbacks should be
wrapped in `action` as well! 

Let's start with a basic example:

### Using RunInAction

Although this is clean and explicit, it might get a bit verbose with complex async flows. It illustrates how to modify
state from an asynchronous method nicely.

```csharp
using Cortex.Net;
using Cortex.Net.Api;

var sharedState = SharedState.GlobalState;

[Observable]
public class Store {
    
    public IList<Project> GithubProjects { get; set; }
    
    public string State { get; set; } = "pending"; // "pending" / "done" / "error"
 
    // do not wrap FetchProjects, as it has multiple entry and exit points.
    async Task FetchProjects() {
        //modify the state. We could have wrapped fetchprojects, but this is more clear.
        sharedState.RunInAction(r =>
        {
            this.GithubProjects.Clear();
            this.State = "pending";
        });
        
        try
        {
            // new entry point after the await.
            var projects = await fetchGithubProjectsSomehow();
        } 
        catch (FetchException ex)
        {
            // after the await we're back, so we have to run this in an action.
            sharedState.RunInAction(r =>
            {
                this.State = "error";
            });
            return;
        }   

        // another action.
        sharedState.RunInAction(r =>
        {
                var filteredProjects = somePreprocessing(projects);
                this.GithubProjects.AddRange(filteredProjects);
                this.state = "done";
        }); 
    }
}
```

### Using the action attribute

However, using [Fody weaving](weaving.md) we are able to detect the generated state machine and wrap the right parts
with the [Action method](action.md) automatically. All it takes is to add the
[[Action]](xref:Cortex.Net.Api.ActionAttribute) attribute to the asynchronous method and it will be wrapped in multiple
actions automatically. The code below is completely equivalent to the code above:

```csharp
using Cortex.Net;
using Cortex.Net.Api;

var sharedState = SharedState.GlobalState;

[Observable]
public class Store {
    
    public IList<Project> GithubProjects { get; set; }
    
    public string State { get; set; } = "pending"; // "pending" / "done" / "error"
 
    [Action]
    async Task FetchProjects() {
        this.GithubProjects.Clear();
        this.State = "pending";
        
        try
        {
            // new entry point after the await.
            var projects = await fetchGithubProjectsSomehow();
        } 
        catch (FetchException ex)
        {
            this.State = "error";
        }   

        var filteredProjects = somePreprocessing(projects);
        this.GithubProjects.AddRange(filteredProjects);
        this.state = "done";
    }
}
```

