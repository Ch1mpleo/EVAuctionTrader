using EVAuctionTrader.BusinessObject.Enums;

namespace EVAuctionTrader.DataAccess.Entities;

public sealed class Payment : BaseEntity
{
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; }
    public DateTime PaymentDate { get; set; }

    public string? PaymentIntentId { get; set; }
    public string? CheckoutSessionId { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public User User { get; set; }
}
