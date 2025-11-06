namespace EVAuctionTrader.BusinessObject.DTOs.BatteryDTOs
{
    public class BatteryRequestPostDto
    {
        public string Manufacturer { get; set; }
        public string Chemistry { get; set; }
        public decimal CapacityKwh { get; set; }
        public int CycleCount { get; set; }
        public decimal SohPercent { get; set; }
        public decimal VoltageV { get; set; }
        public string ConnectorType { get; set; }
    }
}
