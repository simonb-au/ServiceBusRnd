using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiComs2.Common
{
    public class ComsFulfilledEvent : ComsMsg
    {
        public ComsType ComsType;
        public string CustomerId;
        public bool Success;
        public DateTime FulfilledTimestampUtc;
    }
}
