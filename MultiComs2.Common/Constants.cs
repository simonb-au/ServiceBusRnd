using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiComs2.Common
{
    public static class Constants
    {
        public const string EventQueueName = "MultiComsEvents";
        public const string BusTopicName = "BusSysTopic";
        public const string BusSubs = "BusSubs";

        public const string GenComsCmd = "GenComsCmd";

        public const string ComsGendEvent = "ComsGendEvent";
        public const string ComsAuditSubs = "ComsAudit";
        public const string ComsStoreSubs = "ComsStore";
        public const string ComsSmsFulfilmentSubs = "ComsSmsFulfilmentSubs";
        public const string ComsEmailFulfilmentSubs = "ComsEmailFulfilmentSubs";
    }
}
