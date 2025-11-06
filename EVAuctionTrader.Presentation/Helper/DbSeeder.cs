using EVAuctionTrader.Business.Utils;
using EVAuctionTrader.BusinessObject.Enums;
using EVAuctionTrader.DataAccess;
using EVAuctionTrader.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace EVAuctionTrader.Presentation.Helper
{
    public static class DbSeeder
    {
        public static async Task SeedUsersAsync(EVAuctionTraderDbContext context)
        {
            await context.Database.MigrateAsync();

            if (!await context.Users.AnyAsync(u => u.Role == RoleType.Admin))
            {
                var passwordHasher = new PasswordHasher();
                var admin = new User
                {
                    FullName = "Admin User",
                    Email = "admin@gmail.com",
                    Phone = "0786315267",
                    PasswordHash = passwordHasher.HashPassword("1@"),
                    Role = RoleType.Admin,
                    Status = "Active"
                };

                var wallet = new Wallet
                {
                    User = admin,
                    Balance = 0.0m
                };
                
                await context.Users.AddAsync(admin);
                await context.Wallets.AddAsync(wallet);
            }

            if (!await context.Users.AnyAsync(u => u.Role == RoleType.Member))
            {
                var passwordHasher = new PasswordHasher();
                var customer = new List<User> {
                    new User
                    {
                        FullName = "Member 1",
                        Email = "customer1@gmail.com",
                        Phone = "0786315268",
                        PasswordHash = passwordHasher.HashPassword("1@"),
                        Role = RoleType.Member,
                        Status = "Active"
                    },

                    new User
                    {
                        FullName = "Member 2",
                        Email = "customer2@gmail.com",
                        Phone = "0786315268",
                        PasswordHash = passwordHasher.HashPassword("1@"),
                        Role = RoleType.Member,
                        Status = "Active"
                    }
                };
                await context.Users.AddRangeAsync(customer);

                var wallets = new List<Wallet>
                {
                    new Wallet
                    {
                        User = customer[0],
                        Balance = 0.0m
                    },
                    new Wallet
                    {
                        User = customer[1],
                        Balance = 0.0m
                    }
                };
                await context.Wallets.AddRangeAsync(wallets);
            }

            await context.SaveChangesAsync();
        }

    }
}
