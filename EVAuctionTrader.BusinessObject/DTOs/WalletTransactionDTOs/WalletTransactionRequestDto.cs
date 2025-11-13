using EVAuctionTrader.BusinessObject.Enums;

namespace EVAuctionTrader.BusinessObject.DTOs.WalletTransactionDTOs
{
    public class WalletTransactionRequestDto
    {
        public WalletTransactionType Type { get; set; }
        public decimal Amount { get; set; }
        public decimal? BalanceAfter { get; set; }
        public WalletTransactionStatus Status { get; set; }
        public Guid? PostId { get; set; }
        public Guid? AuctionId { get; set; }
        public Guid? PaymentId { get; set; }
    }
}
