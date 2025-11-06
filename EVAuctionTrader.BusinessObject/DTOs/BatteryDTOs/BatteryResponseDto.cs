namespace EVAuctionTrader.BusinessObject.DTOs.BatteryDTOs
{
    public class BatteryResponseDto
    {
        public Guid Id { get; set; }
        public string Manufacturer { get; set; } = string.Empty;
        public string Chemistry { get; set; } = string.Empty;
        public decimal CapacityKwh { get; set; }
        public int CycleCount { get; set; }
        public decimal SohPercent { get; set; }
        public decimal VoltageV { get; set; }
        public string ConnectorType { get; set; } = string.Empty;
    }
}
