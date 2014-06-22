using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiComs2.Common
{
    public class GenComsCmd : ComsMsg
    {
        public BusEventType BusEventType;
        public ComsType ComsType;
        public string CustomerId;
    }
}
