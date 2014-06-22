using System;
using System.Collections.Generic;
using Microsoft.ServiceBus.Messaging;
using MultiComs2.Common;

namespace MultiComs2.Crm
{
    internal class CrmRequestListener : Thready
    {
        private readonly IDictionary<Guid, ContactRecord> _crmDb;

        private readonly TimeSpan _waitTime = new TimeSpan(TimeSpan.TicksPerSecond);
        private SubscriptionClient _sc;

        public CrmRequestListener(IDictionary<Guid, ContactRecord> crmDb)
            : base("MultiComs2.Crm")
        {
            _crmDb = crmDb;
        }

        protected override void Init(string[] args)
        {
            VerifySubs(Constants.ComsGendEvent, Constants.ComsAuditSubs, Reset);
            _sc = SubscriptionClient.Create(Constants.ComsGendEvent, Constants.ComsAuditSubs);
        }

        protected override void ThreadLoop()
        {
            var msg = _sc.Receive(_waitTime);
            if (msg == null) 
                return;
            
            var comsGenEvent = msg.GetBody<ComsGeneratedEvent>();
            var now = DateTime.UtcNow;
            msg.Complete();

            Console.WriteLine("Storing Coms Contact Request Event for Customer {0} - {1} {2} (took {3}, {4})",
                comsGenEvent.ComsId,
                comsGenEvent.CustomerId,
                comsGenEvent.ComsType,
                (int) ((now - comsGenEvent.OrigReqTimestampUtc).TotalMilliseconds),
                (int)((now - comsGenEvent.MessageTimestampUtc).TotalMilliseconds));
            
            SaveComsReq(comsGenEvent);
        }

        private void SaveComsReq(ComsGeneratedEvent comsGenEvent)
        {
            ContactRecord contact;
            var newRec = false;
            if (!_crmDb.TryGetValue(comsGenEvent.ComsId, out contact))
            {
                contact = new ContactRecord
                {
                    ContactId = comsGenEvent.MessageId
                };
                newRec = true;
            }

            contact.CustomerId = comsGenEvent.CustomerId;
            contact.DocId = comsGenEvent.DocId;


            if (newRec)
                contact.ContactStatus = ContactStatus.Requested;
            contact.RequestedUtc = comsGenEvent.OrigReqTimestampUtc;

            _crmDb[comsGenEvent.ComsId] = contact;
        }
    }
}