namespace EVAuctionTrader.DataAccess.Entities
{
    public class Review : BaseEntity
    {
        public Guid OrderId { get; set; }
        public Order Order { get; set; }

        public Guid ReviewerId { get; set; }
        public Guid RevieweeId { get; set; }

        public int Rating { get; set; }
        public string Comment { get; set; }
    }
}
