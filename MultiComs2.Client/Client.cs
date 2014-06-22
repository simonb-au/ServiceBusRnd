using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

using MultiComs2.Common;

namespace MultiComs2.Client
{
    class Program : Thready
    {
        static void Main(string[] args)
        {
            var p = new Program();
            p.Run(args);
        }

        private readonly Random _rnd = new Random();
        private readonly int _eventTypeCount = Enum.GetValues(typeof(BusEventType)).Length;

        TopicClient _topicClient;
        private int _eventCounter;

        public Program()
            : base("MultiComs2 - Client")
        {
        }

        protected override void Init(string[] args)
        {
            Console.WriteLine("Starting.");
            VerifyTopic(Constants.BusEvent, Reset);

            if (args.Contains("pause", StringComparer.OrdinalIgnoreCase))
            {
                Console.WriteLine("Ready?");
                Console.ReadLine();
            }

            _topicClient = TopicClient.Create(Constants.BusEvent);

            _eventCounter = 0;
        }

        protected override void CleanUp()
        {
            _topicClient.Close();
            Console.WriteLine("Done.");
        }

        protected override void ThreadLoop()
        {
            Console.WriteLine("Event {0} ...", _eventCounter++);

            var msg = UtilExt.CreateComsMsg<BusEvent>(_eventCounter);

            msg.BusEventType = (BusEventType)_rnd.Next(_eventTypeCount);
            msg.CustomerId = _rnd.Next(10) > 5 ? "123" : "456";

            var sendMessage = new BrokeredMessage(msg);
            _topicClient.Send(sendMessage);

            Thread.Sleep(_rnd.Next(1500));
        }
    }
}
