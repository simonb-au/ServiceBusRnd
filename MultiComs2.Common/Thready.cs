using System;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using log4net;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace MultiComs2.Common
{
    public abstract class Thready
    {
        private readonly ILog _log;

        protected Thready(string title)
        {
            _log = LogManager.GetLogger(GetType());
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

        protected virtual void ThreadLoop() {  throw new NotImplementedException(); }

        protected void ParseArgs(IEnumerable<string> args)
        {
            foreach(var arg in args)
            {
                if (arg.StartsWith("#"))
                    continue;

                if (arg.StartsWith("@"))
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

        protected virtual void Run(string[] args)
        {
            StartUp(args);

            RunThread();

            Console.ReadLine();
            JoinThread();

            CleanUp();
        }

        private Thread _mainThread;

        public void JoinThread()
        {
            _running = false;
            _mainThread.Join();
        }

        public void RunThread()
        {
            _running = true;
            _mainThread = new Thread(ThreadMain);
            _mainThread.Start();
        }

        public void StartUp(string[] args)
        {
            Console.Title = _title;

            ParseArgs(args);

            Init(args);

            if (Reset)
                Console.WriteLine("Reset... done.");

            if (InitOnly)
                return;

            Console.WriteLine("Running {0} ...", _title);
        }

        private void ThreadMain()
        {
            ThreadInit();

            while (_running)
                ThreadLoop();

            ThreadCleanUp();
        }

        protected static void VerifyQueue(string queueName, bool reset)
        {
            Console.WriteLine("VerifyQueue Topic - {0} (Reset = {1})", queueName, reset);

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

        protected static void VerifyTopic(string topicName, bool reset)
        {
            Console.WriteLine("Verifying Topic - {0} (Reset = {1})", topicName, reset);

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
            VerifySubs(topicPath, subsName, reset, null);
        }

        protected void VerifySubs(string topicPath, string subsName, bool reset, Filter filter)
        {
            _log.InfoFormat("VerifySubs Topic - {0}, {1} (Reset = {2})", topicPath, subsName, reset) ;
            
            var nsMgr = NamespaceManager.Create();

            var subsExists = nsMgr.SubscriptionExists(topicPath, subsName);

            if (subsExists && reset)
            {
                nsMgr.DeleteSubscription(topicPath, subsName);
                subsExists = false;
            }

            if (subsExists) 
                return;
            
            if (filter != null)
                nsMgr.CreateSubscription(topicPath, subsName, filter);
            else
                nsMgr.CreateSubscription(topicPath, subsName);
        }
    }
}
