using EVAuctionTrader.BusinessObject.Enums;

namespace EVAuctionTrader.DataAccess.Entities
{
    public class Post : BaseEntity
    {
        public Guid AuthorId { get; set; }

        public PostType PostType { get; set; }
        public Guid? VehicleId { get; set; }
        public Guid? BatteryId { get; set; }

        // Version: VIP or Free
        public PostVersion Version { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        public string Detail { get; set; }

        public decimal? Price { get; set; }

        // Địa chỉ
        public string LocationCity { get; set; }
        public string LocationDistrict { get; set; }
        public string LocationAddress { get; set; }
        public List<string> PhotoUrls { get; set; } = new();

        // Trạng thái bài đăng
        public PostStatus Status { get; set; }
        public DateTime? PublishedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }

        public User Author { get; set; }
        public Vehicle? Vehicle { get; set; }
        public Battery? Battery { get; set; }
        public ICollection<PostComment> Comments { get; set; }
    }
}
