using System;
using System.Threading;
using System.IO;
using Microsoft.ServiceBus.Messaging;
using MultiComs2.Common;

namespace MultiComs2.EmailFulfilment
{
    class Program : Thready
    {
        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure(new FileInfo("log4net.config"));
            var program = new Program();
            program.Run(args);
        }

        private Program()
            : base("MultiComs2.EmailFulfilment")
        {
            _rnd = new Random();
        }

        private SubscriptionClient _sc;
        private readonly TimeSpan _waitTime = new TimeSpan(TimeSpan.TicksPerSecond);

        private TopicClient _tc;
        private readonly Random _rnd;
        

        protected override void Init(string[] args)
        {
            VerifySubs(Constants.ComsGendEvent, Constants.ComsEmailFulfilmentSubs, Reset, new SqlFilter("ComsType='Email'"));
            _sc = SubscriptionClient.Create(Constants.ComsGendEvent, Constants.ComsEmailFulfilmentSubs);

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
                    var nowUtc = DateTime.UtcNow;
                    msg.Complete();

                    Log.InfoFormat("Sending Email To Customer {0} {1} (took {2}, {3})",
                        comsGenEvent.CustomerId,
                        comsGenEvent.ComsType,
                        (int)((nowUtc - comsGenEvent.OrigReqTimestampUtc).TotalMilliseconds),
                        (int)((nowUtc - comsGenEvent.MessageTimestampUtc).TotalMilliseconds));

                    Thread.Sleep(_rnd.Next(100, 2000));

                    var comsFilfilledEvent = comsGenEvent.CreateComsMsg<ComsFulfilledEvent>();
                    comsFilfilledEvent.ComsId = comsGenEvent.ComsId;
                    comsFilfilledEvent.CustomerId = comsGenEvent.CustomerId;
                    comsFilfilledEvent.Success = (_rnd.Next(100) > 5);

                    Log.InfoFormat("   ... fulfilled msg->{0}", comsFilfilledEvent.Success);

                    var eventMsg = new BrokeredMessage(comsFilfilledEvent);
                    _tc.Send(eventMsg);
                }
                catch (System.Runtime.Serialization.SerializationException ex)
                {
                    Log.Error(ex.GetType().Name + ": " + ex.Message);
                    Log.Error(ex);
                }
            }
        }
    }
}
