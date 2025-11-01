using EVAuctionTrader.DataAccess.Interfaces;

namespace EVAuctionTrader.DataAccess
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly EVAuctionTraderDbContext _dbContext;

        public UnitOfWork(EVAuctionTraderDbContext dbContext
            )
        {
            _dbContext = dbContext;
        }


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
