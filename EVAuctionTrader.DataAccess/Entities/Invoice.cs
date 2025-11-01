using EVAuctionTrader.BusinessObject.Enums;

namespace EVAuctionTrader.DataAccess.Entities
{
    public class Invoice : BaseEntity
    {
        public Guid OrderId { get; set; }
        public Order Order { get; set; }

        public string InvoiceNumber { get; set; }
        public Guid BuyerId { get; set; }
        public Guid SellerId { get; set; }

        public decimal Subtotal { get; set; }
        public decimal PlatformFee { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public InvoiceStatus Status { get; set; }
        public DateTime IssuedAt { get; set; }
        public DateTime? PaidAt { get; set; }

        public ICollection<Payment> Payments { get; set; }
    }
}
