using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using MultiComs2.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiComs2.DocRepo
{
    class Program: Thready
    {
        static void Main(string[] args)
        {
            var program = new Program();
            program.Run(args);
        }

        public Program()
            : base("MultiComs2.DocRepo")
        {
        }

        private SubscriptionClient _sc;
        private readonly TimeSpan _waitTime = new TimeSpan(TimeSpan.TicksPerSecond);

        protected override void Init(string[] args)
        {
            var nsMgr = NamespaceManager.Create();

            var subsExists = nsMgr.SubscriptionExists(Constants.ComsGendEvent, Constants.ComsStoreSubs);

            if (subsExists && Reset)
            {
                nsMgr.DeleteSubscription(Constants.ComsGendEvent, Constants.ComsStoreSubs);
                subsExists = false;
            }

            if (!subsExists)
                nsMgr.CreateSubscription(Constants.ComsGendEvent, Constants.ComsStoreSubs);

            _sc = SubscriptionClient.Create(Constants.ComsGendEvent, Constants.ComsStoreSubs);
        }

        protected override void ThreadLoop()
        {
            var msg = _sc.Receive(_waitTime);
            if (msg != null)
            {
                var comsGenEvent = msg.GetBody<ComsGeneratedEvent>();
                var now = DateTime.UtcNow;
                msg.Complete();

                Console.WriteLine("Saving Coms Document for Customer {0} - {1}, {2} (took {3}ms)",
                    comsGenEvent.CustomerId,
                    comsGenEvent.ComsType,
                    comsGenEvent.DocId,
                    (int)((now - comsGenEvent.Timestamp).TotalMilliseconds));
            }
        }
    }
}
