namespace EVAuctionTrader.BusinessObject.DTOs.RevenueDTOs;

public sealed class RevenueDetailDto
{
    public Guid TransactionId { get; set; }
    public Guid PostId { get; set; }
    public string PostTitle { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; }
}
