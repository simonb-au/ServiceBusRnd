using System;
using System.Diagnostics;

using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

using MultiComs2.Common;

namespace MultiComs2.EmailFulfilment
{
    class Program : Thready
    {
        static void Main(string[] args)
        {
            var program = new Program();
            program.Run(args);
        }

        public Program()
            : base("MultiComs2.EmailFulfilment")
        {
        }

        private SubscriptionClient _sc;
        private readonly TimeSpan _waitTime = new TimeSpan(TimeSpan.TicksPerSecond);

        protected override void Init(string[] args)
        {
            var nsMgr = NamespaceManager.Create();

            var exists = nsMgr.SubscriptionExists(Constants.ComsGendEvent, Constants.ComsEmailFulfilmentSubs);

            if (Reset && exists)
            {
                nsMgr.DeleteSubscription(Constants.ComsGendEvent, Constants.ComsEmailFulfilmentSubs);
                exists = false;
            }

            if (!exists)
                nsMgr.CreateSubscription(Constants.ComsGendEvent, Constants.ComsEmailFulfilmentSubs, new SqlFilter("ComsType='Email'"));

            _sc = SubscriptionClient.Create(Constants.ComsGendEvent, Constants.ComsEmailFulfilmentSubs);

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

                    Console.WriteLine("Sending Email To Customer {0} (took {1}ms) {2}",
                        comsGenEvent.CustomerId,
                        (int)((now - comsGenEvent.Timestamp).TotalMilliseconds),
                        comsGenEvent.ComsType);
                }
                catch (System.Runtime.Serialization.SerializationException ex)
                {
                    Debug.WriteLine(ex.GetType().Name + ": " + ex.Message);
                }
            }
        }
    }
}
