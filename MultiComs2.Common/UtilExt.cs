using System;

namespace MultiComs2.Common
{
    public static class UtilExt
    {
        public static T CreateComsMsg<T>(int reqSeq)
            where T : ComsMsg, new()
        {
            var reqId = Guid.NewGuid();
            var timestampUtc = DateTime.UtcNow;

            return new T
            {
                ReqSeq = reqSeq,
                ReqProcCount = 0,
                RequestId = reqId,
                ReqTimestampUtc = timestampUtc,
                OrigReqId = reqId,
                OrigReqTimestampUtc = timestampUtc
            };
        }

        public static T CreateComsMsg<T>(this ComsMsg orig)
            where T : ComsMsg, new()
        {
            return new T
            {
                ReqSeq = orig.ReqSeq,
                ReqProcCount = orig.ReqProcCount + 1,
                RequestId = Guid.NewGuid(),
                ReqTimestampUtc = DateTime.UtcNow,
                OrigReqId = orig.OrigReqId,
                OrigReqTimestampUtc = orig.OrigReqTimestampUtc,
            };
        }
    }
}
