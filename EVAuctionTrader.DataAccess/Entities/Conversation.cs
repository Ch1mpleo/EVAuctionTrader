namespace EVAuctionTrader.DataAccess.Entities
{
    public class Conversation : BaseEntity
    {
        public Guid PostId { get; set; }
        public Guid SellerId { get; set; }
        public Guid BuyerId { get; set; }
        public string Status { get; set; }

        public Post Post { get; set; }
        public User Seller { get; set; }
        public User Buyer { get; set; }

        public ICollection<Message> Messages { get; set; }
    }
}
