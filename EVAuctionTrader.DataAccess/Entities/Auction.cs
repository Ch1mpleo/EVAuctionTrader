using EVAuctionTrader.BusinessObject.Enums;

namespace EVAuctionTrader.DataAccess.Entities
{

    // Buổi đấu giá
    public class Auction : BaseEntity
    {
        public Guid CreatedBy { get; set; }
        public AuctionType AuctionType { get; set; }
        public Guid? VehicleId { get; set; }
        public Guid? BatteryId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal StartPrice { get; set; }
        public decimal MinIncrement { get; set; }
        public decimal DepositRate { get; set; }    // Rate để hold tiền bidder: 20%
        public decimal CurrentPrice { get; set; }   // Current highest bid amount
        public Guid? WinnerId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public AuctionStatus Status { get; set; }

        public string PhotoUrl { get; set; }

        public User Creator { get; set; }
        public User? Winner { get; set; }
        public Vehicle? Vehicle { get; set; }
        public Battery? Battery { get; set; }
        public ICollection<Bid> Bids { get; set; }
    }
}
