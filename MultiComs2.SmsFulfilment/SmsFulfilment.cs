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
        }

        private SubscriptionClient _sc;
        private readonly TimeSpan _waitTime = new TimeSpan(TimeSpan.TicksPerSecond);

        protected override void Init(string[] args)
        {
            VerifySubs(Constants.ComsGendEvent, Constants.ComsSmsFulfilmentSubs, Reset, new SqlFilter("ComsType='SMS'"));
            _sc = SubscriptionClient.Create(Constants.ComsGendEvent, Constants.ComsSmsFulfilmentSubs);
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
                        (int)((now - comsGenEvent.Timestamp).TotalMilliseconds),
                        comsGenEvent.ComsType,
                        now,
                        comsGenEvent.Timestamp);
                }
                catch(System.Runtime.Serialization.SerializationException ex)
                {
                    Debug.WriteLine(ex.GetType().Name + ": " + ex.Message);
                }
            }
        }
    }
}
