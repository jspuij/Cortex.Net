using Cortex.Net;
using Cortex.Net.Spy;
using Cortex.Net.Core;
using System;
using Cortex.Net.Api;
using System.Diagnostics;

namespace Cortext.Next.Playground
{
    class Program
    {
        static void Main(string[] args)
        {
            var sharedState = new SharedState(new CortexConfiguration()
            {
                EnforceActions = EnforceAction.Never,    
            });
            sharedState.SpyEvent += SharedState_SpyEvent;

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

            var personWeave = new PersonWeave();
            ((IObservableObject)personWeave).SharedState = sharedState;
            personWeave.Age = 30;

            var d2 = sharedState.Reaction<string>(r => personWeave.FullName, (s, r) =>
            {
                r.Trace(TraceMode.Log);
                Console.WriteLine($"Weaved: FullName Changed: {s}");
            });

            var d4 = sharedState.Autorun(r =>
            {
                Console.WriteLine($"Autorun Fullname weaved: {personWeave.FullName}");
                r.Trace(TraceMode.Log);
            });

            personWeave.Trace(x => x.FullName2());

            personWeave.ChangeBothNames("Jan-Willem", "Spuij");
            personWeave.ChangeBothNames("Jan-Willem", "Spuijtje");
            personWeave.ChangeFullNameToBirdseyeview();

            var group = new Group(sharedState);
            var d5 = sharedState.Autorun(r =>
            {
                Console.WriteLine($"Autorun Average: {group.Average}");
                r.Trace(TraceMode.Log);
            });

            var person2 = new PersonWeave();
            ((IObservableObject)person2).SharedState = sharedState;
            person2.ChangeBothNames("Claudia", "Pietryga");
            person2.Age = 20;

            group.People.Add(personWeave);
            group.People.Add(person2);

            d.Dispose();
            d2.Dispose();
        }

        private static void SharedState_SpyEvent(object sender, Cortex.Net.Spy.SpyEventArgs e)
        {
            var type = e.GetType();
            Trace.WriteLine("-------------");
            Trace.WriteLine($"[Spy] Event: {type.Name}");
            foreach (var prop in type.GetProperties())
            {
                Trace.WriteLine($"[Spy] {prop.Name}: {prop.GetValue(e)}");
            }
        }
    }
}
