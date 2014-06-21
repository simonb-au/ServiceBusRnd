using System;
using System.Collections.Generic;
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

            var crmDb = new Dictionary<Guid, ContactRecord>();

            var crmReqListener = new CrmRequestListener(crmDb);
            crmReqListener.StartUp(args);
            crmReqListener.RunThread();

            var crmFulfilmentListener = new CrmFulfilmentListener(crmDb);
            crmFulfilmentListener.StartUp(args);
            crmFulfilmentListener.RunThread();

            Console.ReadLine();

            crmFulfilmentListener.JoinThread();
            crmReqListener.JoinThread();


            foreach (var cr in crmDb.Values.OrderBy(x => x.CustomerId))
            {
                Console.WriteLine("{0} - {1}, {2}", cr.CustomerId, cr.ContactId, cr.ContactStatus);
            }

            Console.ReadLine();
        }
    }
}