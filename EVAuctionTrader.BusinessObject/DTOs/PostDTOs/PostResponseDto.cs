using EVAuctionTrader.BusinessObject.DTOs.BatteryDTOs;
using EVAuctionTrader.BusinessObject.DTOs.VehicleDTOs;
using EVAuctionTrader.BusinessObject.Enums;

namespace EVAuctionTrader.BusinessObject.DTOs.PostDTOs
{
    public class PostResponseDto
    {
        public Guid Id { get; set; }
        public Guid AuthorId { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public PostType PostType { get; set; }
        public VehicleResponseDto? Vehicle { get; set; }
        public BatteryResponseDto? Battery { get; set; }

        public PostVersion Version { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public decimal? Price { get; set; }

        public string LocationAddress { get; set; } = string.Empty;
        public List<string> PhotoUrls { get; set; } = new();

        public PostStatus Status { get; set; } = PostStatus.Draft;
        public DateTime? PublishedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
}
