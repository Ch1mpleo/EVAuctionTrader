using EVAuctionTrader.DataAccess.Entities;

namespace EVAuctionTrader.DataAccess.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<User> Users { get; }
        Task<int> SaveChangesAsync();
    }
}
