using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiComs2.Common
{
    public class ComsMsg
    {
        public int OrigRequestSeq { get; set; }
        public Guid OrigRequestId { get; set; }
        public DateTime OrigReqTimestampUtc { get; set; }    
        public Guid MessageId { get; set; }
        public DateTime MessageTimestampUtc { get; set; }
        public int ReqProcCount { get; set; }
    }
}
