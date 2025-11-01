using EVAuctionTrader.BusinessObject.Enums;

namespace EVAuctionTrader.DataAccess.Entities
{
    public class Auction : BaseEntity
    {
        public Guid SellerId { get; set; }
        public User Seller { get; set; }

        public string AuctionType { get; set; }     // vehicle | battery
        public Guid? VehicleId { get; set; }
        public Vehicle Vehicle { get; set; }
        public Guid? BatteryId { get; set; }
        public Battery Battery { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        public string ConditionNote { get; set; }

        public decimal StartPrice { get; set; }
        public decimal MinIncrement { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public AuctionStatus Status { get; set; }

        public string MainPhotoUrl { get; set; }
        public string GalleryJson { get; set; }

        public ICollection<Bid> Bids { get; set; }
        public ICollection<Order> Orders { get; set; }
    }
}
