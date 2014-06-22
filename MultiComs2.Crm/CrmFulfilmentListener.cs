using System;
using System.Collections.Generic;
using Microsoft.ServiceBus.Messaging;
using MultiComs2.Common;

namespace MultiComs2.Crm
{
    internal class CrmFulfilmentListener : Thready
    {
        private readonly IDictionary<Guid, ContactRecord> _crmDb;

        private readonly TimeSpan _waitTime = new TimeSpan(TimeSpan.TicksPerSecond);
        private SubscriptionClient _sc;

        public CrmFulfilmentListener(IDictionary<Guid, ContactRecord> crmDb)
            : base("MultiComs2.Crm - Fulfilment")
        {
            _crmDb = crmDb;
        }

        protected override void Init(string[] args)
        {
            VerifySubs(Constants.ComsFulfilledEvent, Constants.ComsAuditSubs, Reset);
            _sc = SubscriptionClient.Create(Constants.ComsFulfilledEvent, Constants.ComsAuditSubs);
        }

        protected override void ThreadLoop()
        {
            var msg = _sc.Receive(_waitTime);
            if (msg == null) 
                return;
            
            var comsFilfilledEvent = msg.GetBody<ComsFulfilledEvent>();
            var now = DateTime.UtcNow;
            msg.Complete();

            Console.WriteLine("Storing Coms Contact Fulfilment Event for Customer {0} {1} - (took {2}, {3})",
                comsFilfilledEvent.ComsId,
                comsFilfilledEvent.Success,
                (int) ((now - comsFilfilledEvent.OrigReqTimestampUtc).TotalMilliseconds),
                (int)((now - comsFilfilledEvent.MessageTimestampUtc).TotalMilliseconds));


            ContactRecord contact;
            if (!_crmDb.TryGetValue(comsFilfilledEvent.ComsId, out contact))
            {
                contact = new ContactRecord
                {
                    ContactId = comsFilfilledEvent.MessageId
                };
            }

            contact.FulfilledUtc = comsFilfilledEvent.MessageTimestampUtc;
            contact.ContactStatus = comsFilfilledEvent.Success ? ContactStatus.ContactFulfilledSuccessfully : ContactStatus.ContactFulfilledFailed;

            _crmDb[comsFilfilledEvent.ComsId] = contact;
        }
    }
}