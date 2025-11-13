using EVAuctionTrader.BusinessObject.Enums;

namespace EVAuctionTrader.DataAccess.Entities
{
    public class Fee : BaseEntity
    {
        public FeeType Type { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; } = null!;
    }
}
