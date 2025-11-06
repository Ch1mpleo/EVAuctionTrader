namespace EVAuctionTrader.BusinessObject.DTOs.VehicleDTOs
{
    public class VehicleRequestPostDto
    {
        public string Brand { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public int OdometerKm { get; set; }
        public string ConditionGrade { get; set; }
    }
}
