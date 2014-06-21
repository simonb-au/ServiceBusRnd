using System.Threading;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using MultiComs2.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiComs2.SmsFulfilment
{
    class Program : Thready
    {
        static void Main(string[] args)
        {
            var program = new Program();
            program.Run(args);
        }

        public Program()
            : base("MultiComs2.SmsFulfilment")
        {
            _rnd = new Random();
        }

        private SubscriptionClient _sc;
        private readonly TimeSpan _waitTime = new TimeSpan(TimeSpan.TicksPerSecond);

        private TopicClient _tc;
        private readonly Random _rnd;

        protected override void Init(string[] args)
        {
            VerifySubs(Constants.ComsGendEvent, Constants.ComsSmsFulfilmentSubs, Reset, new SqlFilter("ComsType='SMS'"));
            _sc = SubscriptionClient.Create(Constants.ComsGendEvent, Constants.ComsSmsFulfilmentSubs);

            VerifyTopic(Constants.ComsFulfilledEvent, Reset);
            _tc = TopicClient.Create(Constants.ComsFulfilledEvent);
        }

        protected override void ThreadLoop()
        {
            var msg = _sc.Receive(_waitTime);
            if (msg != null)
            {
                try
                {
                    var comsGenEvent = msg.GetBody<ComsGeneratedEvent>();
                    var now = DateTime.UtcNow;
                    msg.Complete();

                    Console.WriteLine("Sending SMS To Customer {0} (took {1}ms) {2} ({3:HH:mm:ss.fff} - {4:hh:mm:ss.fff}",
                        comsGenEvent.CustomerId,
                        (int)((now - comsGenEvent.OrigReqTimestampUtc).TotalMilliseconds),
                        comsGenEvent.ComsType,
                        now,
                        comsGenEvent.OrigReqTimestampUtc);

                    Thread.Sleep(_rnd.Next(100, 2000));

                    var comsFilfilledEvent = new ComsFulfilledEvent
                    {
                        RequestId = comsGenEvent.RequestId,
                        ComsType = comsGenEvent.ComsType,
                        CustomerId = comsGenEvent.CustomerId,
                        FulfilledTimestampUtc = DateTime.UtcNow,
                        ReqProcCount = comsGenEvent.ReqProcCount + 1,
                        ReqSeq = comsGenEvent.ReqSeq,
                        OrigReqTimestampUtc = comsGenEvent.OrigReqTimestampUtc
                    };

                    Console.WriteLine("   ... fulfilled msg");

                    var eventMsg = new BrokeredMessage(comsFilfilledEvent);
                    _tc.Send(eventMsg);
                }
                catch(System.Runtime.Serialization.SerializationException ex)
                {
                    Debug.WriteLine(ex.GetType().Name + ": " + ex.Message);
                }
            }
        }
    }
}
