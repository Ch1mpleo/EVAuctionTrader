namespace EVAuctionTrader.DataAccess.Entities
{
    public class Vehicle : BaseEntity
    {
        public Guid OwnerId { get; set; }
        public User Owner { get; set; }

        public string Brand { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public int OdometerKm { get; set; }
        public string ConditionGrade { get; set; }
        public string LocationCity { get; set; }
        public string LocationCountry { get; set; }

        public ICollection<Listing> Listings { get; set; }
        public ICollection<Auction> Auctions { get; set; }
    }
}
