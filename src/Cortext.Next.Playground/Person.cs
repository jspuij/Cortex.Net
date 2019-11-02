using Cortex.Net;
using Cortex.Net.Api;
using Cortex.Net.Core;
using Cortex.Net.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cortext.Next.Playground
{
    public class Person : IObservableObject
    {
        private readonly ObservableObject observableObject;
        private Action<string, string> testAction;

        public Person(SharedState sharedState)
        {


            observableObject = new ObservableObject(nameof(Person),sharedState.ReferenceEnhancer(), sharedState);
            observableObject.AddObservableProperty<string>(nameof(FirstName));
            observableObject.AddObservableProperty<string>(nameof(LastName));

            string getter() => this.FirstName + " " + this.LastName;

            observableObject.AddComputedMember<string>(nameof(FullName), new ComputedValueOptions<string>(getter, nameof(FullName))
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
                return this.observableObject.Read<string>(nameof(FirstName));
            }
            set
            {
                this.observableObject.Write(nameof(FirstName), value);
            }
        }

        public string LastName
        {
            get
            {
                return this.observableObject.Read<string>(nameof(LastName));
            }
            set
            {
                this.observableObject.Write(nameof(LastName), value);
            }
        }

        public string FullName => this.observableObject.Read<string>(nameof(FullName));

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
