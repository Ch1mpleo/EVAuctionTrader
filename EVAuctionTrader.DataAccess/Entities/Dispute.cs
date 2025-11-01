using EVAuctionTrader.BusinessObject.Enums;

namespace EVAuctionTrader.DataAccess.Entities
{
    public class Dispute : BaseEntity
    {
        public Guid OrderId { get; set; }
        public Order Order { get; set; }

        public Guid RaisedBy { get; set; }
        public Guid? HandledBy { get; set; }

        public DisputeStatus Status { get; set; }
        public string Reason { get; set; }
        public string Resolution { get; set; }
    }
}
