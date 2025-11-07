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

            if (!await context.Posts.AnyAsync())
            {
                var members = await context.Users
                    .Where(u => u.Role == RoleType.Member)
                    .OrderBy(u => u.Id)
                    .Take(2)
                    .ToListAsync();

                if (members.Count < 2)
                {
                    return;
                }

                // Post 1 - Tesla Model 3
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
                    Title = "Tesla Model 3 2022 - Excellent Condition",
                    Description = "Tesla Model 3 from 2022, only 15,000 km driven. Car is like new, full options, 2 years warranty remaining. Original owner, no accidents.",
                    Price = 45000m,
                    LocationAddress = "District 1, Ho Chi Minh City",
                    Version = PostVersion.Vip,
                    Status = PostStatus.Active,
                    PublishedAt = DateTime.UtcNow.AddDays(-5),
                    ExpiresAt = DateTime.UtcNow.AddDays(25),
                    PhotoUrls = new List<string> { "https://images.unsplash.com/photo-1603584173870-7f23fdae1b7a?ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&q=80&w=1169" }
                };

                // Post 2 - CATL Battery
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
                    Title = "CATL Lithium 60kWh - 85% State of Health",
                    Description = "CATL lithium-ion battery 60kWh capacity, 85% state of health, 800 charge cycles completed. Suitable for mid-range electric vehicles. Battery in good working condition, no swelling.",
                    Price = 5500m,
                    LocationAddress = "Binh Thanh District, Ho Chi Minh City",
                    Version = PostVersion.Free,
                    Status = PostStatus.Active,
                    PublishedAt = DateTime.UtcNow.AddDays(-3),
                    ExpiresAt = DateTime.UtcNow.AddDays(12),
                    PhotoUrls = new List<string> { "https://plus.unsplash.com/premium_photo-1681433401553-589104846604?ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&q=80&w=1760" }
                };

                // Post 3 - VinFast VF8
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
                    Title = "VinFast VF8 2023 - Family Car",
                    Description = "VinFast VF8 Plus version, blue color, 8,000 km. Original owner, no accidents or flooding. Full manufacturer warranty.",
                    Price = 38000m,
                    LocationAddress = "District 7, Ho Chi Minh City",
                    Version = PostVersion.Vip,
                    Status = PostStatus.Active,
                    PublishedAt = DateTime.UtcNow.AddDays(-7),
                    ExpiresAt = DateTime.UtcNow.AddDays(23),
                    PhotoUrls = new List<string> { "https://images.unsplash.com/photo-1494905998402-395d579af36f?ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&q=80&w=1170" }
                };

                // Post 4 - Hyundai Kona Electric (Draft)
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

                var post4 = new Post
                {
                    AuthorId = members[1].Id,
                    PostType = PostType.Vehicle,
                    VehicleId = vehicle3.Id,
                    Title = "Hyundai Kona Electric 2021",
                    Description = "Hyundai Kona Electric standard version, white color, 25,000 km. Nice car, good price. Draft in progress to complete information.",
                    Price = 25000m,
                    LocationAddress = "Thanh Xuan District, Hanoi",
                    Version = PostVersion.Free,
                    Status = PostStatus.Draft,
                    PublishedAt = null,
                    ExpiresAt = null,
                    PhotoUrls = new List<string> { "https://plus.unsplash.com/premium_photo-1661891539075-24b4e473f67f?ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&q=80&w=1171" }
                };

                // Post 5 - BYD Battery
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

                var post5 = new Post
                {
                    AuthorId = members[1].Id,
                    PostType = PostType.Battery,
                    BatteryId = battery2.Id,
                    Title = "BYD LFP 75kWh Battery - 92% SOH",
                    Description = "BYD LFP battery 75kWh capacity, 92% state of health, only 500 charge cycles. Safe battery, long lifespan. Suitable for premium vehicles.",
                    Price = 7500m,
                    LocationAddress = "Cau Giay District, Hanoi",
                    Version = PostVersion.Vip,
                    Status = PostStatus.Active,
                    PublishedAt = DateTime.UtcNow.AddDays(-1),
                    ExpiresAt = DateTime.UtcNow.AddDays(29),
                    PhotoUrls = new List<string> { "https://images.unsplash.com/photo-1605191737662-98ba90cb953e?ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&q=80&w=1170" }
                };

                // Post 6 - Nissan Leaf
                var vehicle4 = new Vehicle
                {
                    OwnerId = members[0].Id,
                    Brand = "Nissan",
                    Model = "Leaf",
                    Year = 2020,
                    OdometerKm = 30000,
                    ConditionGrade = "Good"
                };
                await context.Vehicles.AddAsync(vehicle4);
                await context.SaveChangesAsync();

                var post6 = new Post
                {
                    AuthorId = members[0].Id,
                    PostType = PostType.Vehicle,
                    VehicleId = vehicle4.Id,
                    Title = "Nissan Leaf 2020 - Economical EV",
                    Description = "Nissan Leaf 2020 with 30,000 km. Perfect city car, low maintenance costs. Well maintained, all service records available.",
                    Price = 22000m,
                    LocationAddress = "District 3, Ho Chi Minh City",
                    Version = PostVersion.Free,
                    Status = PostStatus.Active,
                    PublishedAt = DateTime.UtcNow.AddDays(-10),
                    ExpiresAt = DateTime.UtcNow.AddDays(20),
                    PhotoUrls = new List<string> { "https://images.unsplash.com/photo-1519641471654-76ce0107ad1b?ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&q=80&w=1171" }
                };

                // Post 7 - LG Chem Battery
                var battery3 = new Battery
                {
                    OwnerId = members[0].Id,
                    Manufacturer = "LG Chem",
                    Chemistry = "Lithium-ion NCM",
                    CapacityKwh = 64.0m,
                    CycleCount = 600,
                    SohPercent = 88.0m,
                    VoltageV = 380.0m,
                    ConnectorType = "CCS2"
                };
                await context.Batteries.AddAsync(battery3);
                await context.SaveChangesAsync();

                var post7 = new Post
                {
                    AuthorId = members[0].Id,
                    PostType = PostType.Battery,
                    BatteryId = battery3.Id,
                    Title = "LG Chem 64kWh Battery Pack - 88% SOH",
                    Description = "LG Chem lithium-ion battery 64kWh, 88% state of health, 600 cycles. High quality Korean battery, reliable performance.",
                    Price = 6000m,
                    LocationAddress = "District 2, Ho Chi Minh City",
                    Version = PostVersion.Vip,
                    Status = PostStatus.Active,
                    PublishedAt = DateTime.UtcNow.AddDays(-6),
                    ExpiresAt = DateTime.UtcNow.AddDays(24),
                    PhotoUrls = new List<string> { "https://images.unsplash.com/photo-1591964006776-90b32e88f5ec?ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&q=80&w=764" }
                };

                // Post 8 - BMW i3
                var vehicle5 = new Vehicle
                {
                    OwnerId = members[1].Id,
                    Brand = "BMW",
                    Model = "i3",
                    Year = 2019,
                    OdometerKm = 35000,
                    ConditionGrade = "Very Good"
                };
                await context.Vehicles.AddAsync(vehicle5);
                await context.SaveChangesAsync();

                var post8 = new Post
                {
                    AuthorId = members[1].Id,
                    PostType = PostType.Vehicle,
                    VehicleId = vehicle5.Id,
                    Title = "BMW i3 2019 - Premium Electric",
                    Description = "BMW i3 2019, 35,000 km. Stylish design, premium interior. Perfect for urban driving with excellent range.",
                    Price = 28000m,
                    LocationAddress = "Ba Dinh District, Hanoi",
                    Version = PostVersion.Vip,
                    Status = PostStatus.Active,
                    PublishedAt = DateTime.UtcNow.AddDays(-4),
                    ExpiresAt = DateTime.UtcNow.AddDays(26),
                    PhotoUrls = new List<string> { "https://images.unsplash.com/photo-1441148345475-03a2e82f9719?ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&q=80&w=1170" }
                };

                // Post 9 - Panasonic Battery
                var battery4 = new Battery
                {
                    OwnerId = members[1].Id,
                    Manufacturer = "Panasonic",
                    Chemistry = "Lithium-ion NCA",
                    CapacityKwh = 70.0m,
                    CycleCount = 700,
                    SohPercent = 90.0m,
                    VoltageV = 400.0m,
                    ConnectorType = "CCS2"
                };
                await context.Batteries.AddAsync(battery4);
                await context.SaveChangesAsync();

                var post9 = new Post
                {
                    AuthorId = members[1].Id,
                    PostType = PostType.Battery,
                    BatteryId = battery4.Id,
                    Title = "Panasonic 70kWh Battery - 90% SOH",
                    Description = "Panasonic lithium-ion NCA battery 70kWh, 90% state of health, 700 cycles. Premium Japanese quality, excellent thermal management.",
                    Price = 6800m,
                    LocationAddress = "Hoan Kiem District, Hanoi",
                    Version = PostVersion.Free,
                    Status = PostStatus.Active,
                    PublishedAt = DateTime.UtcNow.AddDays(-8),
                    ExpiresAt = DateTime.UtcNow.AddDays(22),
                    PhotoUrls = new List<string> { "https://plus.unsplash.com/premium_photo-1661771836121-9108a83875d7?ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&q=80&w=1666" }
                };

                // Post 10 - Chevrolet Bolt
                var vehicle6 = new Vehicle
                {
                    OwnerId = members[0].Id,
                    Brand = "Chevrolet",
                    Model = "Bolt EV",
                    Year = 2021,
                    OdometerKm = 18000,
                    ConditionGrade = "Excellent"
                };
                await context.Vehicles.AddAsync(vehicle6);
                await context.SaveChangesAsync();

                var post10 = new Post
                {
                    AuthorId = members[0].Id,
                    PostType = PostType.Vehicle,
                    VehicleId = vehicle6.Id,
                    Title = "Chevrolet Bolt EV 2021 - Great Range",
                    Description = "Chevrolet Bolt EV 2021, only 18,000 km. Excellent range of 417 km per charge. Spacious interior, modern tech features.",
                    Price = 26500m,
                    LocationAddress = "District 10, Ho Chi Minh City",
                    Version = PostVersion.Vip,
                    Status = PostStatus.Active,
                    PublishedAt = DateTime.UtcNow.AddDays(-2),
                    ExpiresAt = DateTime.UtcNow.AddDays(28),
                    PhotoUrls = new List<string> { "https://images.unsplash.com/photo-1494697536454-6f39e2cc972d?ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&q=80&w=1171" }
                };

                // Post 11 - Samsung SDI Battery
                var battery5 = new Battery
                {
                    OwnerId = members[0].Id,
                    Manufacturer = "Samsung SDI",
                    Chemistry = "Lithium-ion NCM",
                    CapacityKwh = 68.0m,
                    CycleCount = 550,
                    SohPercent = 91.0m,
                    VoltageV = 390.0m,
                    ConnectorType = "Type 2"
                };
                await context.Batteries.AddAsync(battery5);
                await context.SaveChangesAsync();

                var post11 = new Post
                {
                    AuthorId = members[0].Id,
                    PostType = PostType.Battery,
                    BatteryId = battery5.Id,
                    Title = "Samsung SDI 68kWh Battery - 91% SOH",
                    Description = "Samsung SDI lithium-ion battery 68kWh, 91% state of health, 550 cycles. High energy density, excellent performance in all weather conditions.",
                    Price = 6200m,
                    LocationAddress = "Phu Nhuan District, Ho Chi Minh City",
                    Version = PostVersion.Free,
                    Status = PostStatus.Active,
                    PublishedAt = DateTime.UtcNow.AddDays(-9),
                    ExpiresAt = DateTime.UtcNow.AddDays(21),
                    PhotoUrls = new List<string> { "https://images.unsplash.com/photo-1622062929134-a8fa99b46f56?ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&q=80&w=1631" }
                };

                // Post 12 - Audi e-tron
                var vehicle7 = new Vehicle
                {
                    OwnerId = members[1].Id,
                    Brand = "Audi",
                    Model = "e-tron",
                    Year = 2022,
                    OdometerKm = 12000,
                    ConditionGrade = "Excellent"
                };
                await context.Vehicles.AddAsync(vehicle7);
                await context.SaveChangesAsync();

                var post12 = new Post
                {
                    AuthorId = members[1].Id,
                    PostType = PostType.Vehicle,
                    VehicleId = vehicle7.Id,
                    Title = "Audi e-tron 2022 - Luxury Electric SUV",
                    Description = "Audi e-tron 2022, only 12,000 km. Luxurious electric SUV with cutting-edge technology. Full options, virtual cockpit, premium sound system.",
                    Price = 72000m,
                    LocationAddress = "Dong Da District, Hanoi",
                    Version = PostVersion.Vip,
                    Status = PostStatus.Active,
                    PublishedAt = DateTime.UtcNow.AddDays(-3),
                    ExpiresAt = DateTime.UtcNow.AddDays(27),
                    PhotoUrls = new List<string> { "https://images.unsplash.com/photo-1541899481282-d53bffe3c35d?ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&q=80&w=1170" }
                };

                // Post 13 - SK Innovation Battery
                var battery6 = new Battery
                {
                    OwnerId = members[1].Id,
                    Manufacturer = "SK Innovation",
                    Chemistry = "Lithium-ion NCM",
                    CapacityKwh = 77.0m,
                    CycleCount = 450,
                    SohPercent = 93.0m,
                    VoltageV = 410.0m,
                    ConnectorType = "CCS2"
                };
                await context.Batteries.AddAsync(battery6);
                await context.SaveChangesAsync();

                var post13 = new Post
                {
                    AuthorId = members[1].Id,
                    PostType = PostType.Battery,
                    BatteryId = battery6.Id,
                    Title = "SK Innovation 77kWh Battery - 93% SOH",
                    Description = "SK Innovation lithium-ion battery 77kWh, 93% state of health, only 450 cycles. Latest technology, fast charging capability, excellent longevity.",
                    Price = 7800m,
                    LocationAddress = "Tay Ho District, Hanoi",
                    Version = PostVersion.Vip,
                    Status = PostStatus.Active,
                    PublishedAt = DateTime.UtcNow.AddDays(-5),
                    ExpiresAt = DateTime.UtcNow.AddDays(25),
                    PhotoUrls = new List<string> { "https://images.unsplash.com/photo-1694889649703-e86125c14fe2?ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&q=80&w=1170" }
                };

                // Post 14 - Kia EV6
                var vehicle8 = new Vehicle
                {
                    OwnerId = members[0].Id,
                    Brand = "Kia",
                    Model = "EV6",
                    Year = 2023,
                    OdometerKm = 5000,
                    ConditionGrade = "Excellent"
                };
                await context.Vehicles.AddAsync(vehicle8);
                await context.SaveChangesAsync();

                var post14 = new Post
                {
                    AuthorId = members[0].Id,
                    PostType = PostType.Vehicle,
                    VehicleId = vehicle8.Id,
                    Title = "Kia EV6 2023 - Award Winning Design",
                    Description = "Kia EV6 2023, nearly brand new with only 5,000 km. Award-winning design, ultra-fast charging, advanced driver assistance systems. Like new condition.",
                    Price = 52000m,
                    LocationAddress = "District 5, Ho Chi Minh City",
                    Version = PostVersion.Vip,
                    Status = PostStatus.Active,
                    PublishedAt = DateTime.UtcNow.AddDays(-1),
                    ExpiresAt = DateTime.UtcNow.AddDays(29),
                    PhotoUrls = new List<string> { "https://images.unsplash.com/photo-1599599054812-1fee22d625e1?ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&q=80&w=1170" }
                };

                // Post 15 - Tesla Model Y
                var vehicle9 = new Vehicle
                {
                    OwnerId = members[1].Id,
                    Brand = "Tesla",
                    Model = "Model Y",
                    Year = 2023,
                    OdometerKm = 10000,
                    ConditionGrade = "Excellent"
                };
                await context.Vehicles.AddAsync(vehicle9);
                await context.SaveChangesAsync();

                var post15 = new Post
                {
                    AuthorId = members[1].Id,
                    PostType = PostType.Vehicle,
                    VehicleId = vehicle9.Id,
                    Title = "Tesla Model Y 2023 - Family SUV",
                    Description = "Tesla Model Y 2023, 10,000 km. Spacious 7-seater configuration, autopilot enabled, premium connectivity. Perfect family electric SUV.",
                    Price = 58000m,
                    LocationAddress = "Long Bien District, Hanoi",
                    Version = PostVersion.Vip,
                    Status = PostStatus.Active,
                    PublishedAt = DateTime.UtcNow.AddDays(-7),
                    ExpiresAt = DateTime.UtcNow.AddDays(23),
                    PhotoUrls = new List<string> { "https://images.unsplash.com/photo-1579508542697-bb18e7d9aeaa?ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&q=80&w=1170" }
                };

                await context.Posts.AddRangeAsync(new[] { post1, post2, post3, post4, post5, post6, post7, post8, post9, post10, post11, post12, post13, post14, post15 });
                await context.SaveChangesAsync();
            }
        }
    }
}
