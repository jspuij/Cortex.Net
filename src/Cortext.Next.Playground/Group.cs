using Cortex.Net;
using Cortex.Net.Core;
using Cortex.Net.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cortext.Next.Playground
{
    public class Group : IObservableObject
    {
        private readonly ObservableObject observableObject;

        public Group(SharedState sharedState)
        {
            SharedState = sharedState;
            observableObject = new ObservableObject(nameof(Person), sharedState.GetEnhancer(typeof(DeepEnhancer)), sharedState);

            People = new ObservableCollection<PersonWeave>(SharedState, SharedState.DeepEnhancer(), nameof(People));
            observableObject.AddComputedMember(nameof(Average), new ComputedValueOptions<Double>(this.Getter, nameof(Average))
            {
                Context = this,
                KeepAlive = false,
                RequiresReaction = false,
            });

        }

        public ISharedState SharedState { get; set; }

        public ICollection<PersonWeave> People { get; }

        public Double Average
        {
            get => this.observableObject.Read<Double>(nameof(Average));
        }

        private Double Getter()
        {
            return this.People.Count > 0 ? this.People.Average(x => x.Age) : 0.0d;
        }
    }
}
