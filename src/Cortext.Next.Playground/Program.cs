using Cortex.Net;
using Cortex.Net.Spy;
using Cortex.Net.Core;
using System;
using Cortex.Net.Api;
using System.Diagnostics;
using System.Threading.Tasks;
using System.IO;
using Nito.AsyncEx;
using System.Collections;
using System.Reactive.Linq;
using Cortex.Net.Rx;

namespace Cortext.Next.Playground
{
    public class Program
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Program start.")]
        public static void Main(string[] args)
        {
            AsyncContext.Run(Run);
        }

        private static async Task Run()
        { 
            var sharedState = SharedState.GlobalState;
            sharedState.Configuration.EnforceActions = EnforceAction.Never;

            sharedState.SpyEvent += SharedState_SpyEvent;


            var timer = Observable.Interval(TimeSpan.FromSeconds(1));

            var rx = sharedState.FromObservable(timer, 0);

            var d100 = sharedState.Autorun(r => Console.WriteLine($"Counter: {rx.Value}"));


            var person = new Person(sharedState);

            var d = sharedState.Reaction<string>(r => person.FullName3, (s, r) =>
            {
                r.Trace(TraceMode.Log);
                Console.WriteLine($"Fullname Changed: {s}");
            });

            var d3 = sharedState.Autorun(r => Console.WriteLine($"Autorun Fullname: {person.FullName3}"));

            person.ChangeBothNames("Eddy", "Tick");
            Console.WriteLine(person.FullName3);
            person.ChangeBothNames("Eddy", "Tickie");
            Console.WriteLine(person.FullName3);

            var personWeave = new PersonWeave(sharedState)
            {
                Age = 30
            };

            var d2 = sharedState.Reaction<string>(r => personWeave.FullName, (s, r) =>
            {
                r.Trace(TraceMode.Log);
                Console.WriteLine($"Weaved: FullName Changed: {s}");
            }, new ReactionOptions<string>()
            {
                Delay = 10,
            });

            var test = sharedState.When(
                () => personWeave.FullName == "Dikkertje Dap",
                new WhenOptions()
                {
                    TimeOut = 1000
                });

            var d4 = sharedState.Autorun(r =>
            {
                Console.WriteLine($"Autorun Fullname weaved: {personWeave.FullName}");
                r.Trace(TraceMode.Log);
            });

            personWeave.Trace(x => x.FullName2());

            personWeave.ChangeBothNames("Jan-Willem", "Spuij");
            Console.WriteLine(personWeave.FullName);
            personWeave.ChangeBothNames("Jan-Willem", "Spuijtje");
            Console.WriteLine(personWeave.FullName);
            personWeave.ChangeFullNameToBirdseyeview();
            Console.WriteLine(personWeave.FullName);
            
            await test;
            personWeave.ChangeBothNames("Dikkertje", "Dap");

            await Task.Delay(1000);

            var group = new Group();
            
            var d5 = sharedState.Autorun(r =>
            {
                Console.WriteLine($"Autorun Average: {group.Average}");
                r.Trace(TraceMode.Log);
            });

            var person2 = new PersonWeave(sharedState);
            person2.ChangeBothNames("Claudia", "Pietryga");
            person2.Age = 20;

            group.People.Add(personWeave);
            group.People.Add(person2);

            var program = new Program();
            var task = program.WriteToFileAsync(group, sharedState, new Random().Next(0, 200));

            personWeave.ChangeBothNames("Jan-Willem", "Spuij2");
            await task;

            Console.WriteLine($"Completed: {task.Result}");
        }

        private static void SharedState_SpyEvent(object sender, Cortex.Net.Spy.SpyEventArgs e)
        {
            var type = e.GetType();
            Trace.WriteLine("-------------");
            Trace.WriteLine($"[Spy] Event: {type.Name}");
            foreach (var prop in type.GetProperties())
            {
                object value = prop.GetValue(e);

                if (value is IEnumerable enumerable && !(value is string))
                {
                    int counter = 0;
                    foreach (var val in enumerable)
                    {
                        Trace.WriteLine($"[Spy] {prop.Name}{counter++}: {val}");
                    }
                } else
                {
                    Trace.WriteLine($"[Spy] {prop.Name}: {value}");
                }
            }
        }

        [Action]
        public async Task<int> WriteToFileAsync(Group group, ISharedState sharedState, int count)
        {
            await File.WriteAllTextAsync("output.txt", group.Average.ToString());
            var person3 = new PersonWeave(sharedState);
            person3.ChangeBothNames("Pipo", $"De clown{count}");
            person3.Age = 10;

            await Task.Delay(3000);

            group.People.Add(person3);

            return count + 10;
        }
    }
}
