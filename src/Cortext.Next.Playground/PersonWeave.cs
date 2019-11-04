using Cortex.Net.Api;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cortext.Next.Playground
{
    public class PersonWeave
    {
        [Observable]
        public string FirstName { get; set; }

        [Observable]
        public string LastName { get; set; }

        [Action("PipoNames")]
        public void ChangeBothNames(string firstName, string lastName)
        {
            FirstName = firstName;
            LastName = lastName;
        }

        [Action]
        public void ChangeBothNamesToJohnDoe()
        {
            FirstName = "John";
            LastName = "Doe";
        }
    }
}
