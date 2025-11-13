using EVAuctionTrader.DataAccess.Commons;
using EVAuctionTrader.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace EVAuctionTrader.DataAccess
{
    public class EVAuctionTraderDbContext : DbContext
    {
        public EVAuctionTraderDbContext() { }

        public EVAuctionTraderDbContext(DbContextOptions<EVAuctionTraderDbContext> options)
            : base(options) { }

        // -------------------- DbSets --------------------
        public DbSet<User> Users { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Battery> Batteries { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<PostComment> PostComments { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<WalletTransaction> WalletTransactions { get; set; }
        public DbSet<Auction> Auctions { get; set; }
        public DbSet<Bid> Bids { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Fee> Fees { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Enum string conversion
            modelBuilder.UseStringForEnums();

            // -------------------- RELATIONSHIPS --------------------
            modelBuilder.Entity<Conversation>()
                .HasIndex(c => new { c.PostId, c.BuyerId, c.SellerId })
                .IsUnique();

            modelBuilder.Entity<Vehicle>()
                .HasOne(v => v.Owner)
                .WithMany(u => u.Vehicles)
                .HasForeignKey(v => v.OwnerId);

            modelBuilder.Entity<Battery>()
                .HasOne(b => b.Owner)
                .WithMany(u => u.Batteries)
                .HasForeignKey(b => b.OwnerId);

            modelBuilder.Entity<Post>()
                .HasOne(p => p.Author)
                .WithMany(u => u.Posts)
                .HasForeignKey(p => p.AuthorId);

            modelBuilder.Entity<Post>()
                .HasOne(p => p.Vehicle)
                .WithMany()
                .HasForeignKey(p => p.VehicleId);

            modelBuilder.Entity<Post>()
                .HasOne(p => p.Battery)
                .WithMany()
                .HasForeignKey(p => p.BatteryId);

            modelBuilder.Entity<PostComment>()
                .HasOne(pc => pc.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(pc => pc.PostId);

            modelBuilder.Entity<PostComment>()
                .HasOne(pc => pc.Author)
                .WithMany()
                .HasForeignKey(pc => pc.AuthorId);

            // Self-referencing relationship cho nested comments
            modelBuilder.Entity<PostComment>()
                .HasOne(pc => pc.ParentComment)
                .WithMany(pc => pc.Replies)
                .HasForeignKey(pc => pc.ParentCommentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Wallet>()
                .HasOne(w => w.User)
                .WithMany(u => u.Wallets)
                .HasForeignKey(w => w.UserId);

            modelBuilder.Entity<WalletTransaction>()
                .HasOne(wt => wt.Wallet)
                .WithMany(w => w.Transactions)
                .HasForeignKey(wt => wt.WalletId);

            modelBuilder.Entity<WalletTransaction>()
                .HasOne(wt => wt.Post)
                .WithMany()
                .HasForeignKey(wt => wt.PostId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WalletTransaction>()
                .HasOne(wt => wt.Auction)
                .WithMany()
                .HasForeignKey(wt => wt.AuctionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Auction>()
                .HasOne(a => a.Creator)
                .WithMany()
                .HasForeignKey(a => a.CreatedBy);

            modelBuilder.Entity<Auction>()
                .HasOne(a => a.Vehicle)
                .WithMany()
                .HasForeignKey(a => a.VehicleId);

            modelBuilder.Entity<Auction>()
                .HasOne(a => a.Battery)
                .WithMany()
                .HasForeignKey(a => a.BatteryId);

            modelBuilder.Entity<Bid>()
                .HasOne(b => b.Auction)
                .WithMany(a => a.Bids)
                .HasForeignKey(b => b.AuctionId);

            modelBuilder.Entity<Bid>()
                .HasOne(b => b.Bidder)
                .WithMany()
                .HasForeignKey(b => b.BidderId);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId);
        }
    }
}
