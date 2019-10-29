using Cortex.Net.Api;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cortext.Next.Playground
{
    public class PersonWeave
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        [Action]
        public void ChangeBothNames(string firstName, string lastName)
        {
            FirstName = firstName;
            LastName = lastName;
        }
    }
}
