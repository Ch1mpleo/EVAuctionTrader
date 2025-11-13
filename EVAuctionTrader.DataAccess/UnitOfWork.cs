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
            IGenericRepository<Wallet> walletRepository,
            IGenericRepository<WalletTransaction> walletTransactionRepository,
            IGenericRepository<Post> postRepository,
            IGenericRepository<PostComment> postCommentRepository,
            IGenericRepository<Auction> auctionRepository,
            IGenericRepository<Bid> bidRepository,
            IGenericRepository<Conversation> conversationRepository,
            IGenericRepository<Message> messageRepository,
            IGenericRepository<Payment> paymentRepository,
            IGenericRepository<Fee> feeRepository
            )
        {
            _dbContext = dbContext;
            Users = userRepository;
            Vehicles = vehicleRepository;
            Batteries = batteryRepository;
            Wallets = walletRepository;
            WalletTransactions = walletTransactionRepository;
            Posts = postRepository;
            PostComments = postCommentRepository;
            Auctions = auctionRepository;
            Bids = bidRepository;
            Conversations = conversationRepository;
            Messages = messageRepository;
            Payments = paymentRepository;
            Fees = feeRepository;
        }

        public IGenericRepository<User> Users { get; set; }
        public IGenericRepository<Vehicle> Vehicles { get; set; }
        public IGenericRepository<Battery> Batteries { get; set; }
        public IGenericRepository<Wallet> Wallets { get; set; }
        public IGenericRepository<WalletTransaction> WalletTransactions { get; set; }
        public IGenericRepository<Post> Posts { get; set; }
        public IGenericRepository<PostComment> PostComments { get; set; }
        public IGenericRepository<Auction> Auctions { get; set; }
        public IGenericRepository<Bid> Bids { get; set; }
        public IGenericRepository<Conversation> Conversations { get; set; }
        public IGenericRepository<Message> Messages { get; set; }
        public IGenericRepository<Payment> Payments { get; set; }
        public IGenericRepository<Fee> Fees { get; set; }

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
