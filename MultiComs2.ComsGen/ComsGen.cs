using System;
using System.Threading;

using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

using MultiComs2.Common;

namespace MultiComs2.ComsGen
{
    class Program : Thready
    {
        static void Main(string[] args)
        {
            var p = new Program();
            p.Run(args);
        }

        public Program()
            : base("MultiComs2 - ComsGen")
        {
        }

        private QueueClient _cmdQueueClient;
        private TopicClient _eventTopicClient;

        private readonly TimeSpan _serverWait = new TimeSpan(0, 0, 1);

        protected override void Init(string[] args)
        {
            VerifyTopic(Constants.ComsGendEvent, Reset);

            _cmdQueueClient = QueueClient.Create(Constants.GenComsCmd);
            _eventTopicClient = TopicClient.Create(Constants.ComsGendEvent);
        }

        protected override void CleanUp()
        {
            _eventTopicClient.Close();
            _cmdQueueClient.Close();
        }

        protected override void ThreadLoop()
        {
            var brokerMsg = _cmdQueueClient.Receive(_serverWait);
            if (brokerMsg == null)
                return;

            var now = DateTime.UtcNow;

            var msg = brokerMsg.GetBody<GenComsCmd>();

            brokerMsg.Complete();

            Console.WriteLine("Received: {0} (took {1})",
                msg.ComsType,
                (int)((now - msg.Timestamp).TotalMilliseconds));

            var body = GenerateComs(msg.ComsType, msg.CustomerId);

            var comsGenEvent = new ComsGeneratedEvent
            {
                CustomerId = msg.CustomerId,
                ComsType = msg.ComsType,
                RequestId = msg.RequestId,
                Timestamp = msg.Timestamp,
                Body = body,
                DocId = Guid.NewGuid()
            };

            var eventMsg = new BrokeredMessage(comsGenEvent);
            eventMsg.Properties["ComsType"] = msg.ComsType.ToString();

            _eventTopicClient.Send(eventMsg);
        }

        private string GenerateComs(ComsType comsType, string custId)
        {
            switch(comsType)
            {
                case ComsType.SMS:
                    return string.Format("SMS for {0}", custId);
                case ComsType.Email:
                    return string.Format("CustomerId {0} - Email", custId);
                default:
                    throw new ApplicationException("Error - Unknown ComsType");
            }
        }
    }
}
