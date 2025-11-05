namespace EVAuctionTrader.DataAccess.Entities
{
    public class Wallet : BaseEntity
    {
        public Guid UserId { get; set; }
        public string Currency { get; set; }
        public decimal Balance { get; set; }

        public User User { get; set; }
        public ICollection<WalletTransaction> Transactions { get; set; }
    }
}
