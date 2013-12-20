using System;
using System.Collections.Generic;

using MultiComs2.Common;
using Microsoft.ServiceBus.Messaging;
using Microsoft.ServiceBus;

namespace MultiComs2.Crm
{
    class Program : Thready
    {
        static void Main(string[] args)
        {
            var program = new Program();
            program.Run(args);
        }

        public Program()
            : base("MultiComs2.Crm")
        {
        }

        private SubscriptionClient _sc;
        private readonly TimeSpan _waitTime = new TimeSpan(TimeSpan.TicksPerSecond);

        protected override void Init(string[] args)
        {

            var nsMgr = NamespaceManager.Create();

            var subsExists = nsMgr.SubscriptionExists(Constants.ComsGendEvent, Constants.ComsAuditSubs);

            if (subsExists && Reset)
            {
                nsMgr.DeleteSubscription(Constants.ComsGendEvent, Constants.ComsAuditSubs);
                subsExists = false;
            }

            if (!subsExists)
                nsMgr.CreateSubscription(Constants.ComsGendEvent, Constants.ComsAuditSubs);

            _sc = SubscriptionClient.Create(Constants.ComsGendEvent, Constants.ComsAuditSubs);

        }

        protected override void ThreadLoop()
        {
            var msg = _sc.Receive(_waitTime);
            if (msg != null)
            {
                var comsGenEvent = msg.GetBody<ComsGeneratedEvent>();
                var now = DateTime.UtcNow;
                msg.Complete();
    
                Console.WriteLine("Storing Coms Contact Event for Customer {0} - {1} (took {2}ms)", 
                    comsGenEvent.CustomerId, 
                    comsGenEvent.ComsType,
                    (int)((now - comsGenEvent.Timestamp).TotalMilliseconds));
            }
        }
    }
}
