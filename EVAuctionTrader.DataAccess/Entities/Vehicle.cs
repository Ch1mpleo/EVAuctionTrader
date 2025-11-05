namespace EVAuctionTrader.DataAccess.Entities
{
    public class Vehicle : BaseEntity
    {
        public Guid OwnerId { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public int OdometerKm { get; set; }
        public string ConditionGrade { get; set; }

        public User Owner { get; set; }
    }
}
