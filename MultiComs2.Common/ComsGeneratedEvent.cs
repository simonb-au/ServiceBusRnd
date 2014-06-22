using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiComs2.Common
{
    public class ComsGeneratedEvent : ComsEvent
    {
        public string Body;
        public ComsType ComsType;
        public Guid DocId;
    }
}
