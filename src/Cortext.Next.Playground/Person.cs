using Cortex.Net;
using Cortex.Net.Api;
using Cortex.Net.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cortext.Next.Playground
{
    public class Person : IObservableObject
    {
        private readonly IAtom firstNameAtom;
        private readonly IAtom lastNameAtom;
        private readonly ComputedValue<string> fullnameComputedValue;
        private Action<string, string> testAction;

        private string firstName;
        private string lastName;


        public Person(SharedState sharedState)
        {
            firstNameAtom = sharedState.CreateAtom("firstName");
            lastNameAtom = sharedState.CreateAtom("lastName");

            string getter() => this.FirstName + " " + this.LastName;

            fullnameComputedValue = new ComputedValue<string>(sharedState, new ComputedValueOptions<string>(getter, "FullName")
            {
                Context = this,
                KeepAlive = true,
            });

            testAction = sharedState.CreateAction("ChangeBothNames", this, new Action<string, string>(this.ChangeBothNames));
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

        private ISharedState sharedState;

        ISharedState IObservableObject.SharedState
        {
            get => sharedState;
            set
            {
                sharedState = value;

                if (value == null)
                {
                    return;
                }

                testAction = sharedState.CreateAction<string, string>("ChangeBothNames", this, ChangeBothNames);
            }
        }

        private int changeBothNamesCount = 0;

        public void ChangeBothNames(string firstName, string lastName)
        {
            if (this.testAction != null && (++changeBothNamesCount %2) == 1)
            {
                this.testAction(firstName, lastName);
                changeBothNamesCount -= 2;
                return;
            }

            this.FirstName = firstName;
            this.LastName = lastName;
        }
    }
}
