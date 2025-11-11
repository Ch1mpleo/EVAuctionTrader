using EVAuctionTrader.BusinessObject.DTOs.BatteryDTOs;
using EVAuctionTrader.BusinessObject.DTOs.VehicleDTOs;
using EVAuctionTrader.BusinessObject.Enums;

namespace EVAuctionTrader.BusinessObject.DTOs.AuctionDTOs;

public class AuctionResponseDto
{
    public Guid Id { get; set; }
    public Guid CreatedBy { get; set; }
    public string CreatorName { get; set; } = string.Empty;
    public AuctionType AuctionType { get; set; }
    public VehicleResponseDto? Vehicle { get; set; }
    public BatteryResponseDto? Battery { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal StartPrice { get; set; }
    public decimal MinIncrement { get; set; }
    public decimal DepositRate { get; set; }
    public decimal CurrentPrice { get; set; }
    public Guid? WinnerId { get; set; }
    public string? WinnerName { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public AuctionStatus Status { get; set; }
    public string PhotoUrl { get; set; } = string.Empty;
    public int TotalBids { get; set; }
}
