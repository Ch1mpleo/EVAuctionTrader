using EVAuctionTrader.DataAccess.Entities;
using EVAuctionTrader.DataAccess.Interfaces;

namespace EVAuctionTrader.DataAccess
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly EVAuctionTraderDbContext _dbContext;

        public UnitOfWork(EVAuctionTraderDbContext dbContext,
            IGenericRepository<User> userRepository,
            IGenericRepository<Vehicle> vehicleRepository,
            IGenericRepository<Battery> batteryRepository,
            IGenericRepository<Wallet> walletRepository
            )
        {
            _dbContext = dbContext;
            Users = userRepository;
            Vehicles = vehicleRepository;
            Batteries = batteryRepository;
            Wallets = walletRepository;
        }

        public IGenericRepository<User> Users { get; set; }
        public IGenericRepository<Vehicle> Vehicles { get; set; }
        public IGenericRepository<Battery> Batteries { get; set; }
        public IGenericRepository<Wallet> Wallets { get; set; }

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
