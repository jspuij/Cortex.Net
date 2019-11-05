using Cortex.Net.Api;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cortext.Next.Playground
{
    [Observable]
    public class PersonWeave
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public int Counter { get; set; }

        [Computed]
        public string FullName => $"{this.FirstName} {this.LastName}";

        [Computed]
        public string FullName2()
        {
            return $"{this.FirstName} {this.LastName}";
        }

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
