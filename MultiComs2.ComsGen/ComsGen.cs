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

            _cmdQueueClient = QueueClient.Create(Constants.ComsGenCmd);
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

            var cmd = brokerMsg.GetBody<GenComsCmd>();
            brokerMsg.Complete();

            var nowUtc = DateTime.UtcNow;

            Console.WriteLine("Received: {0} (took {1}, {2})",
                cmd.ComsType,
                (int)((nowUtc - cmd.OrigReqTimestampUtc).TotalMilliseconds),
                (int)((nowUtc - cmd.MessageTimestampUtc).TotalMilliseconds));

            var body = GenerateComs(cmd.ComsType, cmd);

            var comsGenEvent = cmd.CreateComsMsg<ComsGeneratedEvent>();

            comsGenEvent.ComsId = Guid.NewGuid();
            comsGenEvent.CustomerId = cmd.CustomerId;
            comsGenEvent.ComsType = cmd.ComsType;
            comsGenEvent.Body = body;
            comsGenEvent.DocId = Guid.NewGuid();

            var eventMsg = new BrokeredMessage(comsGenEvent);
            eventMsg.Properties["ComsType"] = cmd.ComsType.ToString();

            _eventTopicClient.Send(eventMsg);
        }

        private string GenerateComs(ComsType comsType, GenComsCmd cmd)
        {
            switch(comsType)
            {
                case ComsType.SMS:
                    return string.Format("SMS for {0} {1}", cmd.CustomerId, cmd.BusEventType);
                case ComsType.Email:
                    return string.Format("CustomerId {0} {1} - Email", cmd.CustomerId, cmd.BusEventType);
                default:
                    throw new ApplicationException("Error - Unknown ComsType");
            }
        }
    }
}
