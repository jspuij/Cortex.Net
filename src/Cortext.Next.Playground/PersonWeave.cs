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

        [Computed]
        public string FullName
        {
            get => $"{this.FirstName} {this.LastName}".Trim();

            set
            {
                this.FirstName = string.Empty;
                this.LastName = value;
            }
        }

        [Computed]
        public string FullName2()
        {
            return $"{this.FirstName} {this.LastName}".Trim();
        }

        [Action("PipoNames")]
        public void ChangeBothNames(string firstName, string lastName)
        {
            FirstName = firstName;
            LastName = lastName;
        }

        [Action]
        public void ChangeFullNameToBirdseyeview()
        {
            FullName = "Birdseyeview";
        }
    }
}
