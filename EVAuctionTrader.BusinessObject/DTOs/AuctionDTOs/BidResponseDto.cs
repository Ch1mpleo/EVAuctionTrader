namespace EVAuctionTrader.BusinessObject.DTOs.AuctionDTOs;

public class BidResponseDto
{
    public Guid Id { get; set; }
    public Guid AuctionId { get; set; }
    public Guid BidderId { get; set; }
    public string BidderName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; }
}
