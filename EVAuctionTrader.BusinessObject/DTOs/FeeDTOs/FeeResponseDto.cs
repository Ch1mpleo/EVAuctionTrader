using EVAuctionTrader.BusinessObject.Enums;

namespace EVAuctionTrader.BusinessObject.DTOs.FeeDTOs;

public sealed class FeeResponseDto
{
    public Guid Id { get; set; }
    public FeeType Type { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
