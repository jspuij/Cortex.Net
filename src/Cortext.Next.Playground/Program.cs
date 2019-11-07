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

            var personWeaver = new PersonWeave();
            ((IObservableObject)personWeaver).SharedState = sharedState;

            var d2 = sharedState.Reaction<string>(r => personWeaver.FullName, (s, r) =>
            {
                r.Trace(TraceMode.Log);
                Console.WriteLine($"Weaved: FullName Changed: {s}");
            });

            var d4 = sharedState.Autorun(r =>
            {
                Console.WriteLine($"Autorun Fullname weaved: {personWeaver.FullName}");
                r.Trace(TraceMode.Log);
            });

            personWeaver.Trace(x => x.FullName2());

            personWeaver.ChangeBothNames("Jan-Willem", "Spuij");
            personWeaver.ChangeBothNames("Jan-Willem", "Spuijtje");
            personWeaver.ChangeFullNameToBirdseyeview();

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
