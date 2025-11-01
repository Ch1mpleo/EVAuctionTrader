using EVAuctionTrader.BusinessObject.Enums;
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
        public DbSet<Listing> Listings { get; set; }
        public DbSet<Auction> Auctions { get; set; }
        public DbSet<Bid> Bids { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Dispute> Disputes { get; set; }
        public DbSet<Verification> Verifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- Store enums as strings for readability ---
            modelBuilder
                .UseEnumStringMappings<RoleType, ListingStatus, AuctionStatus,
                    OrderStatus, OrderType, InvoiceStatus, PaymentStatus,
                    VerificationStatus, VerificationMethod, DisputeStatus>();

            // ---- Vehicle & Battery Owners ----
            modelBuilder.Entity<Vehicle>()
                .HasOne(v => v.Owner)
                .WithMany(u => u.Vehicles)
                .HasForeignKey(v => v.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Battery>()
                .HasOne(b => b.Owner)
                .WithMany(u => u.Batteries)
                .HasForeignKey(b => b.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            // ---- Listing relationships ----
            modelBuilder.Entity<Listing>()
                .HasOne(l => l.Seller)
                .WithMany(u => u.Listings)
                .HasForeignKey(l => l.SellerId)
                .OnDelete(DeleteBehavior.Restrict);

            // ---- Auction relationships ----
            modelBuilder.Entity<Auction>()
                .HasOne(a => a.Seller)
                .WithMany(u => u.Auctions)
                .HasForeignKey(a => a.SellerId)
                .OnDelete(DeleteBehavior.Restrict);

            // ---- Bid relationships ----
            modelBuilder.Entity<Bid>()
                .HasOne(b => b.Auction)
                .WithMany(a => a.Bids)
                .HasForeignKey(b => b.AuctionId)
                .OnDelete(DeleteBehavior.Cascade);

            // ---- Order relationships ----
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Listing)
                .WithMany(l => l.Orders)
                .HasForeignKey(o => o.ListingId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Auction)
                .WithMany(a => a.Orders)
                .HasForeignKey(o => o.AuctionId)
                .OnDelete(DeleteBehavior.Restrict);

            // ---- Invoice relationships ----
            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Order)
                .WithMany(o => o.Invoices)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // ---- Payment relationships ----
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Invoice)
                .WithMany(i => i.Payments)
                .HasForeignKey(p => p.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            // ---- Verification relationship ----
            modelBuilder.Entity<Verification>()
                .HasOne(v => v.VerifiedByUser)
                .WithMany()
                .HasForeignKey(v => v.VerifiedBy)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    // Helper extension to simplify enum-to-string conversions
    internal static class ModelBuilderEnumExtensions
    {
        public static ModelBuilder UseEnumStringMappings<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this ModelBuilder builder)
        {
            builder.Entity<User>().Property(e => e.Role).HasConversion<string>();
            builder.Entity<Listing>().Property(e => e.Status).HasConversion<string>();
            builder.Entity<Auction>().Property(e => e.Status).HasConversion<string>();
            builder.Entity<Order>().Property(e => e.Status).HasConversion<string>();
            builder.Entity<Order>().Property(e => e.OrderType).HasConversion<string>();
            builder.Entity<Invoice>().Property(e => e.Status).HasConversion<string>();
            builder.Entity<Payment>().Property(e => e.Status).HasConversion<string>();
            builder.Entity<Verification>().Property(e => e.Status).HasConversion<string>();
            builder.Entity<Verification>().Property(e => e.Method).HasConversion<string>();
            builder.Entity<Dispute>().Property(e => e.Status).HasConversion<string>();
            return builder;
        }
    }
}
