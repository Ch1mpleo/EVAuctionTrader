namespace EVAuctionTrader.DataAccess.Entities
{

    // Người tham gia đấu giá
    public class Bid : BaseEntity
    {
        public Guid AuctionId { get; set; }
        public Guid BidderId { get; set; }
        public decimal Amount { get; set; }

        public Auction Auction { get; set; }
        public User Bidder { get; set; }
    }
}
