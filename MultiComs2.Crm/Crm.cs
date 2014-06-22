using System;
using System.Collections.Concurrent;
using System.Linq;
using log4net;

namespace MultiComs2.Crm
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var log = LogManager.GetLogger(typeof (Program));
            log.Info("Starting CRM...");

            var crmDb = new ConcurrentDictionary<Guid, ContactRecord>();

            var crmReqListener = new CrmRequestListener(crmDb);
            crmReqListener.StartUp(args);

            var crmFulfilmentListener = new CrmFulfilmentListener(crmDb);
            crmFulfilmentListener.StartUp(args);
            
            crmReqListener.RunThread();
            crmFulfilmentListener.RunThread();

            while (string.IsNullOrEmpty(Console.ReadLine()))
            {
                Console.WriteLine();
                Console.WriteLine("---------------->");

                foreach (var cr in crmDb.Values
                    .OrderBy(x => x.CustomerId)
                    .ThenBy(x => x.RequestedUtc))
                {
                    Console.WriteLine("{0} - {1}, {2} ({3}ms)", 
                        cr.CustomerId, 
                        cr.ContactId, 
                        cr.ContactStatus,
                        (cr.ContactStatus == ContactStatus.Requested ? TimeSpan.FromMilliseconds(0) : (cr.FulfilledUtc - cr.RequestedUtc)));
                }
                Console.WriteLine("<----------------");
                Console.WriteLine();
            } 

            crmFulfilmentListener.JoinThread();
            crmReqListener.JoinThread();
        }
    }
}