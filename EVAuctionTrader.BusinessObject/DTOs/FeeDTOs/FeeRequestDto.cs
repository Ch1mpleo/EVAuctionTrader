using EVAuctionTrader.BusinessObject.Enums;

namespace EVAuctionTrader.BusinessObject.DTOs.FeeDTOs;

public sealed class FeeRequestDto
{
    public FeeType Type { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = null!;
}
