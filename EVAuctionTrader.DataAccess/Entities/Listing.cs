using EVAuctionTrader.BusinessObject.Enums;

namespace EVAuctionTrader.DataAccess.Entities
{
    public class Listing : BaseEntity
    {
        public Guid SellerId { get; set; }
        public User Seller { get; set; }

        public string ListingType { get; set; }     // vehicle | battery
        public Guid? VehicleId { get; set; }
        public Vehicle Vehicle { get; set; }
        public Guid? BatteryId { get; set; }
        public Battery Battery { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        public string ConditionNote { get; set; }

        public decimal BuyNowPrice { get; set; }
        public string Currency { get; set; }

        public string MainPhotoUrl { get; set; }
        public string GalleryJson { get; set; }

        public ListingStatus Status { get; set; }

        public ICollection<Order> Orders { get; set; }
    }
}
