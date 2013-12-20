using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiComs2.Common
{
    public class ComsGeneratedEvent : ComsMsg
    {
        public string Body;
        public ComsType ComsType;
        public string CustomerId;
        public Guid DocId;
    }
}
