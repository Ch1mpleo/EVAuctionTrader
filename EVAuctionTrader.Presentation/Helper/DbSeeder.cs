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
                await context.Users.AddAsync(admin);
            }

            if (!await context.Users.AnyAsync(u => u.Role == RoleType.Customer))
            {
                var passwordHasher = new PasswordHasher();
                var customer = new List<User> {
                    new User
                    {
                        FullName = "Customer 1",
                        Email = "customer1@gmail.com",
                        Phone = "0786315268",
                        PasswordHash = passwordHasher.HashPassword("1@"),
                        Role = RoleType.Customer,
                        Status = "Active"
                    },

                    new User
                    {
                        FullName = "Customer 2",
                        Email = "customer2@gmail.com",
                        Phone = "0786315268",
                        PasswordHash = passwordHasher.HashPassword("1@"),
                        Role = RoleType.Customer,
                        Status = "Active"
                    }
                };
                await context.Users.AddRangeAsync(customer);
            }

            await context.SaveChangesAsync();
        }              

    }
}
