using EVAuctionTrader.Business.Interfaces;
using EVAuctionTrader.Business.Utils;
using EVAuctionTrader.BusinessObject.DTOs.BatteryDTOs;
using EVAuctionTrader.BusinessObject.DTOs.PostDTOs;
using EVAuctionTrader.BusinessObject.DTOs.VehicleDTOs;
using EVAuctionTrader.BusinessObject.Enums;
using EVAuctionTrader.DataAccess.Entities;
using EVAuctionTrader.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EVAuctionTrader.Business.Services
{
    public class PostService : IPostService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClaimsService _claimsService;
        private readonly ILogger<PostService> _logger;

        public PostService(IUnitOfWork unitOfWork, IClaimsService claimsService, ILogger<PostService> logger)
        {
            _unitOfWork = unitOfWork;
            _claimsService = claimsService;
            _logger = logger;
        }

        public async Task<PostResponseDto?> CreatePostAsync(PostRequestDto createPostDto)
        {
            try
            {
                _logger.LogInformation("Creating a new post.");
                if (createPostDto == null)
                {
                    _logger.LogWarning("CreatePostAsync failed: createPostDto is null.");
                    throw new ArgumentNullException(nameof(createPostDto));
                }
                var authorId = _claimsService.GetCurrentUserId;
                var author = await _unitOfWork.Users.GetByIdAsync(authorId);
                if (author == null)
                {
                    _logger.LogWarning($"CreatePostAsync failed: Author with ID {authorId} not found.");
                    throw new InvalidOperationException("Author not found.");
                }

                if (createPostDto.Vehicle == null && createPostDto.Battery == null)
                {
                    _logger.LogWarning("CreatePostAsync failed: Either Vehicle or Battery information must be provided.");
                    throw new ArgumentException("Either Vehicle or Battery information must be provided.");
                }

                var vehicleEntity = null as Vehicle;
                var batteryEntity = null as Battery;

                if (createPostDto.PostType == PostType.Vehicle && createPostDto.Vehicle != null)
                {
                    var checkVehicle = await _unitOfWork.Vehicles.FirstOrDefaultAsync(v =>
                        v.OwnerId == author.Id &&
                        v.Brand == createPostDto.Vehicle.Brand &&
                        v.Model == createPostDto.Vehicle.Model &&
                        v.Year == createPostDto.Vehicle.Year &&
                        v.OdometerKm == createPostDto.Vehicle.OdometerKm &&
                        v.ConditionGrade == createPostDto.Vehicle.ConditionGrade &&
                        !v.IsDeleted
                    );
                    if (checkVehicle != null)
                    {
                        vehicleEntity = checkVehicle;
                        _logger.LogInformation($"Existing vehicle entity found with ID: {vehicleEntity.Id}");
                    }
                    else
                    {
                        vehicleEntity = new Vehicle
                        {
                            OwnerId = author.Id,
                            Brand = createPostDto.Vehicle.Brand,
                            Model = createPostDto.Vehicle.Model,
                            Year = createPostDto.Vehicle.Year,
                            OdometerKm = createPostDto.Vehicle.OdometerKm,
                            ConditionGrade = createPostDto.Vehicle.ConditionGrade
                        };
                        await _unitOfWork.Vehicles.AddAsync(vehicleEntity);
                        _logger.LogInformation($"Vehicle entity created with ID: {vehicleEntity.Id}");
                    }
                }
                else if (createPostDto.PostType == PostType.Battery && createPostDto.Battery != null)
                {
                    var checkBattery = await _unitOfWork.Batteries.FirstOrDefaultAsync(b =>
                        b.OwnerId == author.Id &&
                        b.Manufacturer == createPostDto.Battery.Manufacturer &&
                        b.Chemistry == createPostDto.Battery.Chemistry &&
                        b.CapacityKwh == createPostDto.Battery.CapacityKwh &&
                        b.CycleCount == createPostDto.Battery.CycleCount &&
                        b.SohPercent == createPostDto.Battery.SohPercent &&
                        b.VoltageV == createPostDto.Battery.VoltageV &&
                        b.ConnectorType == createPostDto.Battery.ConnectorType &&
                        !b.IsDeleted
                    );
                    if (checkBattery != null)
                    {
                        batteryEntity = checkBattery;
                        _logger.LogInformation($"Existing battery entity found with ID: {batteryEntity.Id}");
                    }
                    else
                    {
                        batteryEntity = new Battery
                        {
                            OwnerId = author.Id,
                            Manufacturer = createPostDto.Battery.Manufacturer,
                            Chemistry = createPostDto.Battery.Chemistry,
                            CapacityKwh = createPostDto.Battery.CapacityKwh,
                            CycleCount = createPostDto.Battery.CycleCount,
                            SohPercent = createPostDto.Battery.SohPercent,
                            VoltageV = createPostDto.Battery.VoltageV,
                            ConnectorType = createPostDto.Battery.ConnectorType
                        };
                        await _unitOfWork.Batteries.AddAsync(batteryEntity);
                        _logger.LogInformation($"Battery entity created with ID: {batteryEntity.Id}");
                    }
                }
                else
                {
                    _logger.LogWarning("CreatePostAsync failed: Mismatch between PostType and provided details.");
                    throw new ArgumentException("Mismatch between PostType and provided details.");
                }

                // ✅ FIX: Convert all DateTime to UTC
                DateTime publishedAt = createPostDto.PublishedAt.HasValue
                    ? DateTime.SpecifyKind(createPostDto.PublishedAt.Value, DateTimeKind.Utc)
                    : DateTime.UtcNow;

                DateTime expiresAt;
                if (createPostDto.Version == PostVersion.Free)
                {
                    expiresAt = publishedAt.AddDays(15);
                }
                else if (createPostDto.Version == PostVersion.Vip)
                {
                    expiresAt = publishedAt.AddDays(30);
                }
                else
                {
                    expiresAt = publishedAt.AddDays(15); // Default to Free
                }

                var postEntity = new Post
                {
                    AuthorId = author.Id,
                    PostType = createPostDto.PostType,
                    VehicleId = vehicleEntity != null ? vehicleEntity.Id : null,
                    BatteryId = batteryEntity != null ? batteryEntity.Id : null,
                    Version = createPostDto.Version,
                    Title = createPostDto.Title,
                    Description = createPostDto.Description,
                    Price = createPostDto.Price,
                    LocationAddress = createPostDto.LocationAddress,
                    PhotoUrls = createPostDto.PhotoUrls,
                    Status = createPostDto.Status,
                    PublishedAt = publishedAt,  // ✅ UTC DateTime
                    ExpiresAt = expiresAt       // ✅ UTC DateTime
                };


                await _unitOfWork.Posts.AddAsync(postEntity);
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation($"Post created successfully with ID: {postEntity.Id}");


                return new PostResponseDto
                {
                    Id = postEntity.Id,
                    AuthorId = postEntity.AuthorId,
                    AuthorName = author.FullName,
                    PostType = postEntity.PostType,
                    Vehicle = vehicleEntity != null ? new VehicleResponseDto
                    {
                        Id = vehicleEntity.Id,
                        Brand = vehicleEntity.Brand,
                        Model = vehicleEntity.Model,
                        Year = vehicleEntity.Year,
                        OdometerKm = vehicleEntity.OdometerKm,
                        ConditionGrade = vehicleEntity.ConditionGrade
                    } : null,
                    Battery = batteryEntity != null ? new BatteryResponseDto
                    {
                        Id = batteryEntity.Id,
                        Manufacturer = batteryEntity.Manufacturer,
                        Chemistry = batteryEntity.Chemistry,
                        CapacityKwh = batteryEntity.CapacityKwh,
                        CycleCount = batteryEntity.CycleCount,
                        SohPercent = batteryEntity.SohPercent,
                        VoltageV = batteryEntity.VoltageV,
                        ConnectorType = batteryEntity.ConnectorType
                    } : null,
                    Version = postEntity.Version,
                    Title = postEntity.Title,
                    Description = postEntity.Description,
                    Price = postEntity.Price,
                    LocationAddress = postEntity.LocationAddress,
                    PhotoUrls = postEntity.PhotoUrls,
                    Status = postEntity.Status,
                    PublishedAt = postEntity.PublishedAt,
                    ExpiresAt = postEntity.ExpiresAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a new post.");
                throw;
            }
        }

        public async Task<Pagination<PostResponseDto>> GetAllPostsAsync
    (int pageNumber = 1,
    int pageSize = 10,
    string? search = null,
    PostType? postType = null,
    PostVersion? postVersion = null,
    PostStatus? postStatus = null,
    bool priceSort = true)
        {
            try
            {
                _logger.LogInformation("Retrieving paginated list of posts.");

                // ✅ Get current user to check role
                var currentUserId = _claimsService.GetCurrentUserId;
                var currentUser = currentUserId != Guid.Empty
                    ? await _unitOfWork.Users.GetByIdAsync(currentUserId)
                    : null;

                bool isAdmin = currentUser?.Role == RoleType.Admin;

                // ✅ Base query: Always exclude deleted posts
                var query = _unitOfWork.Posts.GetQueryable().Where(q => !q.IsDeleted);

                // ✅ If NOT admin: exclude banned posts (Status = Removed)
                if (!isAdmin)
                {
                    query = query.Where(q => q.Status != PostStatus.Removed);
                    _logger.LogInformation("Non-admin user: filtering out removed/banned posts.");
                }
                else
                {
                    _logger.LogInformation("Admin user: showing all posts including banned ones.");
                }

                // ✅ Auto-update status based on time
                var utcNow = DateTime.UtcNow;
                var postsToUpdate = await query
                    .Where(p =>
                        (p.PublishedAt <= utcNow && p.Status == PostStatus.Draft) ||
                        (p.ExpiresAt <= utcNow && p.Status == PostStatus.Active))
                    .ToListAsync();

                foreach (var item in postsToUpdate)
                {
                    if (item.PublishedAt <= utcNow && item.Status == PostStatus.Draft)
                        item.Status = PostStatus.Active;

                    if (item.ExpiresAt <= utcNow && item.Status == PostStatus.Active)
                        item.Status = PostStatus.Closed;
                }

                if (postsToUpdate.Any())
                {
                    await _unitOfWork.SaveChangesAsync();
                    _logger.LogInformation($"Updated status for {postsToUpdate.Count} posts.");
                }

                // ✅ Apply filters
                if (postType.HasValue)
                {
                    query = query.Where(p => p.PostType == postType.Value);
                }
                if (postStatus.HasValue)
                {
                    query = query.Where(p => p.Status == postStatus.Value);
                }
                if (postVersion.HasValue)
                {
                    query = query.Where(p => p.Version == postVersion.Value);
                }
                search = search?.ToLower();
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(p => p.Title.ToLower().Contains(search) || p.LocationAddress.ToLower().Contains(search));
                }

                query = priceSort
                    ? query.OrderBy(p => p.Price)
                    : query.OrderByDescending(p => p.Price);

                var totalCount = await query.CountAsync();

                var posts = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var postDtos = new List<PostResponseDto>();

                foreach (var post in posts)
                {
                    var author = await _unitOfWork.Users.GetByIdAsync(post.AuthorId);
                    if (author == null)
                    {
                        _logger.LogWarning($"GetAllPostsAsync warning: Author with ID {post.AuthorId} not found for post ID {post.Id}.");
                        continue; // Skip this post instead of throwing
                    }

                    VehicleResponseDto? vehicleDto = null;
                    BatteryResponseDto? batteryDto = null;

                    if (post.VehicleId.HasValue)
                    {
                        var vehicleEntity = await _unitOfWork.Vehicles.GetByIdAsync(post.VehicleId.Value);
                        if (vehicleEntity != null)
                        {
                            vehicleDto = new VehicleResponseDto
                            {
                                Id = vehicleEntity.Id,
                                Brand = vehicleEntity.Brand,
                                Model = vehicleEntity.Model,
                                Year = vehicleEntity.Year,
                                OdometerKm = vehicleEntity.OdometerKm,
                                ConditionGrade = vehicleEntity.ConditionGrade
                            };
                        }
                    }

                    if (post.BatteryId.HasValue)
                    {
                        var batteryEntity = await _unitOfWork.Batteries.GetByIdAsync(post.BatteryId.Value);
                        if (batteryEntity != null)
                        {
                            batteryDto = new BatteryResponseDto
                            {
                                Id = batteryEntity.Id,
                                Manufacturer = batteryEntity.Manufacturer,
                                Chemistry = batteryEntity.Chemistry,
                                CapacityKwh = batteryEntity.CapacityKwh,
                                CycleCount = batteryEntity.CycleCount,
                                SohPercent = batteryEntity.SohPercent,
                                VoltageV = batteryEntity.VoltageV,
                                ConnectorType = batteryEntity.ConnectorType
                            };
                        }
                    }

                    postDtos.Add(new PostResponseDto
                    {
                        Id = post.Id,
                        AuthorId = post.AuthorId,
                        AuthorName = author.FullName,
                        PostType = post.PostType,
                        Vehicle = vehicleDto,
                        Battery = batteryDto,
                        Version = post.Version,
                        Title = post.Title,
                        Description = post.Description,
                        Price = post.Price,
                        LocationAddress = post.LocationAddress,
                        PhotoUrls = post.PhotoUrls,
                        Status = post.Status,
                        PublishedAt = post.PublishedAt,
                        ExpiresAt = post.ExpiresAt
                    });
                }

                return new Pagination<PostResponseDto>(postDtos, totalCount, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving paginated list of posts.");
                throw;
            }
        }
        public async Task<Pagination<PostResponseDto>> GetAllMemberPostsAsync(
            int pageNumber = 1,
            int pageSize = 10,
            string? search = null,
            PostType? postType = null,
            PostVersion? postVersion = null,
            PostStatus? postStatus = null,
            bool priceSort = true)
        {
            try
            {
                _logger.LogInformation("Retrieving paginated list of posts for member.");
                var query = _unitOfWork.Posts.GetQueryable().Where(q => !q.IsDeleted);

                var currentUserId = _claimsService.GetCurrentUserId;
                query = query.Where(p => p.AuthorId == currentUserId);
                _logger.LogInformation($"Filtering posts by current user ID: {currentUserId}");

                var utcNow = DateTime.UtcNow;

                foreach (var item in query)
                {
                    if (item.PublishedAt <= utcNow && item.Status == PostStatus.Draft)
                        item.Status = PostStatus.Active;

                    if (item.ExpiresAt <= utcNow && item.Status == PostStatus.Active)
                        item.Status = PostStatus.Closed;
                }

                if (postType.HasValue)
                {
                    query = query.Where(p => p.PostType == postType.Value);
                }
                if (postStatus.HasValue)
                {
                    query = query.Where(p => p.Status == postStatus.Value);
                }
                if (postVersion.HasValue)
                {
                    query = query.Where(p => p.Version == postVersion.Value);
                }
                search = search?.ToLower();
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(p => p.Title.ToLower().Contains(search) || p.LocationAddress.ToLower().Contains(search));
                }

                query = priceSort
                    ? query.OrderBy(p => p.Price)
                    : query.OrderByDescending(p => p.Price);

                var totalCount = await query.CountAsync();

                var posts = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var postDtos = new List<PostResponseDto>();

                foreach (var post in posts)
                {
                    var author = await _unitOfWork.Users.GetByIdAsync(post.AuthorId);
                    if (author == null)
                    {
                        _logger.LogWarning($"GetAllMemberPostsAsync warning: Author with ID {post.AuthorId} not found for post ID {post.Id}.");
                        throw new InvalidOperationException("Author not found.");
                    }

                    VehicleResponseDto? vehicleDto = null;
                    BatteryResponseDto? batteryDto = null;
                    if (post.VehicleId.HasValue)
                    {
                        var vehicleEntity = await _unitOfWork.Vehicles.GetByIdAsync(post.VehicleId.Value);
                        if (vehicleEntity != null)
                        {
                            vehicleDto = new VehicleResponseDto
                            {
                                Id = vehicleEntity.Id,
                                Brand = vehicleEntity.Brand,
                                Model = vehicleEntity.Model,
                                Year = vehicleEntity.Year,
                                OdometerKm = vehicleEntity.OdometerKm,
                                ConditionGrade = vehicleEntity.ConditionGrade
                            };
                        }
                    }
                    if (post.BatteryId.HasValue)
                    {
                        var batteryEntity = await _unitOfWork.Batteries.GetByIdAsync(post.BatteryId.Value);
                        if (batteryEntity != null)
                        {
                            batteryDto = new BatteryResponseDto
                            {
                                Id = batteryEntity.Id,
                                Manufacturer = batteryEntity.Manufacturer,
                                Chemistry = batteryEntity.Chemistry,
                                CapacityKwh = batteryEntity.CapacityKwh,
                                CycleCount = batteryEntity.CycleCount,
                                SohPercent = batteryEntity.SohPercent,
                                VoltageV = batteryEntity.VoltageV,
                                ConnectorType = batteryEntity.ConnectorType
                            };
                        }
                    }
                    postDtos.Add(new PostResponseDto
                    {
                        Id = post.Id,
                        AuthorId = post.AuthorId,
                        AuthorName = author.FullName,
                        PostType = post.PostType,
                        Vehicle = vehicleDto,
                        Battery = batteryDto,
                        Version = post.Version,
                        Title = post.Title,
                        Description = post.Description,
                        Price = post.Price,
                        LocationAddress = post.LocationAddress,
                        PhotoUrls = post.PhotoUrls,
                        Status = post.Status,
                        PublishedAt = post.PublishedAt,
                        ExpiresAt = post.ExpiresAt
                    });
                }
                return new Pagination<PostResponseDto>(postDtos, totalCount, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving paginated list of posts.");
                throw;
            }
        }

        public async Task<PostResponseDto?> GetPostByIdAsync(Guid postId)
        {
            try
            {
                _logger.LogInformation($"Retrieving post with ID: {postId}");
                var postEntity = await _unitOfWork.Posts.GetByIdAsync(postId);

                if (postEntity == null || postEntity.IsDeleted)
                {
                    _logger.LogWarning($"GetPostByIdAsync failed: Post with ID {postId} not found.");
                    return null;
                }
                var author = await _unitOfWork.Users.GetByIdAsync(postEntity.AuthorId);
                if (author == null)
                {
                    _logger.LogWarning($"GetPostByIdAsync failed: Author with ID {postEntity.AuthorId} not found.");
                    throw new InvalidOperationException("Author not found.");
                }
                VehicleResponseDto? vehicleDto = null;
                BatteryResponseDto? batteryDto = null;
                if (postEntity.VehicleId.HasValue)
                {
                    var vehicleEntity = await _unitOfWork.Vehicles.GetByIdAsync(postEntity.VehicleId.Value);
                    if (vehicleEntity != null)
                    {
                        vehicleDto = new VehicleResponseDto
                        {
                            Id = vehicleEntity.Id,
                            Brand = vehicleEntity.Brand,
                            Model = vehicleEntity.Model,
                            Year = vehicleEntity.Year,
                            OdometerKm = vehicleEntity.OdometerKm,
                            ConditionGrade = vehicleEntity.ConditionGrade
                        };
                    }
                }
                if (postEntity.BatteryId.HasValue)
                {
                    var batteryEntity = await _unitOfWork.Batteries.GetByIdAsync(postEntity.BatteryId.Value);
                    if (batteryEntity != null)
                    {
                        batteryDto = new BatteryResponseDto
                        {
                            Id = batteryEntity.Id,
                            Manufacturer = batteryEntity.Manufacturer,
                            Chemistry = batteryEntity.Chemistry,
                            CapacityKwh = batteryEntity.CapacityKwh,
                            CycleCount = batteryEntity.CycleCount,
                            SohPercent = batteryEntity.SohPercent,
                            VoltageV = batteryEntity.VoltageV,
                            ConnectorType = batteryEntity.ConnectorType
                        };
                    }
                }
                return new PostResponseDto
                {
                    Id = postEntity.Id,
                    AuthorId = postEntity.AuthorId,
                    AuthorName = author.FullName,
                    PostType = postEntity.PostType,
                    Vehicle = vehicleDto,
                    Battery = batteryDto,
                    Version = postEntity.Version,
                    Title = postEntity.Title,
                    Description = postEntity.Description,
                    Price = postEntity.Price,
                    LocationAddress = postEntity.LocationAddress,
                    PhotoUrls = postEntity.PhotoUrls,
                    Status = postEntity.Status,
                    PublishedAt = postEntity.PublishedAt,
                    ExpiresAt = postEntity.ExpiresAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while retrieving post with ID: {postId}");
                throw;
            }
        }

        public async Task<PostResponseDto?> UpdatePostAsync(Guid postId, PostRequestDto updatePostDto)
        {
            try
            {
                _logger.LogInformation($"Updating post with ID: {postId}");
                if (updatePostDto == null)
                {
                    _logger.LogWarning("UpdatePostAsync failed: updatePostDto is null.");
                    throw new ArgumentNullException(nameof(updatePostDto));
                }

                var postEntity = await _unitOfWork.Posts.GetByIdAsync(postId);
                if (postEntity == null || postEntity.IsDeleted || postEntity.Status == PostStatus.Removed)
                {
                    _logger.LogWarning($"UpdatePostAsync failed: Post with ID {postId} not found.");
                    return null;
                }

                var currentUserId = _claimsService.GetCurrentUserId;
                if (postEntity.AuthorId != currentUserId)
                {
                    _logger.LogWarning($"UpdatePostAsync failed: User {currentUserId} is not authorized to update post {postId}.");
                    throw new UnauthorizedAccessException("You are not authorized to update this post.");
                }

                var author = await _unitOfWork.Users.GetByIdAsync(currentUserId);
                if (author == null)
                {
                    _logger.LogWarning($"UpdatePostAsync failed: Author with ID {currentUserId} not found.");
                    throw new InvalidOperationException("Author not found.");
                }

                Vehicle? vehicleEntity = null;
                Battery? batteryEntity = null;

                bool shouldUpdateVehicleBattery = updatePostDto.Vehicle != null || updatePostDto.Battery != null;

                if (shouldUpdateVehicleBattery)
                {
                    if (updatePostDto.PostType == PostType.Vehicle && updatePostDto.Vehicle != null)
                    {
                        if (postEntity.VehicleId.HasValue)
                        {
                            vehicleEntity = await _unitOfWork.Vehicles.GetByIdAsync(postEntity.VehicleId.Value);
                            if (vehicleEntity != null && vehicleEntity.OwnerId == currentUserId)
                            {
                                vehicleEntity.Brand = updatePostDto.Vehicle.Brand;
                                vehicleEntity.Model = updatePostDto.Vehicle.Model;
                                vehicleEntity.Year = updatePostDto.Vehicle.Year;
                                vehicleEntity.OdometerKm = updatePostDto.Vehicle.OdometerKm;
                                vehicleEntity.ConditionGrade = updatePostDto.Vehicle.ConditionGrade;
                                await _unitOfWork.Vehicles.Update(vehicleEntity);
                                _logger.LogInformation($"Vehicle entity updated with ID: {vehicleEntity.Id}");
                            }
                        }

                        if (vehicleEntity == null)
                        {
                            var checkVehicle = await _unitOfWork.Vehicles.FirstOrDefaultAsync(v =>
                                v.OwnerId == currentUserId &&
                                v.Brand == updatePostDto.Vehicle.Brand &&
                                v.Model == updatePostDto.Vehicle.Model &&
                                v.Year == updatePostDto.Vehicle.Year &&
                                v.OdometerKm == updatePostDto.Vehicle.OdometerKm &&
                                v.ConditionGrade == updatePostDto.Vehicle.ConditionGrade &&
                                !v.IsDeleted
                            );

                            if (checkVehicle != null)
                            {
                                vehicleEntity = checkVehicle;
                                _logger.LogInformation($"Existing vehicle entity found with ID: {vehicleEntity.Id}");
                            }
                            else
                            {
                                vehicleEntity = new Vehicle
                                {
                                    OwnerId = currentUserId,
                                    Brand = updatePostDto.Vehicle.Brand,
                                    Model = updatePostDto.Vehicle.Model,
                                    Year = updatePostDto.Vehicle.Year,
                                    OdometerKm = updatePostDto.Vehicle.OdometerKm,
                                    ConditionGrade = updatePostDto.Vehicle.ConditionGrade
                                };
                                await _unitOfWork.Vehicles.AddAsync(vehicleEntity);
                                _logger.LogInformation($"New vehicle entity created with ID: {vehicleEntity.Id}");

                                if (postEntity.BatteryId.HasValue)
                                {
                                    batteryEntity = await _unitOfWork.Batteries.GetByIdAsync(postEntity.BatteryId.Value);
                                    if (batteryEntity != null)
                                    {
                                        batteryEntity.IsDeleted = true;
                                        await _unitOfWork.Batteries.Update(batteryEntity);
                                        _logger.LogInformation($"Battery entity marked as deleted with ID: {batteryEntity.Id}");
                                    }
                                }
                            }
                        }

                        postEntity.PostType = PostType.Vehicle;
                        postEntity.VehicleId = vehicleEntity.Id;
                        postEntity.BatteryId = null;
                    }
                    else if (updatePostDto.PostType == PostType.Battery && updatePostDto.Battery != null)
                    {
                        if (postEntity.BatteryId.HasValue)
                        {
                            batteryEntity = await _unitOfWork.Batteries.GetByIdAsync(postEntity.BatteryId.Value);
                            if (batteryEntity != null && batteryEntity.OwnerId == currentUserId)
                            {
                                batteryEntity.Manufacturer = updatePostDto.Battery.Manufacturer;
                                batteryEntity.Chemistry = updatePostDto.Battery.Chemistry;
                                batteryEntity.CapacityKwh = updatePostDto.Battery.CapacityKwh;
                                batteryEntity.CycleCount = updatePostDto.Battery.CycleCount;
                                batteryEntity.SohPercent = updatePostDto.Battery.SohPercent;
                                batteryEntity.VoltageV = updatePostDto.Battery.VoltageV;
                                batteryEntity.ConnectorType = updatePostDto.Battery.ConnectorType;
                                await _unitOfWork.Batteries.Update(batteryEntity);
                                _logger.LogInformation($"Battery entity updated with ID: {batteryEntity.Id}");
                            }
                        }

                        if (batteryEntity == null)
                        {
                            var checkBattery = await _unitOfWork.Batteries.FirstOrDefaultAsync(b =>
                                b.OwnerId == currentUserId &&
                                b.Manufacturer == updatePostDto.Battery.Manufacturer &&
                                b.Chemistry == updatePostDto.Battery.Chemistry &&
                                b.CapacityKwh == updatePostDto.Battery.CapacityKwh &&
                                b.CycleCount == updatePostDto.Battery.CycleCount &&
                                b.SohPercent == updatePostDto.Battery.SohPercent &&
                                b.VoltageV == updatePostDto.Battery.VoltageV &&
                                b.ConnectorType == updatePostDto.Battery.ConnectorType &&
                                !b.IsDeleted
                            );

                            if (checkBattery != null)
                            {
                                batteryEntity = checkBattery;
                                _logger.LogInformation($"Existing battery entity found with ID: {batteryEntity.Id}");
                            }
                            else
                            {
                                batteryEntity = new Battery
                                {
                                    OwnerId = currentUserId,
                                    Manufacturer = updatePostDto.Battery.Manufacturer,
                                    Chemistry = updatePostDto.Battery.Chemistry,
                                    CapacityKwh = updatePostDto.Battery.CapacityKwh,
                                    CycleCount = updatePostDto.Battery.CycleCount,
                                    SohPercent = updatePostDto.Battery.SohPercent,
                                    VoltageV = updatePostDto.Battery.VoltageV,
                                    ConnectorType = updatePostDto.Battery.ConnectorType
                                };
                                await _unitOfWork.Batteries.AddAsync(batteryEntity);
                                _logger.LogInformation($"New battery entity created with ID: {batteryEntity.Id}");

                                if (postEntity.VehicleId.HasValue)
                                {
                                    vehicleEntity = await _unitOfWork.Vehicles.GetByIdAsync(postEntity.VehicleId.Value);
                                    if (vehicleEntity != null)
                                    {
                                        vehicleEntity.IsDeleted = true;
                                        await _unitOfWork.Vehicles.Update(vehicleEntity);
                                        _logger.LogInformation($"Vehicle entity marked as deleted with ID: {vehicleEntity.Id}");
                                    }
                                }
                            }
                        }

                        postEntity.PostType = PostType.Battery;
                        postEntity.BatteryId = batteryEntity.Id;
                        postEntity.VehicleId = null;
                    }
                }
                else
                {
                    if (postEntity.VehicleId.HasValue)
                    {
                        vehicleEntity = await _unitOfWork.Vehicles.GetByIdAsync(postEntity.VehicleId.Value);
                    }
                    if (postEntity.BatteryId.HasValue)
                    {
                        batteryEntity = await _unitOfWork.Batteries.GetByIdAsync(postEntity.BatteryId.Value);
                    }
                }

                if (!string.IsNullOrWhiteSpace(updatePostDto.Title))
                {
                    postEntity.Title = updatePostDto.Title;
                }

                if (!string.IsNullOrWhiteSpace(updatePostDto.Description))
                {
                    postEntity.Description = updatePostDto.Description;
                }

                if (updatePostDto.Price.HasValue)
                {
                    postEntity.Price = updatePostDto.Price;
                }

                if (!string.IsNullOrWhiteSpace(updatePostDto.LocationAddress))
                {
                    postEntity.LocationAddress = updatePostDto.LocationAddress;
                }

                if (updatePostDto.PhotoUrls != null && updatePostDto.PhotoUrls.Any())
                {
                    postEntity.PhotoUrls = updatePostDto.PhotoUrls;
                }

                postEntity.Status = updatePostDto.Status;

                // ✅ FIX: Convert PublishedAt to UTC and recalculate ExpiresAt
                if (updatePostDto.PublishedAt.HasValue && postEntity.PublishedAt <= DateTime.UtcNow)
                {
                    postEntity.PublishedAt = DateTime.SpecifyKind(updatePostDto.PublishedAt.Value, DateTimeKind.Utc);

                    if (postEntity.Version == PostVersion.Free)
                    {
                        postEntity.ExpiresAt = postEntity.PublishedAt.Value.AddDays(15);
                    }
                    else if (postEntity.Version == PostVersion.Vip)
                    {
                        postEntity.ExpiresAt = postEntity.PublishedAt.Value.AddDays(30);
                    }
                }

                await _unitOfWork.Posts.Update(postEntity);
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation($"Post updated successfully with ID: {postEntity.Id}");

                return new PostResponseDto
                {
                    Id = postEntity.Id,
                    AuthorId = postEntity.AuthorId,
                    AuthorName = author.FullName,
                    PostType = postEntity.PostType,
                    Vehicle = vehicleEntity != null ? new VehicleResponseDto
                    {
                        Id = vehicleEntity.Id,
                        Brand = vehicleEntity.Brand,
                        Model = vehicleEntity.Model,
                        Year = vehicleEntity.Year,
                        OdometerKm = vehicleEntity.OdometerKm,
                        ConditionGrade = vehicleEntity.ConditionGrade
                    } : null,
                    Battery = batteryEntity != null ? new BatteryResponseDto
                    {
                        Id = batteryEntity.Id,
                        Manufacturer = batteryEntity.Manufacturer,
                        Chemistry = batteryEntity.Chemistry,
                        CapacityKwh = batteryEntity.CapacityKwh,
                        CycleCount = batteryEntity.CycleCount,
                        SohPercent = batteryEntity.SohPercent,
                        VoltageV = batteryEntity.VoltageV,
                        ConnectorType = batteryEntity.ConnectorType
                    } : null,
                    Version = postEntity.Version,
                    Title = postEntity.Title,
                    Description = postEntity.Description,
                    Price = postEntity.Price,
                    LocationAddress = postEntity.LocationAddress,
                    PhotoUrls = postEntity.PhotoUrls,
                    Status = postEntity.Status,
                    PublishedAt = postEntity.PublishedAt,
                    ExpiresAt = postEntity.ExpiresAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while updating post with ID: {postId}");
                throw;
            }
        }

        public async Task<bool> UpdatePostStatusAsync(Guid postId, PostStatus newStatus)
        {
            try
            {
                _logger.LogInformation($"Updating status of post with ID: {postId} to {newStatus}");
                var postEntity = await _unitOfWork.Posts.GetByIdAsync(postId);
                if (postEntity == null || postEntity.IsDeleted || postEntity.Status == PostStatus.Removed)
                {
                    _logger.LogWarning($"UpdatePostStatusAsync failed: Post with ID {postId} not found.");
                    return false;
                }
                if (postEntity.Status == newStatus)
                {
                    _logger.LogInformation($"Post with ID: {postId} already has status {newStatus}. No update needed.");
                    return true;
                }
                if (postEntity.Status == PostStatus.Draft && newStatus == PostStatus.Active)
                {
                    postEntity.PublishedAt = DateTime.UtcNow;
                    if (postEntity.Version == PostVersion.Free)
                    {
                        postEntity.ExpiresAt = DateTime.UtcNow.AddDays(15);
                    }
                    else if (postEntity.Version == PostVersion.Vip)
                    {
                        postEntity.ExpiresAt = DateTime.UtcNow.AddDays(30);
                    }
                    postEntity.Status = newStatus;
                }
                else if (newStatus == PostStatus.Closed)
                {
                    postEntity.ExpiresAt = DateTime.UtcNow;
                    postEntity.Status = newStatus;
                }
                await _unitOfWork.Posts.Update(postEntity);
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation($"Post status updated successfully for post ID: {postId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while updating status of post with ID: {postId}");
                throw;
            }
        }

        public async Task<bool> DeletePostAsync(Guid postId)
        {
            try
            {
                _logger.LogInformation($"Deleting post with ID: {postId}");

                var postEntity = await _unitOfWork.Posts.GetByIdAsync(postId);

                if (postEntity == null || postEntity.IsDeleted || postEntity.Status == PostStatus.Removed)
                {
                    _logger.LogWarning($"DeletePostAsync failed: Post with ID {postId} not found or already deleted.");
                    return false;
                }

                var currentUserId = _claimsService.GetCurrentUserId;

                if (postEntity.AuthorId != currentUserId)
                {
                    _logger.LogWarning($"DeletePostAsync failed: User {currentUserId} is not authorized to delete post {postId}.");
                    throw new UnauthorizedAccessException("You are not authorized to delete this post.");
                }

                if (postEntity.VehicleId.HasValue)
                {
                    var vehicleEntity = await _unitOfWork.Vehicles.GetByIdAsync(postEntity.VehicleId.Value);
                    if (vehicleEntity != null)
                    {
                        vehicleEntity.IsDeleted = true;
                        await _unitOfWork.Vehicles.Update(vehicleEntity);
                        _logger.LogInformation($"Vehicle entity marked as deleted with ID: {vehicleEntity.Id}");
                    }
                }
                if (postEntity.BatteryId.HasValue)
                {
                    var batteryEntity = await _unitOfWork.Batteries.GetByIdAsync(postEntity.BatteryId.Value);
                    if (batteryEntity != null)
                    {
                        batteryEntity.IsDeleted = true;
                        await _unitOfWork.Batteries.Update(batteryEntity);
                        _logger.LogInformation($"Battery entity marked as deleted with ID: {batteryEntity.Id}");
                    }
                }

                postEntity.IsDeleted = true;

                await _unitOfWork.Posts.Update(postEntity);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"Post with ID {postId} successfully deleted (soft delete).");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while deleting post with ID: {postId}");
                throw;
            }
        }
        public async Task<bool> BanPostAsync(Guid postId)
        {
            try
            {
                _logger.LogInformation($"Admin attempting to ban post with ID: {postId}");

                var postEntity = await _unitOfWork.Posts.GetByIdAsync(postId);

                if (postEntity == null || postEntity.IsDeleted)
                {
                    _logger.LogWarning($"BanPostAsync failed: Post with ID {postId} not found or already deleted.");
                    return false;
                }

                // ✅ Check if current user is Admin
                var currentUserId = _claimsService.GetCurrentUserId;
                var currentUser = await _unitOfWork.Users.GetByIdAsync(currentUserId);

                if (currentUser == null || currentUser.Role != RoleType.Admin)
                {
                    _logger.LogWarning($"BanPostAsync failed: User {currentUserId} is not an admin.");
                    throw new UnauthorizedAccessException("Only admins can ban posts.");
                }

                postEntity.Status = PostStatus.Removed;

                await _unitOfWork.Posts.Update(postEntity);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"Admin {currentUserId} successfully banned post {postId} (Author: {postEntity.AuthorId}).");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while admin banning post with ID: {postId}");
                throw;
            }
        }
    }
}