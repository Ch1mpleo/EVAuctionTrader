using EVAuctionTrader.BusinessObject.DTOs.BatteryDTOs;
using EVAuctionTrader.BusinessObject.DTOs.VehicleDTOs;
using EVAuctionTrader.BusinessObject.Enums;

namespace EVAuctionTrader.BusinessObject.DTOs.PostDTOs
{
    public class PostRequestDto
    {
        public PostType PostType { get; set; }
        public VehicleRequestPostDto? Vehicle { get; set; }
        public BatteryRequestPostDto? Battery { get; set; }

        public PostVersion Version { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }

        public decimal? Price { get; set; }

        public string LocationAddress { get; set; }
        public List<string> PhotoUrls { get; set; } = new();

        public PostStatus Status { get; set; } = PostStatus.Draft;
        public DateTime? PublishedAt { get; set; }
    }
}
