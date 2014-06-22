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
                OrigRequestSeq = reqSeq,
                ReqProcCount = 0,
                MessageId = reqId,
                MessageTimestampUtc = timestampUtc,
                OrigRequestId = reqId,
                OrigReqTimestampUtc = timestampUtc
            };
        }

        public static T CreateComsMsg<T>(this ComsMsg orig)
            where T : ComsMsg, new()
        {
            return new T
            {
                OrigRequestId = orig.OrigRequestId,
                OrigReqTimestampUtc = orig.OrigReqTimestampUtc,
                OrigRequestSeq = orig.OrigRequestSeq,
                ReqProcCount = orig.ReqProcCount + 1,
                MessageId = Guid.NewGuid(),
                MessageTimestampUtc = DateTime.UtcNow,
            };
        }
    }
}
