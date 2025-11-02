using EVAuctionTrader.DataAccess.Entities;
using EVAuctionTrader.DataAccess.Interfaces;

namespace EVAuctionTrader.DataAccess
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly EVAuctionTraderDbContext _dbContext;

        public UnitOfWork(EVAuctionTraderDbContext dbContext,
            IGenericRepository<User> userRepository
            )
        {
            _dbContext = dbContext;
            Users = userRepository;
        }

        public IGenericRepository<User> Users { get; set; }

        public void Dispose()
        {
            _dbContext.Dispose();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }
    }
}
