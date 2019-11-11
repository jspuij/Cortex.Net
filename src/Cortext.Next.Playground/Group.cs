using Cortex.Net;
using Cortex.Net.Api;
using Cortex.Net.Core;
using Cortex.Net.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cortext.Next.Playground
{
    [Observable]
    public class Group
    {
        public ICollection<PersonWeave> People { get; }

        [Computed]
        public Double Average
        {
            get => this.People.Count > 0 ? this.People.Average(x => x.Age) : 0.0d;
        }
    }
}
