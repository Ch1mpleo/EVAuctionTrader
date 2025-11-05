using EVAuctionTrader.BusinessObject.Enums;

namespace EVAuctionTrader.DataAccess.Entities
{
    public class User : BaseEntity
    {
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Phone { get; set; }
        public string FullName { get; set; }
        public RoleType Role { get; set; }
        public string Status { get; set; } // active, suspended

        public ICollection<Vehicle> Vehicles { get; set; }
        public ICollection<Battery> Batteries { get; set; }
        public ICollection<Post> Posts { get; set; }
        public ICollection<Wallet> Wallets { get; set; }
    }
}
