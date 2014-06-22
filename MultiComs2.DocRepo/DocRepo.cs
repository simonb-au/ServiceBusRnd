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

        private Program()
            : base("MultiComs2.DocRepo")
        {
        }

        private SubscriptionClient _sc;
        private readonly TimeSpan _waitTime = new TimeSpan(TimeSpan.TicksPerSecond);

        protected override void Init(string[] args)
        {
            VerifySubs(Constants.ComsGendEvent, Constants.ComsStoreSubs, Reset);
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

                Console.WriteLine("Saving Coms Document for Customer {0} - {1}, {2} (took {3}, {4}) {5}",
                    comsGenEvent.CustomerId,
                    comsGenEvent.ComsType,
                    comsGenEvent.DocId,
                    (int)((now - comsGenEvent.OrigReqTimestampUtc).TotalMilliseconds),
                    (int)((now - comsGenEvent.MessageTimestampUtc).TotalMilliseconds),
                    comsGenEvent.Body);
            }
        }
    }
}
