using Cortex.Net;
using Cortex.Net.Spy;
using Cortex.Net.Core;
using System;
using Cortex.Net.Api;

namespace Cortext.Next.Playground
{
    class Program
    {
        static void Main(string[] args)
        {
            var sharedState = new SharedState(new CortexConfiguration()
            {
                
            });

            //sharedState.SpyEvent += SharedState_SpyEvent;

            var person = new Person(sharedState);

            var d = sharedState.Reaction<string>(r => person.FullName, (s, r) => Console.WriteLine($"Fullname Changed: {s}"));

            person.FirstName = "Jan-Willem";
            person.LastName = "Spuij";

            Console.WriteLine(person.FullName);

//            d.Dispose();

            person.LastName = "Spuijtje";

            person.ChangeBothNames("Eddy", "Tick");

            Console.WriteLine(person.FullName);

            var personWeaver = new PersonWeave();
            ((IObservableObject)personWeaver).SharedState = sharedState;
            personWeaver.ChangeBothNames("Eddy", "Tick");
            Console.WriteLine(personWeaver.FirstName);
            Console.WriteLine(personWeaver.LastName);


        }

        private static void SharedState_SpyEvent(object sender, Cortex.Net.Spy.SpyEventArgs e)
        {
            if (e is ComputedSpyEventArgs)
            {
                Console.WriteLine($"Computed: {(e as ComputedSpyEventArgs).Name}");
            }
            if (e is ReactionStartSpyEventArgs)
            {
                Console.WriteLine($"Reaction started: {(e as ReactionSpyEventArgs).Name}");
            }
            if (e is ReactionEndSpyEventArgs)
            {
                Console.WriteLine($"Reaction ended: {(e as ReactionSpyEventArgs).Name}");
            }
            if (e is ActionStartSpyEventArgs)
            {
                Console.WriteLine($"Action started: {(e as ActionStartSpyEventArgs).Name}");
            }
            if (e is ActionEndSpyEventArgs)
            {
                Console.WriteLine($"Action ended: {(e as ActionEndSpyEventArgs).Name}");
            }
        }
    }
}
