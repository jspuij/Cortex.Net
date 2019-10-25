using Cortex.Net;
using Cortex.Net.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cortext.Next.Playground
{
    public class Person
    {
        private readonly IAtom firstNameAtom;
        private readonly IAtom lastNameAtom;
        private readonly ComputedValue<string> fullnameComputedValue;

        private string firstName;
        private string lastName;


        public Person(SharedState sharedState)
        {
            firstNameAtom = sharedState.CreateAtom("firstName");
            lastNameAtom = sharedState.CreateAtom("lastName");

            Func<string> getter = () => this.FirstName + " " + this.LastName;

            fullnameComputedValue = new ComputedValue<string>(sharedState, new ComputedValueOptions<string>(getter, "FullName")
            {
                Context = this,
                KeepAlive = true,
            });

        }

        public string FirstName
        {
            get
            {
                firstNameAtom.ReportObserved();
                return firstName;
            }
            set
            {
                firstName = value;
                firstNameAtom.ReportChanged();
            }
        }

        public string LastName
        {
            get
            {
                lastNameAtom.ReportObserved();
                return lastName;
            }
            set
            {
                lastName = value;
                lastNameAtom.ReportChanged();
            }
        }

        public string FullName => this.fullnameComputedValue.Value;
    }
}
