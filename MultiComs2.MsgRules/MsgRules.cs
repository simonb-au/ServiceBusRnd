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
            log4net.Config.XmlConfigurator.Configure(new FileInfo("log4net.config"));
            var p = new Program();
            p.Run(args);
        }

        private Program()
            : base("MultiComs2 - MsgRules")
        {
        }

        private static IDictionary<string, IDictionary<BusEventType, ComsType>> LoadCustPrefs()
        {
            var custPrefs = new Dictionary<string, IDictionary<BusEventType, ComsType>>();
            using (var f = File.OpenText("CustPrefs.csv"))
            {
                string line;
                while ((line = f.ReadLine()) != null)
                {
                    var vals = line.Split(',');

                    var custPrefList = new Dictionary<BusEventType, ComsType>();

                    for (var idx = 1; idx < vals.Length; idx += 2)
                    {
                        custPrefList.Add(
                            (BusEventType)Enum.Parse(typeof(BusEventType), vals[idx]),
                            (ComsType)Enum.Parse(typeof(ComsType), vals[idx+1]));
                    }


                    custPrefs.Add(vals[0], custPrefList);
                }
            }

            return custPrefs;
        }
        private IDictionary<string, IDictionary<BusEventType, ComsType>> _custPrefs;

        private SubscriptionClient _subsClient;
        private QueueClient _comsCmdQueue;

        private readonly TimeSpan _receiveWait = new TimeSpan(0, 0, 1);

        protected override void Init(string[] args)
        {
            _custPrefs = LoadCustPrefs();
            Thread.Sleep(1000); // Start-up Time...

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

            var msg = brokerMsg.GetBody<BusEvent>();
            brokerMsg.Complete();

            var nowUtc = DateTime.UtcNow;

            Log.InfoFormat("Received: {0} {1} ({2}, Proc {3}, took {4}, {5})",
                msg.BusEventType,
                msg.OrigReqTimestampUtc.ToLocalTime().ToLongTimeString(),
                msg.OrigRequestSeq,
                msg.ReqProcCount,
                (int)((nowUtc - msg.OrigReqTimestampUtc).TotalMilliseconds),
                (int)((nowUtc - msg.MessageTimestampUtc).TotalMilliseconds));

            var genComsCmd = msg.CreateComsMsg<GenComsCmd>();

            genComsCmd.CustomerId = msg.CustomerId;
            genComsCmd.BusEventType = msg.BusEventType;

            // Calc coms pref
            IDictionary<BusEventType, ComsType> custPrefs;
            if ((!_custPrefs.TryGetValue(msg.CustomerId, out custPrefs)) ||
                (!custPrefs.TryGetValue(msg.BusEventType, out genComsCmd.ComsType)))
            {
                genComsCmd.ComsType = ComsType.Email;
            }

            _comsCmdQueue.Send(new BrokeredMessage(genComsCmd));
        }
    }
}
