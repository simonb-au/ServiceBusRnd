using System;

namespace MultiComs2.Crm
{
    public enum ContactStatus
    {
        Requested,
        ContactFulfilledSuccessfully,
        ContactFulfilledFailed
    }

    public class ContactRecord
    {
        public Guid ContactId { get; set; }
        public string CustomerId { get; set; }
        public Guid DocId { get; set; }
        public DateTime RequestedUtc { get; set; }

        public ContactStatus ContactStatus { get; set; }
        public DateTime? FulfilledUtc { get; set; }
    }
}