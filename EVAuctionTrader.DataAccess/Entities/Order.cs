using EVAuctionTrader.BusinessObject.Enums;

namespace EVAuctionTrader.DataAccess.Entities
{
    public class Order : BaseEntity
    {
        public Guid? ListingId { get; set; }
        public Listing Listing { get; set; }

        public Guid? AuctionId { get; set; }
        public Auction Auction { get; set; }

        public Guid BuyerId { get; set; }
        public User Buyer { get; set; }

        public Guid SellerId { get; set; }
        public User Seller { get; set; }

        public OrderType OrderType { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal PlatformFee { get; set; }
        public OrderStatus Status { get; set; }

        public ICollection<Invoice> Invoices { get; set; }
        public ICollection<Review> Reviews { get; set; }
        public ICollection<Dispute> Disputes { get; set; }
    }
}
