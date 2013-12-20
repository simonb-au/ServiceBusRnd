using System;
using System.Linq;
using System.Threading;
using System.IO;

using Microsoft.ServiceBus;


namespace MultiComs2.Common
{
    public abstract class Thready
    {
        public Thready(string title)
        {
            _title = title;
        }

        private readonly string _title;
        private volatile bool _running;

        public bool Reset { get; private set; }
        public bool InitOnly { get; private set; }

        protected virtual void Init(string[] args) { }
        protected virtual void CleanUp() { }
        protected virtual void ThreadInit() { }
        protected virtual void ThreadCleanUp() { }

        protected abstract void ThreadLoop();

        public void ParseArgs(string[] args)
        {
            foreach(var arg in args)
            {
                if (arg.StartsWith("#"))
                    continue;

                else if (arg.StartsWith("@"))
                    ParseArgs(File.ReadAllLines(arg.Substring(1)));

                else if (arg.Equals("Reset", StringComparison.OrdinalIgnoreCase))
                    Reset = true;

                else if (arg.Equals("InitOnly", StringComparison.OrdinalIgnoreCase))
                    InitOnly = true;

                else if (arg.Equals("ResetOnly", StringComparison.OrdinalIgnoreCase))
                {
                    Reset = true;
                    InitOnly = true;
                }

                else 
                    Console.WriteLine("Unknown Argument: {0}", arg);
            }
        }

        protected void Run(string[] args)
        {
            Console.Title = _title;

            ParseArgs(args);

            Init(args);

            if (Reset)
                Console.WriteLine("Reset... done.");

            if (InitOnly)
                return;

            Console.WriteLine("Running {0} ...", _title);

            _running = true;
            var t = new Thread(ThreadMain);
            t.Start();

            Console.ReadLine();
            _running = false;

            t.Join();

            CleanUp();
        }

        protected virtual void ThreadMain()
        {
            ThreadInit();

            while (_running)
                ThreadLoop();

            ThreadCleanUp();
        }

        protected void VerifyQueue(string queueName, bool reset)
        {
            var nsMgr = NamespaceManager.Create();

            var queueExists = nsMgr.QueueExists(queueName);

            if (queueExists && reset)
            {
                nsMgr.DeleteQueue(queueName);
                queueExists = false;
            }

            if (!queueExists)
                nsMgr.CreateQueue(queueName);
        }

        protected void VerifyTopic(string topicName, bool reset)
        {
            var nsMgr = NamespaceManager.Create();

            var exists = nsMgr.TopicExists(topicName);

            if (exists && reset)
            {
                nsMgr.DeleteTopic(topicName);
                exists = false;
            }

            if (!exists)
            {
                nsMgr.CreateTopic(topicName);
            }
        }

        protected void VerifySubs(string topicPath, string subsName, bool reset)
        {
            var nsMgr = NamespaceManager.Create();

            var subsExists = nsMgr.SubscriptionExists(topicPath, subsName);

            if (subsExists && reset)
            {
                nsMgr.DeleteSubscription(topicPath, subsName);
                subsExists = false;
            }

            if (!subsExists)
                nsMgr.CreateSubscription(topicPath, subsName);
        }

    }
}
