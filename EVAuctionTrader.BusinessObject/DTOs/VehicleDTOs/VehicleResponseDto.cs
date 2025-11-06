namespace EVAuctionTrader.BusinessObject.DTOs.VehicleDTOs
{
    public class VehicleResponseDto
    {
        public Guid Id { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public int OdometerKm { get; set; }
        public string ConditionGrade { get; set; } = string.Empty;
    }
}
