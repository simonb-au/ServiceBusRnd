using System;
using System.Threading;
using System.Collections.Generic;

using Microsoft.ServiceBus.Messaging;
using MultiComs2.Common;
using System.IO;
using Microsoft.ServiceBus;

namespace MultiComs2.MsgRules
{
    class Program : Thready
    {
        static void Main(string[] args)
        {
            var p = new Program();
            p.Run(args);
        }

        public Program()
            : base("MultiComs2 - MsgRules")
        {
        }

        private IDictionary<string, ComsType> LoadCustPrefs()
        {
            var custPrefs = new Dictionary<string, ComsType>();
            using (var f = File.OpenText("CustPrefs.csv"))
            {
                string line;
                while ((line = f.ReadLine()) != null)
                {
                    var vals = line.Split(',');
                    custPrefs.Add(vals[0], (ComsType)Enum.Parse(typeof(ComsType), vals[1]));
                }
            }

            return custPrefs;
        }
        private IDictionary<string, ComsType> _custPrefs;

        private SubscriptionClient _subsClient;
        private QueueClient _comsCmdQueue;

        private readonly TimeSpan _receiveWait = new TimeSpan(0, 0, 1);

        protected override void Init(string[] args)
        {
            _custPrefs = LoadCustPrefs();
            Thread.Sleep(1000); // Startup Time...

            VerifySubs(Constants.BusEvent, Constants.BusSubs, Reset);
            VerifyQueue(Constants.ComsGenCmd, Reset);

            _subsClient = SubscriptionClient.Create(Constants.BusEvent, Constants.BusSubs);
            _comsCmdQueue = QueueClient.Create(Constants.ComsGenCmd);
        }

        protected override void CleanUp()
        {
            _comsCmdQueue.Close();
            _subsClient.Close();
        }

        protected override void ThreadLoop()
        {
            var brokerMsg = _subsClient.Receive(_receiveWait);
            if (brokerMsg == null)
                return;

            var now = DateTime.UtcNow;

            var msg = brokerMsg.GetBody<BusEvent>();

            brokerMsg.Complete();

            Console.WriteLine("Received: {0} {1} ({2}, Proc {3}, took {4})",
                msg.BusEventType,
                msg.Timestamp.ToLocalTime().ToLongTimeString(),
                msg.ReqSeq,
                msg.ReqProcCount,
                (int)((now - msg.Timestamp).TotalMilliseconds));

            var genComsCmd = new GenComsCmd
            {
                ComsType = ComsType.SMS,
                CustomerId= msg.CustomerId,
                RequestId = msg.RequestId,
                Timestamp = msg.Timestamp,

                ReqSeq = msg.ReqSeq,
                ReqProcCount = msg.ReqProcCount + 1

            };

            if (!_custPrefs.TryGetValue(msg.CustomerId, out genComsCmd.ComsType))
                genComsCmd.ComsType = ComsType.Email;

            _comsCmdQueue.Send(new BrokeredMessage(genComsCmd));
        }
    }
}
