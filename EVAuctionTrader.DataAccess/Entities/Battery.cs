namespace EVAuctionTrader.DataAccess.Entities
{
    public class Battery : BaseEntity
    {
        public Guid OwnerId { get; set; }
        public User Owner { get; set; }

        public string Manufacturer { get; set; }
        public string Chemistry { get; set; }
        public decimal CapacityKwh { get; set; }
        public int CycleCount { get; set; }
        public decimal SohPercent { get; set; }
        public decimal VoltageV { get; set; }
        public string ConnectorType { get; set; }
        public string LocationCity { get; set; }
        public string LocationCountry { get; set; }

        public ICollection<Listing> Listings { get; set; }
        public ICollection<Auction> Auctions { get; set; }
    }
}
