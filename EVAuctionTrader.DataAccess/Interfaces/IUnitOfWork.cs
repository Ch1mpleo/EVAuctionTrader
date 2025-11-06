using EVAuctionTrader.DataAccess.Entities;

namespace EVAuctionTrader.DataAccess.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<User> Users { get; }
        IGenericRepository<Vehicle> Vehicles { get; }
        IGenericRepository<Battery> Batteries { get; }
        IGenericRepository<Wallet> Wallets { get; }
        IGenericRepository<WalletTransaction> WalletTransactions { get; }
        IGenericRepository<Post> Posts { get; }
        IGenericRepository<PostComment> PostComments { get; }
        IGenericRepository<Auction> Auctions { get; }
        IGenericRepository<Bid> Bids { get; }
        IGenericRepository<Conversation> Conversations { get; }
        IGenericRepository<Message> Messages { get; }
        Task<int> SaveChangesAsync();
    }
}
