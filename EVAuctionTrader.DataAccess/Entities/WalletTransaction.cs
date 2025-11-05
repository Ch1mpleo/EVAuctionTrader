using EVAuctionTrader.BusinessObject.Enums;

namespace EVAuctionTrader.DataAccess.Entities
{
    public class WalletTransaction : BaseEntity
    {
        public Guid WalletId { get; set; }
        public WalletTransactionType Type { get; set; }
        public decimal Amount { get; set; }
        public decimal? BalanceAfter { get; set; }
        public WalletTransactionStatus Status { get; set; }
        public Guid? PostId { get; set; }
        public Guid? AuctionId { get; set; }
        public string Provider { get; set; }
        public string ProviderRef { get; set; }
        public string Note { get; set; }

        public Wallet Wallet { get; set; }
        public Post Post { get; set; }
        public Auction Auction { get; set; }
    }
}
