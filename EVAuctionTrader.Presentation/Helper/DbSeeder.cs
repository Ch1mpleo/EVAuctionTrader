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

        public static async Task SeedPostsWithVehiclesAndBatteriesAsync(EVAuctionTraderDbContext context)
        {
            await context.Database.MigrateAsync();
            
            if(!await context.Posts.AnyAsync())
            {
                var members = await context.Users
                    .Where(u => u.Role == RoleType.Member)
                    .OrderBy(u => u.Id)
                    .Take(2)
                    .ToListAsync();
                
                if(members.Count < 2)
                {
                    return;
                }

                var vehicle1 = new Vehicle
                {
                    OwnerId = members[0].Id,
                    Brand = "Tesla",
                    Model = "Model 3",
                    Year = 2022,
                    OdometerKm = 15000,
                    ConditionGrade = "Excellent"
                };
                await context.Vehicles.AddAsync(vehicle1);
                await context.SaveChangesAsync();

                var post1 = new Post
                {
                    AuthorId = members[0].Id,
                    PostType = PostType.Vehicle,
                    VehicleId = vehicle1.Id,
                    Title = "Bán xe Tesle Model 3 2022 - Tình trạng tốt",
                    Description = "Tesla Model 3 năm 2022, đi được 15,000 km. Xe còn rất mới, full option, bảo hành còn 2 năm. Xe chính chủ, không tai nạn.",
                    Price = 1200000000m,
                    LocationAddress = "Quận 1, TP. Hồ Chí Minh",
                    Version = PostVersion.Vip,
                    Status = PostStatus.Active,
                    PublishedAt = DateTime.UtcNow.AddDays(-5),
                    ExpiresAt = DateTime.UtcNow.AddDays(25),
                    PhotoUrls = new List<string> { "data:image/jpeg;base64,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" }
                };

                var battery1 = new Battery
                {
                    OwnerId = members[0].Id,
                    Manufacturer = "CATL",
                    Chemistry = "Lithium-ion NMC",
                    CapacityKwh = 60.0m,
                    CycleCount = 800,
                    SohPercent = 85.0m,
                    VoltageV = 400.0m,
                    ConnectorType = "CCS2"
                };
                await context.Batteries.AddAsync(battery1);
                await context.SaveChangesAsync();

                var post2 = new Post
                {
                    AuthorId = members[0].Id,
                    PostType = PostType.Battery,
                    BatteryId = battery1.Id,
                    Title = "Pin lithium 60kWh - Độ chai 85%",
                    Description = "Pin lithium-ion dung lượng 60kWh, độ chai còn 85%, đã qua 800 chu kỳ sạc. Phù hợp cho xe điện tầm trung. Pin hoạt động tốt, không bị phồng rộp.",
                    Price = 150000000m,
                    LocationAddress = "Quận Bình Thạnh, TP. Hồ Chí Minh",
                    Version = PostVersion.Free,
                    Status = PostStatus.Active,
                    PublishedAt = DateTime.UtcNow.AddDays(-3),
                    ExpiresAt = DateTime.UtcNow.AddDays(12),
                    PhotoUrls = new List<string> { "https://cdn.tgdd.vn/Files/2019/12/10/1226086/uu-nhuoc-diem-cua-pin-lithium-co-nen-mua-xe-dien-pin-lithium-khong--1.jpg" }
                };

                var vehicle2 = new Vehicle
                {
                    OwnerId = members[1].Id,
                    Brand = "VinFast",
                    Model = "VF8",
                    Year = 2023,
                    OdometerKm = 8000,
                    ConditionGrade = "Very Good"
                };
                await context.Vehicles.AddAsync(vehicle2);
                await context.SaveChangesAsync();

                var post3 = new Post
                {
                    AuthorId = members[1].Id,
                    PostType = PostType.Vehicle,
                    VehicleId = vehicle2.Id,
                    Title = "VinFast VF8 2023 - Xe gia đình",
                    Description = "VinFast VF8 bản Plus, màu xanh, odo 8,000 km. Xe chính chủ, không tai nạn, ngập nước. Bảo hành chính hãng đầy đủ.",
                    Price = 980000000m,
                    LocationAddress = "Quận 7, TP. Hồ Chí Minh",
                    Version = PostVersion.Vip,
                    Status = PostStatus.Active,
                    PublishedAt = DateTime.UtcNow.AddDays(-7),
                    ExpiresAt = DateTime.UtcNow.AddDays(23),
                    PhotoUrls = new List<string> { "https://files01.danhgiaxe.com/I5K6zuMoolDuSRSy0GbZD1j8cQA=/fit-in/1280x0/20221213/vinfast-vf8-anh-9-223340-144338.jpeg" }
                };

                var vehicle3 = new Vehicle
                {
                    OwnerId = members[1].Id,
                    Brand = "Hyundai",
                    Model = "Kona Electric",
                    Year = 2021,
                    OdometerKm = 25000,
                    ConditionGrade = "Good"
                };
                await context.Vehicles.AddAsync(vehicle3);
                await context.SaveChangesAsync();

                var post5 = new Post
                {
                    AuthorId = members[1].Id,
                    PostType = PostType.Vehicle,
                    VehicleId = vehicle3.Id,
                    Title = "Hyundai Kona Electric 2021",
                    Description = "Hyundai Kona Electric bản tiêu chuẩn, màu trắng, odo 25,000 km. Xe đẹp, giá tốt. Đang soạn thảo để hoàn thiện thông tin.",
                    Price = 650000000m,
                    LocationAddress = "Quận Thanh Xuân, Hà Nội",
                    Version = PostVersion.Free,
                    Status = PostStatus.Draft,
                    PublishedAt = null,
                    ExpiresAt = null,
                    PhotoUrls = new List<string> { "data:image/jpeg;base64,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" }
                };

                var battery2 = new Battery
                {
                    OwnerId = members[1].Id,
                    Manufacturer = "BYD",
                    Chemistry = "LFP (Lithium Iron Phosphate)",
                    CapacityKwh = 75.0m,
                    CycleCount = 500,
                    SohPercent = 92.0m,
                    VoltageV = 350.0m,
                    ConnectorType = "CCS2"
                };
                await context.Batteries.AddAsync(battery2);
                await context.SaveChangesAsync();

                var post6 = new Post
                {
                    AuthorId = members[1].Id,
                    PostType = PostType.Battery,
                    BatteryId = battery2.Id,
                    Title = "Pin BYD LFP 75kWh - Độ chai 92%",
                    Description = "Pin BYD LFP dung lượng 75kWh, độ chai còn 92%, mới chạy 500 chu kỳ. Pin an toàn, tuổi thọ cao. Phù hợp cho các dòng xe cao cấp.",
                    Price = 200000000m,
                    LocationAddress = "Quận Cầu Giấy, Hà Nội",
                    Version = PostVersion.Vip,
                    Status = PostStatus.Active,
                    PublishedAt = DateTime.UtcNow.AddDays(-1),
                    ExpiresAt = DateTime.UtcNow.AddDays(29),
                    PhotoUrls = new List<string> { "https://hanoiescooters-battery.com/wp-content/uploads/2024/11/IMG-20241120-WA0004.jpg" }
                };

                await context.Posts.AddRangeAsync(new[] { post1, post2, post3, post5, post6 });
                await context.SaveChangesAsync();
            }           
        }
    }
}
