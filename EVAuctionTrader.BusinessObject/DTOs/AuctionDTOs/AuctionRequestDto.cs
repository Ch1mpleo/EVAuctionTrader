using EVAuctionTrader.BusinessObject.DTOs.BatteryDTOs;
using EVAuctionTrader.BusinessObject.DTOs.VehicleDTOs;
using EVAuctionTrader.BusinessObject.Enums;

namespace EVAuctionTrader.BusinessObject.DTOs.AuctionDTOs;

public class AuctionRequestDto
{
    public AuctionType AuctionType { get; set; }
    public Guid? VehicleId { get; set; }
    public Guid? BatteryId { get; set; }
    
    // Nested creation for new vehicles/batteries
    public VehicleRequestPostDto? Vehicle { get; set; }
    public BatteryRequestPostDto? Battery { get; set; }
    
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal StartPrice { get; set; }
    public decimal MinIncrement { get; set; }
    public decimal DepositRate { get; set; } = 0.20m; // Default 20%
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string PhotoUrl { get; set; } = string.Empty;
}
