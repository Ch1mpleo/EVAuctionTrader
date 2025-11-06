using EVAuctionTrader.DataAccess.Entities;

namespace EVAuctionTrader.DataAccess.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<User> Users { get; }
        IGenericRepository<Vehicle> Vehicles { get; }
        IGenericRepository<Battery> Batteries { get; }
        IGenericRepository<Wallet> Wallets { get; }
        Task<int> SaveChangesAsync();
    }
}
