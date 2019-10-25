using Cortex.Net;
using Cortex.Net.Spy;
using Cortex.Net.Core;
using System;

namespace Cortext.Next.Playground
{
    class Program
    {
        static void Main(string[] args)
        {
            var sharedState = new SharedState(new CortexConfiguration()
            {
                
            });

            sharedState.SpyEvent += SharedState_SpyEvent;

            var person = new Person(sharedState);

            sharedState.Reaction<string>(r => person.FullName, (s, r) => Console.WriteLine($"Fullname Changed: {s}"));

            person.FirstName = "Jan-Willem";
            person.LastName = "Spuij";

            Console.WriteLine(person.FullName);

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
