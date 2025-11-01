using EVAuctionTrader.BusinessObject.Enums;

namespace EVAuctionTrader.DataAccess.Entities
{
    public class Payment : BaseEntity
    {
        public Guid InvoiceId { get; set; }
        public Invoice Invoice { get; set; }

        public Guid PayerId { get; set; }
        public Guid PayeeId { get; set; }

        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Provider { get; set; }
        public string ProviderRef { get; set; }
        public PaymentStatus Status { get; set; }
        public DateTime? PaidAt { get; set; }
    }
}
