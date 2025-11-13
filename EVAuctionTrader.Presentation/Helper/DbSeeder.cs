using EVAuctionTrader.Business.Utils;
using EVAuctionTrader.BusinessObject.Enums;
using EVAuctionTrader.DataAccess;
using EVAuctionTrader.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace EVAuctionTrader.Presentation.Helper
{
    public static class DbSeeder
    {
        private static readonly Guid SystemUserId = Guid.Empty; // System user for seeding

        public static async Task SeedUsersAsync(EVAuctionTraderDbContext context)
        {
            if (!await context.Users.AnyAsync(u => u.Role == RoleType.Admin))
            {
                var passwordHasher = new PasswordHasher();
                var admin = new User
                {
                    Id = Guid.NewGuid(),
                    FullName = "Admin User",
                    Email = "admin@gmail.com",
                    Phone = "0786315267",
                    PasswordHash = passwordHasher.HashPassword("1@"),
                    Role = RoleType.Admin,
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = SystemUserId
                };

                var wallet = new Wallet
                {
                    Id = Guid.NewGuid(),
                    User = admin,
                    Balance = 1000000m,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = SystemUserId
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
                        Id = Guid.NewGuid(),
                        FullName = "Member 1",
                        Email = "customer1@gmail.com",
                        Phone = "0786315268",
                        PasswordHash = passwordHasher.HashPassword("1@"),
                        Role = RoleType.Member,
                        Status = "Active",
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = SystemUserId
                    },
                    new User
                    {
                        Id = Guid.NewGuid(),
                        FullName = "Member 2",
                        Email = "customer2@gmail.com",
                        Phone = "0786315268",
                        PasswordHash = passwordHasher.HashPassword("1@"),
                        Role = RoleType.Member,
                        Status = "Active",
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = SystemUserId
                    }
                };
                await context.Users.AddRangeAsync(customer);

                var wallets = new List<Wallet>
                {
                    new Wallet
                    {
                        Id = Guid.NewGuid(),
                        User = customer[0],
                        Balance = 50000m,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = SystemUserId
                    },
                    new Wallet
                    {
                        Id = Guid.NewGuid(),
                        User = customer[1],
                        Balance = 75000m,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = SystemUserId
                    }
                };
                await context.Wallets.AddRangeAsync(wallets);
            }

            await context.SaveChangesAsync();
        }

        public static async Task SeedPostsWithVehiclesAndBatteriesAsync(EVAuctionTraderDbContext context)
        {
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

                // Seed Comments for Posts
                var comments = new List<PostComment>
                {
                    // Comments for Post 1 - Tesla Model 3
                    new PostComment
                    {
                        PostId = post1.Id,
                        AuthorId = members[1].Id,
                        Body = "This looks like a great deal! Is the autopilot feature included?"
                    },
                    new PostComment
                    {
                        PostId = post1.Id,
                        AuthorId = members[0].Id,
                        Body = "Yes, full self-driving capability is included. The car has all premium features!"
                    },

                    // Comments for Post 2 - CATL Battery
                    new PostComment
                    {
                        PostId = post2.Id,
                        AuthorId = members[1].Id,
                        Body = "Can this battery be used for a Nissan Leaf?"
                    },

                    // Comments for Post 3 - VinFast VF8
                    new PostComment
                    {
                        PostId = post3.Id,
                        AuthorId = members[0].Id,
                        Body = "Interested! Can I schedule a test drive this weekend?"
                    },
                    new PostComment
                    {
                        PostId = post3.Id,
                        AuthorId = members[1].Id,
                        Body = "Sure! I'm available on Saturday morning. Let me know what time works for you."
                    },
                    new PostComment
                    {
                        PostId = post3.Id,
                        AuthorId = members[0].Id,
                        Body = "Great! How about 10 AM on Saturday?"
                    },

                    // Comments for Post 5 - BYD Battery
                    new PostComment
                    {
                        PostId = post5.Id,
                        AuthorId = members[0].Id,
                        Body = "What's the warranty period remaining on this battery?"
                    },
                    new PostComment
                    {
                        PostId = post5.Id,
                        AuthorId = members[1].Id,
                        Body = "The battery still has 5 years of warranty left from BYD."
                    },

                    // Comments for Post 6 - Nissan Leaf
                    new PostComment
                    {
                        PostId = post6.Id,
                        AuthorId = members[1].Id,
                        Body = "Does it come with the original charging cable?"
                    },

                    // Comments for Post 7 - LG Chem Battery
                    new PostComment
                    {
                        PostId = post7.Id,
                        AuthorId = members[1].Id,
                        Body = "Is this compatible with Tesla Model 3?"
                    },
                    new PostComment
                    {
                        PostId = post7.Id,
                        AuthorId = members[0].Id,
                        Body = "Yes, it's compatible! The connector type matches perfectly."
                    },

                    // Comments for Post 8 - BMW i3
                    new PostComment
                    {
                        PostId = post8.Id,
                        AuthorId = members[0].Id,
                        Body = "Beautiful car! What's the real-world range you're getting?"
                    },
                    new PostComment
                    {
                        PostId = post8.Id,
                        AuthorId = members[1].Id,
                        Body = "I'm getting around 200km in city driving. Very efficient for daily commute!"
                    },
                    new PostComment
                    {
                        PostId = post8.Id,
                        AuthorId = members[0].Id,
                        Body = "That's impressive! I'll definitely consider this."
                    },

                    // Comments for Post 9 - Panasonic Battery
                    new PostComment
                    {
                        PostId = post9.Id,
                        AuthorId = members[0].Id,
                        Body = "How long does a full charge take with this battery?"
                    },

                    // Comments for Post 10 - Chevrolet Bolt
                    new PostComment
                    {
                        PostId = post10.Id,
                        AuthorId = members[1].Id,
                        Body = "Is the price negotiable? I'm very interested!"
                    },
                    new PostComment
                    {
                        PostId = post10.Id,
                        AuthorId = members[0].Id,
                        Body = "Yes, we can discuss the price. Please message me directly."
                    },

                    // Comments for Post 11 - Samsung SDI Battery
                    new PostComment
                    {
                        PostId = post11.Id,
                        AuthorId = members[1].Id,
                        Body = "Can you provide the test report for this battery?"
                    },

                    // Comments for Post 12 - Audi e-tron
                    new PostComment
                    {
                        PostId = post12.Id,
                        AuthorId = members[0].Id,
                        Body = "Wow! This is exactly what I've been looking for. Is it still available?"
                    },
                    new PostComment
                    {
                        PostId = post12.Id,
                        AuthorId = members[1].Id,
                        Body = "Yes, still available! Would you like to come see it?"
                    },

                    // Comments for Post 13 - SK Innovation Battery
                    new PostComment
                    {
                        PostId = post13.Id,
                        AuthorId = members[0].Id,
                        Body = "What's the charging speed capacity of this battery?"
                    },
                    new PostComment
                    {
                        PostId = post13.Id,
                        AuthorId = members[1].Id,
                        Body = "It supports up to 150kW DC fast charging. Very quick!"
                    },

                    // Comments for Post 14 - Kia EV6
                    new PostComment
                    {
                        PostId = post14.Id,
                        AuthorId = members[1].Id,
                        Body = "Absolutely stunning car! Does it have the GT-Line package?"
                    },

                    // Comments for Post 15 - Tesla Model Y
                    new PostComment
                    {
                        PostId = post15.Id,
                        AuthorId = members[0].Id,
                        Body = "Perfect for my family! Can we arrange a viewing?"
                    },
                    new PostComment
                    {
                        PostId = post15.Id,
                        AuthorId = members[1].Id,
                        Body = "Of course! I'm free this week. Let me know your preferred time."
                    }
                };

                await context.PostComments.AddRangeAsync(comments);
                await context.SaveChangesAsync();
            }
        }

        public static async Task SeedAuctionsAsync(EVAuctionTraderDbContext context)
        {
            if (!await context.Auctions.AnyAsync())
            {
                var admin = await context.Users.FirstOrDefaultAsync(u => u.Role == RoleType.Admin);
                if (admin == null) return;

                var member1 = await context.Users.FirstOrDefaultAsync(u => u.Email == "customer1@gmail.com");
                var member2 = await context.Users.FirstOrDefaultAsync(u => u.Email == "customer2@gmail.com");

                if (member1 == null || member2 == null) return;

                // Auction 1 - Tesla Model S (Live)
                var vehicle1 = new Vehicle
                {
                    OwnerId = admin.Id,
                    Brand = "Tesla",
                    Model = "Model S",
                    Year = 2023,
                    OdometerKm = 5000,
                    ConditionGrade = "Excellent"
                };
                await context.Vehicles.AddAsync(vehicle1);
                await context.SaveChangesAsync();

                var auction1 = new Auction
                {
                    CreatedBy = admin.Id,
                    AuctionType = AuctionType.Vehicle,
                    VehicleId = vehicle1.Id,
                    Title = "Tesla Model S 2023 - Premium Electric Sedan",
                    Description = "Brand new Tesla Model S with only 5,000 km. Full self-driving capability, premium interior, and all the latest features. Don't miss this opportunity!",
                    StartPrice = 60000m,
                    MinIncrement = 500m,
                    DepositRate = 0.20m,
                    CurrentPrice = 61000m,
                    StartTime = DateTime.UtcNow.AddHours(-2),
                    EndTime = DateTime.UtcNow.AddHours(4),
                    Status = AuctionStatus.Running,
                    PhotoUrl = "https://images.unsplash.com/photo-1617788138017-80ad40651399?w=800"
                };
                await context.Auctions.AddAsync(auction1);
                await context.SaveChangesAsync();

                // Add bids to auction 1
                var bidsAuction1 = new List<Bid>
                {
                    new Bid
                    {
                        AuctionId = auction1.Id,
                        BidderId = member2.Id,
                        Amount = 60500m,
                        CreatedAt = DateTime.UtcNow.AddMinutes(-90)
                    },
                    new Bid
                    {
                        AuctionId = auction1.Id,
                        BidderId = member1.Id,
                        Amount = 61000m,
                        CreatedAt = DateTime.UtcNow.AddMinutes(-60)
                    }
                };
                await context.Bids.AddRangeAsync(bidsAuction1);
                await context.SaveChangesAsync();

                // Auction 2 - LG Chem Battery (Live)
                var battery1 = new Battery
                {
                    OwnerId = admin.Id,
                    Manufacturer = "LG Chem",
                    Chemistry = "Lithium-ion NCM",
                    CapacityKwh = 82.0m,
                    CycleCount = 300,
                    SohPercent = 95.0m,
                    VoltageV = 400.0m,
                    ConnectorType = "CCS2"
                };
                await context.Batteries.AddAsync(battery1);
                await context.SaveChangesAsync();

                var auction2 = new Auction
                {
                    CreatedBy = admin.Id,
                    AuctionType = AuctionType.Battery,
                    BatteryId = battery1.Id,
                    Title = "LG Chem 82kWh Battery Pack - 95% SOH",
                    Description = "High-capacity LG Chem battery with excellent health. Only 300 cycles, perfect for long-range EVs. Includes warranty and certification.",
                    StartPrice = 8000m,
                    MinIncrement = 200m,
                    DepositRate = 0.20m,
                    CurrentPrice = 8400m,
                    StartTime = DateTime.UtcNow.AddHours(-1),
                    EndTime = DateTime.UtcNow.AddHours(5),
                    Status = AuctionStatus.Running,
                    PhotoUrl = "https://images.unsplash.com/photo-1622062929134-a8fa99b46f56?w=800"
                };
                await context.Auctions.AddAsync(auction2);
                await context.SaveChangesAsync();

                // Add bids to auction 2
                var bidsAuction2 = new List<Bid>
                {
                    new Bid
                    {
                        AuctionId = auction2.Id,
                        BidderId = member1.Id,
                        Amount = 8200m,
                        CreatedAt = DateTime.UtcNow.AddMinutes(-45)
                    },
                    new Bid
                    {
                        AuctionId = auction2.Id,
                        BidderId = member2.Id,
                        Amount = 8400m,
                        CreatedAt = DateTime.UtcNow.AddMinutes(-30)
                    }
                };
                await context.Bids.AddRangeAsync(bidsAuction2);
                await context.SaveChangesAsync();

                // Auction 3 - Porsche Taycan (Scheduled)
                var vehicle2 = new Vehicle
                {
                    OwnerId = admin.Id,
                    Brand = "Porsche",
                    Model = "Taycan",
                    Year = 2024,
                    OdometerKm = 1000,
                    ConditionGrade = "Excellent"
                };
                await context.Vehicles.AddAsync(vehicle2);
                await context.SaveChangesAsync();

                var auction3 = new Auction
                {
                    CreatedBy = admin.Id,
                    AuctionType = AuctionType.Vehicle,
                    VehicleId = vehicle2.Id,
                    Title = "Porsche Taycan 2024 - Luxury Performance EV",
                    Description = "Nearly brand new Porsche Taycan with only 1,000 km. Stunning performance, luxurious interior, and cutting-edge technology. Auction starts tomorrow!",
                    StartPrice = 95000m,
                    MinIncrement = 1000m,
                    DepositRate = 0.25m,
                    CurrentPrice = 95000m,
                    StartTime = DateTime.UtcNow.AddHours(6),
                    EndTime = DateTime.UtcNow.AddHours(30),
                    Status = AuctionStatus.Scheduled,
                    PhotoUrl = "https://images.unsplash.com/photo-1614162692292-7ac56d7f7f1e?w=800"
                };
                await context.Auctions.AddAsync(auction3);
                await context.SaveChangesAsync();

                // Auction 4 - BYD Battery (Scheduled)
                var battery2 = new Battery
                {
                    OwnerId = admin.Id,
                    Manufacturer = "BYD",
                    Chemistry = "LFP (Lithium Iron Phosphate)",
                    CapacityKwh = 100.0m,
                    CycleCount = 100,
                    SohPercent = 98.0m,
                    VoltageV = 350.0m,
                    ConnectorType = "CCS2"
                };
                await context.Batteries.AddAsync(battery2);
                await context.SaveChangesAsync();

                var auction4 = new Auction
                {
                    CreatedBy = admin.Id,
                    AuctionType = AuctionType.Battery,
                    BatteryId = battery2.Id,
                    Title = "BYD LFP 100kWh Battery - Like New",
                    Description = "Massive BYD LFP battery with 100kWh capacity. Only 100 cycles, 98% health. Perfect for commercial vehicles or premium EVs. Auction starts soon!",
                    StartPrice = 12000m,
                    MinIncrement = 500m,
                    DepositRate = 0.20m,
                    CurrentPrice = 12000m,
                    StartTime = DateTime.UtcNow.AddHours(8),
                    EndTime = DateTime.UtcNow.AddHours(32),
                    Status = AuctionStatus.Scheduled,
                    PhotoUrl = "https://images.unsplash.com/photo-1694889649703-e86125c14fe2?w=800"
                };
                await context.Auctions.AddAsync(auction4);
                await context.SaveChangesAsync();

                // Auction 5 - Mercedes EQS (Ended - with winner)
                var vehicle3 = new Vehicle
                {
                    OwnerId = admin.Id,
                    Brand = "Mercedes-Benz",
                    Model = "EQS",
                    Year = 2023,
                    OdometerKm = 8000,
                    ConditionGrade = "Very Good"
                };
                await context.Vehicles.AddAsync(vehicle3);
                await context.SaveChangesAsync();

                var auction5 = new Auction
                {
                    CreatedBy = admin.Id,
                    AuctionType = AuctionType.Vehicle,
                    VehicleId = vehicle3.Id,
                    Title = "Mercedes-Benz EQS 2023 - Luxury Flagship",
                    Description = "Mercedes EQS with 8,000 km. The pinnacle of electric luxury with advanced MBUX system and stunning range.",
                    StartPrice = 80000m,
                    MinIncrement = 1000m,
                    DepositRate = 0.25m,
                    CurrentPrice = 87000m,
                    StartTime = DateTime.UtcNow.AddDays(-2),
                    EndTime = DateTime.UtcNow.AddHours(-1),
                    Status = AuctionStatus.Ended,
                    WinnerId = member1.Id,
                    PhotoUrl = "https://images.unsplash.com/photo-1618843479313-40f8afb4b4d8?w=800"
                };
                await context.Auctions.AddAsync(auction5);
                await context.SaveChangesAsync();

                // Add bids to ended auction
                var bidsAuction5 = new List<Bid>
                {
                    new Bid
                    {
                        AuctionId = auction5.Id,
                        BidderId = member1.Id,
                        Amount = 81000m,
                        CreatedAt = DateTime.UtcNow.AddHours(-12)
                    },
                    new Bid
                    {
                        AuctionId = auction5.Id,
                        BidderId = member2.Id,
                        Amount = 83000m,
                        CreatedAt = DateTime.UtcNow.AddHours(-10)
                    },
                    new Bid
                    {
                        AuctionId = auction5.Id,
                        BidderId = member1.Id,
                        Amount = 85000m,
                        CreatedAt = DateTime.UtcNow.AddHours(-8)
                    },
                    new Bid
                    {
                        AuctionId = auction5.Id,
                        BidderId = member2.Id,
                        Amount = 86000m,
                        CreatedAt = DateTime.UtcNow.AddHours(-6)
                    },
                    new Bid
                    {
                        AuctionId = auction5.Id,
                        BidderId = member1.Id,
                        Amount = 87000m,
                        CreatedAt = DateTime.UtcNow.AddHours(-4)
                    }
                };
                await context.Bids.AddRangeAsync(bidsAuction5);
                await context.SaveChangesAsync();
            }
        }

        public static async Task SeedFeesAsync(EVAuctionTraderDbContext context)
        {
            if (!await context.Fees.AnyAsync())
            {
                var vipPostFee = new Fee
                {
                    Id = Guid.NewGuid(),
                    Type = FeeType.VipPostFee,
                    Amount = 5m,
                    Description = "Fee charged for creating a VIP post listing. VIP posts are displayed for 30 days and have premium placement.",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = SystemUserId
                };

                await context.Fees.AddAsync(vipPostFee);
                await context.SaveChangesAsync();
            }
        }

        public static async Task SeedWalletTransactionsAsync(EVAuctionTraderDbContext context)
        {
            if (!await context.WalletTransactions.AnyAsync())
            {
                var member1 = await context.Users
                    .Include(u => u.Wallets)
                    .FirstOrDefaultAsync(u => u.Email == "customer1@gmail.com");
                var member2 = await context.Users
                    .Include(u => u.Wallets)
                    .FirstOrDefaultAsync(u => u.Email == "customer2@gmail.com");

                if (member1 == null || member2 == null) return;

                var wallet1 = member1.Wallets?.FirstOrDefault();
                var wallet2 = member2.Wallets?.FirstOrDefault();

                if (wallet1 == null || wallet2 == null) return;

                // Get some posts and auctions for linking
                var posts = await context.Posts.Take(5).ToListAsync();
                var auctions = await context.Auctions.Take(3).ToListAsync();

                var transactions = new List<WalletTransaction>();

                // Member 1 Transactions
                var member1Balance = 50000m;

                // 1. Initial Top-up
                member1Balance += 50000m;
                transactions.Add(new WalletTransaction
                {
                    Id = Guid.NewGuid(),
                    WalletId = wallet1.Id,
                    Type = WalletTransactionType.Topup,
                    Amount = 50000m,
                    BalanceAfter = member1Balance,
                    Status = WalletTransactionStatus.Succeeded,
                    CreatedAt = DateTime.UtcNow.AddMonths(-6),
                    CreatedBy = member1.Id
                });

                // 2. VIP Post Fee - 6 months ago
                if (posts.Count > 0)
                {
                    member1Balance -= 5m;
                    transactions.Add(new WalletTransaction
                    {
                        Id = Guid.NewGuid(),
                        WalletId = wallet1.Id,
                        Type = WalletTransactionType.PostFee,
                        Amount = 5m,
                        BalanceAfter = member1Balance,
                        Status = WalletTransactionStatus.Succeeded,
                        PostId = posts[0].Id,
                        CreatedAt = DateTime.UtcNow.AddMonths(-6).AddDays(2),
                        CreatedBy = member1.Id
                    });
                }

                // 3. VIP Post Fee - 5 months ago
                if (posts.Count > 1)
                {
                    member1Balance -= 5m;
                    transactions.Add(new WalletTransaction
                    {
                        Id = Guid.NewGuid(),
                        WalletId = wallet1.Id,
                        Type = WalletTransactionType.PostFee,
                        Amount = 5m,
                        BalanceAfter = member1Balance,
                        Status = WalletTransactionStatus.Succeeded,
                        PostId = posts[1].Id,
                        CreatedAt = DateTime.UtcNow.AddMonths(-5).AddDays(5),
                        CreatedBy = member1.Id
                    });
                }

                // 4. VIP Post Fee - 4 months ago
                member1Balance -= 5m;
                transactions.Add(new WalletTransaction
                {
                    Id = Guid.NewGuid(),
                    WalletId = wallet1.Id,
                    Type = WalletTransactionType.PostFee,
                    Amount = 5m,
                    BalanceAfter = member1Balance,
                    Status = WalletTransactionStatus.Succeeded,
                    PostId = posts.Count > 2 ? posts[2].Id : posts[0].Id,
                    CreatedAt = DateTime.UtcNow.AddMonths(-4).AddDays(3),
                    CreatedBy = member1.Id
                });

                // 5. Top-up - 4 months ago
                member1Balance += 30000m;
                transactions.Add(new WalletTransaction
                {
                    Id = Guid.NewGuid(),
                    WalletId = wallet1.Id,
                    Type = WalletTransactionType.Topup,
                    Amount = 30000m,
                    BalanceAfter = member1Balance,
                    Status = WalletTransactionStatus.Succeeded,
                    CreatedAt = DateTime.UtcNow.AddMonths(-4).AddDays(10),
                    CreatedBy = member1.Id
                });

                // 6. VIP Post Fee - 3 months ago
                member1Balance -= 5m;
                transactions.Add(new WalletTransaction
                {
                    Id = Guid.NewGuid(),
                    WalletId = wallet1.Id,
                    Type = WalletTransactionType.PostFee,
                    Amount = 5m,
                    BalanceAfter = member1Balance,
                    Status = WalletTransactionStatus.Succeeded,
                    PostId = posts.Count > 3 ? posts[3].Id : posts[0].Id,
                    CreatedAt = DateTime.UtcNow.AddMonths(-3).AddDays(7),
                    CreatedBy = member1.Id
                });

                // 7. VIP Post Fee - 3 months ago (another one)
                member1Balance -= 5m;
                transactions.Add(new WalletTransaction
                {
                    Id = Guid.NewGuid(),
                    WalletId = wallet1.Id,
                    Type = WalletTransactionType.PostFee,
                    Amount = 5m,
                    BalanceAfter = member1Balance,
                    Status = WalletTransactionStatus.Succeeded,
                    PostId = posts.Count > 4 ? posts[4].Id : posts[0].Id,
                    CreatedAt = DateTime.UtcNow.AddMonths(-3).AddDays(15),
                    CreatedBy = member1.Id
                });

                // 8. Auction Deposit Hold - 2 months ago
                if (auctions.Count > 0)
                {
                    var depositAmount = auctions[0].StartPrice * auctions[0].DepositRate;
                    member1Balance -= depositAmount;
                    transactions.Add(new WalletTransaction
                    {
                        Id = Guid.NewGuid(),
                        WalletId = wallet1.Id,
                        Type = WalletTransactionType.AuctionHold,
                        Amount = depositAmount,
                        BalanceAfter = member1Balance,
                        Status = WalletTransactionStatus.Succeeded,
                        AuctionId = auctions[0].Id,
                        CreatedAt = DateTime.UtcNow.AddMonths(-2).AddDays(5),
                        CreatedBy = member1.Id
                    });
                }

                // 9. VIP Post Fee - 2 months ago
                member1Balance -= 5m;
                transactions.Add(new WalletTransaction
                {
                    Id = Guid.NewGuid(),
                    WalletId = wallet1.Id,
                    Type = WalletTransactionType.PostFee,
                    Amount = 5m,
                    BalanceAfter = member1Balance,
                    Status = WalletTransactionStatus.Succeeded,
                    PostId = posts.Count > 0 ? posts[0].Id : null,
                    CreatedAt = DateTime.UtcNow.AddMonths(-2).AddDays(12),
                    CreatedBy = member1.Id
                });

                // 10. Auction Deposit Release - 2 months ago
                if (auctions.Count > 0)
                {
                    var releaseAmount = auctions[0].StartPrice * auctions[0].DepositRate;
                    member1Balance += releaseAmount;
                    transactions.Add(new WalletTransaction
                    {
                        Id = Guid.NewGuid(),
                        WalletId = wallet1.Id,
                        Type = WalletTransactionType.AuctionRelease,
                        Amount = releaseAmount,
                        BalanceAfter = member1Balance,
                        Status = WalletTransactionStatus.Succeeded,
                        AuctionId = auctions[0].Id,
                        CreatedAt = DateTime.UtcNow.AddMonths(-2).AddDays(20),
                        CreatedBy = member1.Id
                    });
                }

                // 11. Top-up - 1 month ago
                member1Balance += 20000m;
                transactions.Add(new WalletTransaction
                {
                    Id = Guid.NewGuid(),
                    WalletId = wallet1.Id,
                    Type = WalletTransactionType.Topup,
                    Amount = 20000m,
                    BalanceAfter = member1Balance,
                    Status = WalletTransactionStatus.Succeeded,
                    CreatedAt = DateTime.UtcNow.AddMonths(-1).AddDays(3),
                    CreatedBy = member1.Id
                });

                // 12. VIP Post Fee - 1 month ago
                member1Balance -= 5m;
                transactions.Add(new WalletTransaction
                {
                    Id = Guid.NewGuid(),
                    WalletId = wallet1.Id,
                    Type = WalletTransactionType.PostFee,
                    Amount = 5m,
                    BalanceAfter = member1Balance,
                    Status = WalletTransactionStatus.Succeeded,
                    PostId = posts.Count > 1 ? posts[1].Id : null,
                    CreatedAt = DateTime.UtcNow.AddMonths(-1).AddDays(8),
                    CreatedBy = member1.Id
                });

                // 13. VIP Post Fee - 1 month ago (another)
                member1Balance -= 5m;
                transactions.Add(new WalletTransaction
                {
                    Id = Guid.NewGuid(),
                    WalletId = wallet1.Id,
                    Type = WalletTransactionType.PostFee,
                    Amount = 5m,
                    BalanceAfter = member1Balance,
                    Status = WalletTransactionStatus.Succeeded,
                    PostId = posts.Count > 2 ? posts[2].Id : null,
                    CreatedAt = DateTime.UtcNow.AddMonths(-1).AddDays(18),
                    CreatedBy = member1.Id
                });

                // 14. VIP Post Fee - Current month
                member1Balance -= 5m;
                transactions.Add(new WalletTransaction
                {
                    Id = Guid.NewGuid(),
                    WalletId = wallet1.Id,
                    Type = WalletTransactionType.PostFee,
                    Amount = 5m,
                    BalanceAfter = member1Balance,
                    Status = WalletTransactionStatus.Succeeded,
                    PostId = posts.Count > 3 ? posts[3].Id : null,
                    CreatedAt = DateTime.UtcNow.AddDays(-10),
                    CreatedBy = member1.Id
                });

                // 15. VIP Post Fee - Current month (another)
                member1Balance -= 5m;
                transactions.Add(new WalletTransaction
                {
                    Id = Guid.NewGuid(),
                    WalletId = wallet1.Id,
                    Type = WalletTransactionType.PostFee,
                    Amount = 5m,
                    BalanceAfter = member1Balance,
                    Status = WalletTransactionStatus.Succeeded,
                    PostId = posts.Count > 4 ? posts[4].Id : null,
                    CreatedAt = DateTime.UtcNow.AddDays(-5),
                    CreatedBy = member1.Id
                });

                // 16. VIP Post Fee - Current month (third)
                member1Balance -= 5m;
                transactions.Add(new WalletTransaction
                {
                    Id = Guid.NewGuid(),
                    WalletId = wallet1.Id,
                    Type = WalletTransactionType.PostFee,
                    Amount = 5m,
                    BalanceAfter = member1Balance,
                    Status = WalletTransactionStatus.Succeeded,
                    PostId = posts.Count > 0 ? posts[0].Id : null,
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    CreatedBy = member1.Id
                });

                // Member 2 Transactions
                var member2Balance = 75000m;

                // 1. Initial balance - 6 months ago
                transactions.Add(new WalletTransaction
                {
                    Id = Guid.NewGuid(),
                    WalletId = wallet2.Id,
                    Type = WalletTransactionType.Topup,
                    Amount = 75000m,
                    BalanceAfter = member2Balance,
                    Status = WalletTransactionStatus.Succeeded,
                    CreatedAt = DateTime.UtcNow.AddMonths(-6),
                    CreatedBy = member2.Id
                });

                // 2. VIP Post Fee - 6 months ago
                if (posts.Count > 2)
                {
                    member2Balance -= 5m;
                    transactions.Add(new WalletTransaction
                    {
                        Id = Guid.NewGuid(),
                        WalletId = wallet2.Id,
                        Type = WalletTransactionType.PostFee,
                        Amount = 5m,
                        BalanceAfter = member2Balance,
                        Status = WalletTransactionStatus.Succeeded,
                        PostId = posts[2].Id,
                        CreatedAt = DateTime.UtcNow.AddMonths(-6).AddDays(5),
                        CreatedBy = member2.Id
                    });
                }

                // 3. VIP Post Fee - 5 months ago
                if (posts.Count > 3)
                {
                    member2Balance -= 5m;
                    transactions.Add(new WalletTransaction
                    {
                        Id = Guid.NewGuid(),
                        WalletId = wallet2.Id,
                        Type = WalletTransactionType.PostFee,
                        Amount = 5m,
                        BalanceAfter = member2Balance,
                        Status = WalletTransactionStatus.Succeeded,
                        PostId = posts[3].Id,
                        CreatedAt = DateTime.UtcNow.AddMonths(-5).AddDays(8),
                        CreatedBy = member2.Id
                    });
                }

                // 4. Top-up - 5 months ago
                member2Balance += 20000m;
                transactions.Add(new WalletTransaction
                {
                    Id = Guid.NewGuid(),
                    WalletId = wallet2.Id,
                    Type = WalletTransactionType.Topup,
                    Amount = 20000m,
                    BalanceAfter = member2Balance,
                    Status = WalletTransactionStatus.Succeeded,
                    CreatedAt = DateTime.UtcNow.AddMonths(-5).AddDays(15),
                    CreatedBy = member2.Id
                });

                // 5. VIP Post Fee - 4 months ago
                member2Balance -= 5m;
                transactions.Add(new WalletTransaction
                {
                    Id = Guid.NewGuid(),
                    WalletId = wallet2.Id,
                    Type = WalletTransactionType.PostFee,
                    Amount = 5m,
                    BalanceAfter = member2Balance,
                    Status = WalletTransactionStatus.Succeeded,
                    PostId = posts.Count > 4 ? posts[4].Id : posts[0].Id,
                    CreatedAt = DateTime.UtcNow.AddMonths(-4).AddDays(6),
                    CreatedBy = member2.Id
                });

                // 6. VIP Post Fee - 4 months ago (another)
                member2Balance -= 5m;
                transactions.Add(new WalletTransaction
                {
                    Id = Guid.NewGuid(),
                    WalletId = wallet2.Id,
                    Type = WalletTransactionType.PostFee,
                    Amount = 5m,
                    BalanceAfter = member2Balance,
                    Status = WalletTransactionStatus.Succeeded,
                    PostId = posts.Count > 0 ? posts[0].Id : null,
                    CreatedAt = DateTime.UtcNow.AddMonths(-4).AddDays(14),
                    CreatedBy = member2.Id
                });

                // 7. Auction Hold - 3 months ago
                if (auctions.Count > 0)
                {
                    var depositAmount = auctions[0].StartPrice * auctions[0].DepositRate;
                    member2Balance -= depositAmount;
                    transactions.Add(new WalletTransaction
                    {
                        Id = Guid.NewGuid(),
                        WalletId = wallet2.Id,
                        Type = WalletTransactionType.AuctionHold,
                        Amount = depositAmount,
                        BalanceAfter = member2Balance,
                        Status = WalletTransactionStatus.Succeeded,
                        AuctionId = auctions[0].Id,
                        CreatedAt = DateTime.UtcNow.AddMonths(-3).AddDays(5),
                        CreatedBy = member2.Id
                    });
                }

                // 8. VIP Post Fee - 3 months ago
                member2Balance -= 5m;
                transactions.Add(new WalletTransaction
                {
                    Id = Guid.NewGuid(),
                    WalletId = wallet2.Id,
                    Type = WalletTransactionType.PostFee,
                    Amount = 5m,
                    BalanceAfter = member2Balance,
                    Status = WalletTransactionStatus.Succeeded,
                    PostId = posts.Count > 1 ? posts[1].Id : null,
                    CreatedAt = DateTime.UtcNow.AddMonths(-3).AddDays(10),
                    CreatedBy = member2.Id
                });

                // 9. Auction Capture - 3 months ago
                if (auctions.Count > 0)
                {
                    var captureAmount = auctions[0].CurrentPrice;
                    member2Balance -= captureAmount;
                    transactions.Add(new WalletTransaction
                    {
                        Id = Guid.NewGuid(),
                        WalletId = wallet2.Id,
                        Type = WalletTransactionType.AuctionCapture,
                        Amount = captureAmount,
                        BalanceAfter = member2Balance,
                        Status = WalletTransactionStatus.Succeeded,
                        AuctionId = auctions[0].Id,
                        CreatedAt = DateTime.UtcNow.AddMonths(-3).AddDays(18),
                        CreatedBy = member2.Id
                    });
                }

                // 10. Top-up - 2 months ago
                member2Balance += 50000m;
                transactions.Add(new WalletTransaction
                {
                    Id = Guid.NewGuid(),
                    WalletId = wallet2.Id,
                    Type = WalletTransactionType.Topup,
                    Amount = 50000m,
                    BalanceAfter = member2Balance,
                    Status = WalletTransactionStatus.Succeeded,
                    CreatedAt = DateTime.UtcNow.AddMonths(-2).AddDays(2),
                    CreatedBy = member2.Id
                });

                // 11. VIP Post Fee - 2 months ago
                member2Balance -= 5m;
                transactions.Add(new WalletTransaction
                {
                    Id = Guid.NewGuid(),
                    WalletId = wallet2.Id,
                    Type = WalletTransactionType.PostFee,
                    Amount = 5m,
                    BalanceAfter = member2Balance,
                    Status = WalletTransactionStatus.Succeeded,
                    PostId = posts.Count > 2 ? posts[2].Id : null,
                    CreatedAt = DateTime.UtcNow.AddMonths(-2).AddDays(9),
                    CreatedBy = member2.Id
                });

                // 12. VIP Post Fee - 2 months ago (another)
                member2Balance -= 5m;
                transactions.Add(new WalletTransaction
                {
                    Id = Guid.NewGuid(),
                    WalletId = wallet2.Id,
                    Type = WalletTransactionType.PostFee,
                    Amount = 5m,
                    BalanceAfter = member2Balance,
                    Status = WalletTransactionStatus.Succeeded,
                    PostId = posts.Count > 3 ? posts[3].Id : null,
                    CreatedAt = DateTime.UtcNow.AddMonths(-2).AddDays(16),
                    CreatedBy = member2.Id
                });

                // 13. Admin Adjustment (bonus) - 1 month ago
                member2Balance += 1000m;
                transactions.Add(new WalletTransaction
                {
                    Id = Guid.NewGuid(),
                    WalletId = wallet2.Id,
                    Type = WalletTransactionType.Adjust,
                    Amount = 1000m,
                    BalanceAfter = member2Balance,
                    Status = WalletTransactionStatus.Succeeded,
                    CreatedAt = DateTime.UtcNow.AddMonths(-1).AddDays(5),
                    CreatedBy = SystemUserId
                });

                // 14. VIP Post Fee - 1 month ago
                member2Balance -= 5m;
                transactions.Add(new WalletTransaction
                {
                    Id = Guid.NewGuid(),
                    WalletId = wallet2.Id,
                    Type = WalletTransactionType.PostFee,
                    Amount = 5m,
                    BalanceAfter = member2Balance,
                    Status = WalletTransactionStatus.Succeeded,
                    PostId = posts.Count > 4 ? posts[4].Id : null,
                    CreatedAt = DateTime.UtcNow.AddMonths(-1).AddDays(12),
                    CreatedBy = member2.Id
                });

                // 15. VIP Post Fee - 1 month ago (another)
                member2Balance -= 5m;
                transactions.Add(new WalletTransaction
                {
                    Id = Guid.NewGuid(),
                    WalletId = wallet2.Id,
                    Type = WalletTransactionType.PostFee,
                    Amount = 5m,
                    BalanceAfter = member2Balance,
                    Status = WalletTransactionStatus.Succeeded,
                    PostId = posts.Count > 0 ? posts[0].Id : null,
                    CreatedAt = DateTime.UtcNow.AddMonths(-1).AddDays(20),
                    CreatedBy = member2.Id
                });

                // 16. VIP Post Fee - Current month
                member2Balance -= 5m;
                transactions.Add(new WalletTransaction
                {
                    Id = Guid.NewGuid(),
                    WalletId = wallet2.Id,
                    Type = WalletTransactionType.PostFee,
                    Amount = 5m,
                    BalanceAfter = member2Balance,
                    Status = WalletTransactionStatus.Succeeded,
                    PostId = posts.Count > 1 ? posts[1].Id : null,
                    CreatedAt = DateTime.UtcNow.AddDays(-15),
                    CreatedBy = member2.Id
                });

                // 17. VIP Post Fee - Current month (another)
                member2Balance -= 5m;
                transactions.Add(new WalletTransaction
                {
                    Id = Guid.NewGuid(),
                    WalletId = wallet2.Id,
                    Type = WalletTransactionType.PostFee,
                    Amount = 5m,
                    BalanceAfter = member2Balance,
                    Status = WalletTransactionStatus.Succeeded,
                    PostId = posts.Count > 2 ? posts[2].Id : null,
                    CreatedAt = DateTime.UtcNow.AddDays(-8),
                    CreatedBy = member2.Id
                });

                // 18. Top-up - Current month
                member2Balance += 15000m;
                transactions.Add(new WalletTransaction
                {
                    Id = Guid.NewGuid(),
                    WalletId = wallet2.Id,
                    Type = WalletTransactionType.Topup,
                    Amount = 15000m,
                    BalanceAfter = member2Balance,
                    Status = WalletTransactionStatus.Succeeded,
                    CreatedAt = DateTime.UtcNow.AddDays(-3),
                    CreatedBy = member2.Id
                });

                // 19. VIP Post Fee - Current month (recent)
                member2Balance -= 5m;
                transactions.Add(new WalletTransaction
                {
                    Id = Guid.NewGuid(),
                    WalletId = wallet2.Id,
                    Type = WalletTransactionType.PostFee,
                    Amount = 5m,
                    BalanceAfter = member2Balance,
                    Status = WalletTransactionStatus.Succeeded,
                    PostId = posts.Count > 3 ? posts[3].Id : null,
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    CreatedBy = member2.Id
                });

                await context.WalletTransactions.AddRangeAsync(transactions);
                await context.SaveChangesAsync();
            }
        }
    }
}
