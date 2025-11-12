using EVAuctionTrader.BusinessObject.Enums;

namespace EVAuctionTrader.BusinessObject.DTOs.WalletTransactionDTOs
{
    public class WalletTransactionResponseDto
    {
        public Guid Id { get; set; }
        public Guid WalletId { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = null!;
        public WalletTransactionType Type { get; set; }
        public decimal Amount { get; set; }
        public decimal? BalanceAfter { get; set; }
        public WalletTransactionStatus Status { get; set; }

        // Related entities (nullable)
        public Guid? PostId { get; set; }
        public string? PostTitle { get; set; }

        public Guid? AuctionId { get; set; }
        public string? AuctionTitle { get; set; }

        public Guid? PaymentId { get; set; }
        public decimal? PaymentAmount { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
