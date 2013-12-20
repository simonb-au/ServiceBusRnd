using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiComs2.Common
{
    public class BusEvent : ComsMsg
    {
        public BusEventType BusEventType { get; set; }
        public string CustomerId { get; set; }
    }
}
