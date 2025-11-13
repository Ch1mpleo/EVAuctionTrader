using EVAuctionTrader.Business.Interfaces;
using EVAuctionTrader.Business.Utils;
using EVAuctionTrader.BusinessObject.DTOs.AuctionDTOs;
using EVAuctionTrader.BusinessObject.DTOs.BatteryDTOs;
using EVAuctionTrader.BusinessObject.DTOs.VehicleDTOs;
using EVAuctionTrader.BusinessObject.Enums;
using EVAuctionTrader.DataAccess.Entities;
using EVAuctionTrader.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EVAuctionTrader.Business.Services;

public class AuctionService : IAuctionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClaimsService _claimsService;
    private readonly ILogger<AuctionService> _logger;
    private const int GRACE_PERIOD_MINUTES = 15;

    public AuctionService(IUnitOfWork unitOfWork, IClaimsService claimsService, ILogger<AuctionService> logger)
    {
        _unitOfWork = unitOfWork;
        _claimsService = claimsService;
        _logger = logger;
    }

    public async Task<AuctionResponseDto?> CreateAuctionAsync(AuctionRequestDto createAuctionDto)
    {
        try
        {
            _logger.LogInformation("Creating a new auction.");

            if (createAuctionDto == null)
            {
                _logger.LogWarning("CreateAuctionAsync failed: createAuctionDto is null.");
                throw new ArgumentNullException(nameof(createAuctionDto));
            }

            var currentUserId = _claimsService.GetCurrentUserId;
            var creator = await _unitOfWork.Users.GetByIdAsync(currentUserId);

            if (creator == null || creator.Role != RoleType.Admin)
            {
                _logger.LogWarning($"CreateAuctionAsync failed: User {currentUserId} is not an admin.");
                throw new UnauthorizedAccessException("Only admins can create auctions.");
            }

            // Handle Vehicle/Battery creation
            Guid? vehicleId = createAuctionDto.VehicleId;
            Guid? batteryId = createAuctionDto.BatteryId;

            if (createAuctionDto.AuctionType == AuctionType.Vehicle)
            {
                if (createAuctionDto.Vehicle != null)
                {
                    // Create new vehicle from nested DTO
                    var newVehicle = new Vehicle
                    {
                        OwnerId = currentUserId,
                        Brand = createAuctionDto.Vehicle.Brand,
                        Model = createAuctionDto.Vehicle.Model,
                        Year = createAuctionDto.Vehicle.Year,
                        OdometerKm = createAuctionDto.Vehicle.OdometerKm,
                        ConditionGrade = createAuctionDto.Vehicle.ConditionGrade
                    };
                    await _unitOfWork.Vehicles.AddAsync(newVehicle);
                    await _unitOfWork.SaveChangesAsync();
                    vehicleId = newVehicle.Id;
                    _logger.LogInformation($"Created new vehicle with ID: {vehicleId}");
                }
                else if (!vehicleId.HasValue)
                {
                    _logger.LogWarning("CreateAuctionAsync failed: VehicleId or Vehicle details required for Vehicle auction.");
                    throw new ArgumentException("VehicleId or Vehicle details required for Vehicle auction.");
                }
            }
            else if (createAuctionDto.AuctionType == AuctionType.Battery)
            {
                if (createAuctionDto.Battery != null)
                {
                    // Create new battery from nested DTO
                    var newBattery = new Battery
                    {
                        OwnerId = currentUserId,
                        Manufacturer = createAuctionDto.Battery.Manufacturer,
                        Chemistry = createAuctionDto.Battery.Chemistry,
                        CapacityKwh = createAuctionDto.Battery.CapacityKwh,
                        CycleCount = createAuctionDto.Battery.CycleCount,
                        SohPercent = createAuctionDto.Battery.SohPercent,
                        VoltageV = createAuctionDto.Battery.VoltageV,
                        ConnectorType = createAuctionDto.Battery.ConnectorType
                    };
                    await _unitOfWork.Batteries.AddAsync(newBattery);
                    await _unitOfWork.SaveChangesAsync();
                    batteryId = newBattery.Id;
                    _logger.LogInformation($"Created new battery with ID: {batteryId}");
                }
                else if (!batteryId.HasValue)
                {
                    _logger.LogWarning("CreateAuctionAsync failed: BatteryId or Battery details required for Battery auction.");
                    throw new ArgumentException("BatteryId or Battery details required for Battery auction.");
                }
            }

            if (createAuctionDto.StartTime >= createAuctionDto.EndTime)
            {
                _logger.LogWarning("CreateAuctionAsync failed: StartTime must be before EndTime.");
                throw new ArgumentException("StartTime must be before EndTime.");
            }

            if (createAuctionDto.DepositRate <= 0 || createAuctionDto.DepositRate > 1)
            {
                _logger.LogWarning("CreateAuctionAsync failed: DepositRate must be between 0 and 1.");
                throw new ArgumentException("DepositRate must be between 0 and 1.");
            }

            var auctionEntity = new Auction
            {
                CreatedBy = currentUserId,
                AuctionType = createAuctionDto.AuctionType,
                VehicleId = vehicleId,
                BatteryId = batteryId,
                Title = createAuctionDto.Title,
                Description = createAuctionDto.Description,
                StartPrice = createAuctionDto.StartPrice,
                MinIncrement = createAuctionDto.MinIncrement,
                DepositRate = createAuctionDto.DepositRate,
                CurrentPrice = createAuctionDto.StartPrice,
                StartTime = DateTime.SpecifyKind(createAuctionDto.StartTime, DateTimeKind.Utc),
                EndTime = DateTime.SpecifyKind(createAuctionDto.EndTime, DateTimeKind.Utc),
                Status = createAuctionDto.StartTime <= DateTime.UtcNow ? AuctionStatus.Running : AuctionStatus.Scheduled,
                PhotoUrl = createAuctionDto.PhotoUrl
            };

            await _unitOfWork.Auctions.AddAsync(auctionEntity);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation($"Auction created successfully with ID: {auctionEntity.Id}");

            return await MapToAuctionResponseDto(auctionEntity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while creating a new auction.");
            throw;
        }
    }

    public async Task<bool> CancelAuctionAsync(Guid auctionId)
    {
        try
        {
            _logger.LogInformation($"Canceling auction with ID: {auctionId}");

            var auction = await _unitOfWork.Auctions.GetByIdAsync(auctionId);

            if (auction == null || auction.IsDeleted)
            {
                _logger.LogWarning($"CancelAuctionAsync failed: Auction {auctionId} not found.");
                return false;
            }

            var currentUserId = _claimsService.GetCurrentUserId;
            var currentUser = await _unitOfWork.Users.GetByIdAsync(currentUserId);

            if (currentUser == null || currentUser.Role != RoleType.Admin)
            {
                _logger.LogWarning($"CancelAuctionAsync failed: User {currentUserId} is not an admin.");
                throw new UnauthorizedAccessException("Only admins can cancel auctions.");
            }

            if (auction.Status == AuctionStatus.Ended || auction.Status == AuctionStatus.Canceled)
            {
                _logger.LogWarning($"CancelAuctionAsync failed: Auction {auctionId} is already {auction.Status}.");
                return false;
            }

            auction.Status = AuctionStatus.Canceled;
            await _unitOfWork.Auctions.Update(auction);

            // Release all holds
            await ReleaseAllHoldsAsync(auctionId);

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation($"Auction {auctionId} canceled successfully.");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while canceling auction {auctionId}.");
            throw;
        }
    }

    public async Task<Pagination<AuctionResponseDto>> GetAllAuctionsAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string? search = null,
        AuctionType? auctionType = null,
        AuctionStatus? auctionStatus = null,
        bool priceSort = true)
    {
        try
        {
            _logger.LogInformation("Retrieving paginated list of auctions.");

            var query = _unitOfWork.Auctions.GetQueryable().Where(a => !a.IsDeleted);

            if (auctionType.HasValue)
            {
                query = query.Where(a => a.AuctionType == auctionType.Value);
            }

            if (auctionStatus.HasValue)
            {
                query = query.Where(a => a.Status == auctionStatus.Value);
            }

            search = search?.ToLower();
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(a => a.Title.ToLower().Contains(search) || a.Description.ToLower().Contains(search));
            }

            query = priceSort
                ? query.OrderBy(a => a.CurrentPrice)
                : query.OrderByDescending(a => a.CurrentPrice);

            var totalCount = await query.CountAsync();

            var auctions = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var auctionDtos = new List<AuctionResponseDto>();

            foreach (var auction in auctions)
            {
                var dto = await MapToAuctionResponseDto(auction);
                if (dto != null)
                {
                    auctionDtos.Add(dto);
                }
            }

            return new Pagination<AuctionResponseDto>(auctionDtos, totalCount, pageNumber, pageSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving auctions.");
            throw;
        }
    }

    public async Task<AuctionWithBidsResponseDto?> GetAuctionByIdAsync(Guid auctionId)
    {
        try
        {
            _logger.LogInformation($"Retrieving auction with ID: {auctionId}");

            var auction = await _unitOfWork.Auctions.GetByIdAsync(auctionId);

            if (auction == null || auction.IsDeleted)
            {
                _logger.LogWarning($"GetAuctionByIdAsync failed: Auction {auctionId} not found.");
                return null;
            }

            var bids = await _unitOfWork.Bids.GetAllAsync(
                predicate: b => b.AuctionId == auctionId && !b.IsDeleted,
                b => b.Bidder
            );

            var bidDtos = new List<BidResponseDto>();
            foreach (var bid in bids.OrderByDescending(b => b.CreatedAt))
            {
                bidDtos.Add(new BidResponseDto
                {
                    Id = bid.Id,
                    AuctionId = bid.AuctionId,
                    BidderId = bid.BidderId,
                    BidderName = bid.Bidder.FullName,
                    Amount = bid.Amount,
                    CreatedAt = bid.CreatedAt
                });
            }

            var currentUserId = _claimsService.GetCurrentUserId;
            decimal? userCurrentHold = null;
            var userCanBid = false;

            if (currentUserId != Guid.Empty)
            {
                var user = await _unitOfWork.Users.GetByIdAsync(currentUserId);
                if (user != null && user.Role != RoleType.Admin)
                {
                    userCurrentHold = await GetUserHoldAmountAsync(auctionId, currentUserId);

                    var wallet = await _unitOfWork.Wallets.FirstOrDefaultAsync(w => w.UserId == currentUserId && !w.IsDeleted);
                    userCanBid = auction.Status == AuctionStatus.Running &&
                                 wallet != null &&
                                 wallet.Balance >= auction.StartPrice;
                }
            }

            var baseDto = await MapToAuctionResponseDto(auction);

            if (baseDto == null)
            {
                return null;
            }

            return new AuctionWithBidsResponseDto
            {
                Id = baseDto.Id,
                CreatedBy = baseDto.CreatedBy,
                CreatorName = baseDto.CreatorName,
                AuctionType = baseDto.AuctionType,
                Vehicle = baseDto.Vehicle,
                Battery = baseDto.Battery,
                Title = baseDto.Title,
                Description = baseDto.Description,
                StartPrice = baseDto.StartPrice,
                MinIncrement = baseDto.MinIncrement,
                DepositRate = baseDto.DepositRate,
                CurrentPrice = baseDto.CurrentPrice,
                WinnerId = baseDto.WinnerId,
                WinnerName = baseDto.WinnerName,
                StartTime = baseDto.StartTime,
                EndTime = baseDto.EndTime,
                Status = baseDto.Status,
                PhotoUrl = baseDto.PhotoUrl,
                TotalBids = baseDto.TotalBids,
                Bids = bidDtos,
                UserCurrentHold = userCurrentHold,
                UserCanBid = userCanBid
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while retrieving auction {auctionId}.");
            throw;
        }
    }

    public async Task<BidResponseDto?> PlaceBidAsync(Guid auctionId, BidRequestDto bidRequest)
    {
        try
        {
            _logger.LogInformation($"User attempting to place bid on auction {auctionId}");

            if (bidRequest == null)
            {
                throw new ArgumentNullException(nameof(bidRequest));
            }

            var auction = await _unitOfWork.Auctions.GetByIdAsync(auctionId);

            if (auction == null || auction.IsDeleted)
            {
                _logger.LogWarning($"PlaceBidAsync failed: Auction {auctionId} not found.");
                throw new InvalidOperationException("Auction not found.");
            }

            if (auction.Status != AuctionStatus.Running)
            {
                _logger.LogWarning($"PlaceBidAsync failed: Auction {auctionId} is not running.");
                throw new InvalidOperationException($"Auction is {auction.Status}. Cannot place bid.");
            }

            var currentUserId = _claimsService.GetCurrentUserId;
            var bidder = await _unitOfWork.Users.GetByIdAsync(currentUserId);

            if (bidder == null || bidder.Role == RoleType.Admin)
            {
                _logger.LogWarning($"PlaceBidAsync failed: User {currentUserId} cannot bid (admin or not found).");
                throw new UnauthorizedAccessException("Admins cannot place bids.");
            }

            // Validate bid amount
            var minBidAmount = auction.CurrentPrice + auction.MinIncrement;
            if (bidRequest.Amount < minBidAmount)
            {
                _logger.LogWarning($"PlaceBidAsync failed: Bid amount {bidRequest.Amount} is less than minimum {minBidAmount}.");
                throw new InvalidOperationException($"Bid must be at least {minBidAmount:C}.");
            }

            var wallet = await _unitOfWork.Wallets.FirstOrDefaultAsync(w => w.UserId == currentUserId && !w.IsDeleted);

            if (wallet == null)
            {
                _logger.LogWarning($"PlaceBidAsync failed: Wallet not found for user {currentUserId}.");
                throw new InvalidOperationException("Wallet not found.");
            }

            // Calculate required deposit
            var requiredDeposit = Math.Max(auction.StartPrice, Math.Ceiling(bidRequest.Amount * auction.DepositRate));
            var currentHold = await GetUserHoldAmountAsync(auctionId, currentUserId);

            _logger.LogInformation($"Bid: {bidRequest.Amount}, Required deposit: {requiredDeposit}, Current hold: {currentHold}, Wallet balance: {wallet.Balance}");

            // Check if user needs to increase hold
            if (currentHold < requiredDeposit)
            {
                var additionalHoldNeeded = requiredDeposit - currentHold;
                var availableBalance = wallet.Balance - currentHold; // Balance minus existing hold

                if (availableBalance < additionalHoldNeeded)
                {
                    _logger.LogWarning($"PlaceBidAsync failed: Insufficient funds. Need {additionalHoldNeeded:C} more, have {availableBalance:C}.");
                    throw new InvalidOperationException($"Insufficient funds. You need {additionalHoldNeeded:C} more in your wallet. Please top up and try again.");
                }

                // Create hold transaction
                var holdTransaction = new WalletTransaction
                {
                    WalletId = wallet.Id,
                    Type = WalletTransactionType.AuctionHold,
                    Amount = additionalHoldNeeded,
                    BalanceAfter = wallet.Balance,
                    Status = WalletTransactionStatus.Succeeded,
                    AuctionId = auctionId
                };

                await _unitOfWork.WalletTransactions.AddAsync(holdTransaction);
                _logger.LogInformation($"Created hold of {additionalHoldNeeded:C} for user {currentUserId} on auction {auctionId}.");
            }

            // Create the bid
            var bidEntity = new Bid
            {
                AuctionId = auctionId,
                BidderId = currentUserId,
                Amount = bidRequest.Amount
            };

            await _unitOfWork.Bids.AddAsync(bidEntity);

            // Update auction current price
            auction.CurrentPrice = bidRequest.Amount;
            await _unitOfWork.Auctions.Update(auction);

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation($"Bid placed successfully: User {currentUserId}, Auction {auctionId}, Amount {bidRequest.Amount:C}");

            return new BidResponseDto
            {
                Id = bidEntity.Id,
                AuctionId = bidEntity.AuctionId,
                BidderId = bidEntity.BidderId,
                BidderName = bidder.FullName,
                Amount = bidEntity.Amount,
                CreatedAt = bidEntity.CreatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while placing bid on auction {auctionId}.");
            throw;
        }
    }

    public async Task UpdateAuctionStatusesAsync()
    {
        try
        {
            _logger.LogInformation("Updating auction statuses.");

            var utcNow = DateTime.UtcNow;

            var query = _unitOfWork.Auctions.GetQueryable().Where(a => !a.IsDeleted);

            var auctionsToUpdate = await query
                .Where(a =>
                    (a.StartTime <= utcNow && a.Status == AuctionStatus.Scheduled) ||
                    (a.EndTime <= utcNow && a.Status == AuctionStatus.Running))
                .ToListAsync();

            foreach (var auction in auctionsToUpdate)
            {
                if (auction.StartTime <= utcNow && auction.Status == AuctionStatus.Scheduled)
                {
                    auction.Status = AuctionStatus.Running;
                    _logger.LogInformation($"Auction {auction.Id} status changed to Running.");
                }

                if (auction.EndTime <= utcNow && auction.Status == AuctionStatus.Running)
                {
                    auction.Status = AuctionStatus.Ended;
                    _logger.LogInformation($"Auction {auction.Id} status changed to Ended.");
                }

                await _unitOfWork.Auctions.Update(auction);
            }

            if (auctionsToUpdate.Any())
            {
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation($"Updated status for {auctionsToUpdate.Count} auctions.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating auction statuses.");
            throw;
        }
    }

    public async Task FinalizeAuctionAsync(Guid auctionId)
    {
        try
        {
            _logger.LogInformation($"Finalizing auction {auctionId}");

            var auction = await _unitOfWork.Auctions.GetByIdAsync(auctionId);

            if (auction == null || auction.IsDeleted)
            {
                _logger.LogWarning($"FinalizeAuctionAsync failed: Auction {auctionId} not found.");
                return;
            }

            if (auction.Status != AuctionStatus.Ended)
            {
                _logger.LogWarning($"FinalizeAuctionAsync failed: Auction {auctionId} is not ended.");
                return;
            }

            if (auction.WinnerId.HasValue)
            {
                _logger.LogInformation($"Auction {auctionId} already finalized with winner {auction.WinnerId}.");
                return;
            }

            // Find the highest bid
            var highestBid = await _unitOfWork.Bids.GetQueryable()
                .Where(b => b.AuctionId == auctionId && !b.IsDeleted)
                .OrderByDescending(b => b.Amount)
                .FirstOrDefaultAsync();

            if (highestBid == null)
            {
                _logger.LogInformation($"Auction {auctionId} has no bids. No winner.");
                await ReleaseAllHoldsAsync(auctionId);
                await _unitOfWork.SaveChangesAsync();
                return;
            }

            var winnerId = highestBid.BidderId;
            var hammerPrice = highestBid.Amount;

            auction.WinnerId = winnerId;
            await _unitOfWork.Auctions.Update(auction);

            var winnerWallet = await _unitOfWork.Wallets.FirstOrDefaultAsync(w => w.UserId == winnerId && !w.IsDeleted);

            if (winnerWallet == null)
            {
                _logger.LogError($"FinalizeAuctionAsync: Winner wallet not found for user {winnerId}.");
                throw new InvalidOperationException("Winner wallet not found.");
            }

            // Calculate total hold amount for winner
            var winnerHoldAmount = await GetUserHoldAmountAsync(auctionId, winnerId);

            // Check if winner has enough balance
            if (winnerWallet.Balance < hammerPrice)
            {
                _logger.LogWarning($"Winner {winnerId} has insufficient balance ({winnerWallet.Balance:C}) for hammer price ({hammerPrice:C}). Grace period starts.");

                // TODO: Implement grace period logic (e.g., send notification, set a deadline)
                // For now, we'll just log it and continue
                // In production, you might want to schedule a check after 15 minutes

                // Optionally, you could create a "pending payment" status or record
            }

            // Capture the full hammer price from winner
            var captureTransaction = new WalletTransaction
            {
                WalletId = winnerWallet.Id,
                Type = WalletTransactionType.AuctionCapture,
                Amount = hammerPrice,
                BalanceAfter = winnerWallet.Balance - hammerPrice,
                Status = WalletTransactionStatus.Succeeded,
                AuctionId = auctionId
            };

            winnerWallet.Balance -= hammerPrice;
            await _unitOfWork.Wallets.Update(winnerWallet);
            await _unitOfWork.WalletTransactions.AddAsync(captureTransaction);

            _logger.LogInformation($"Captured {hammerPrice:C} from winner {winnerId} for auction {auctionId}.");

            // Release holds for all non-winners
            var allBidders = await _unitOfWork.Bids.GetQueryable()
                .Where(b => b.AuctionId == auctionId && !b.IsDeleted)
                .Select(b => b.BidderId)
                .Distinct()
                .ToListAsync();

            foreach (var bidderId in allBidders)
            {
                if (bidderId == winnerId)
                {
                    // Release winner's hold (since we captured the full amount)
                    await ReleaseUserHoldsAsync(auctionId, bidderId);
                }
                else
                {
                    // Release non-winner's holds
                    await ReleaseUserHoldsAsync(auctionId, bidderId);
                }
            }

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation($"Auction {auctionId} finalized. Winner: {winnerId}, Hammer price: {hammerPrice:C}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while finalizing auction {auctionId}.");
            throw;
        }
    }

    public async Task<Pagination<AuctionResponseDto>> GetMyWonAuctionsAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string? search = null)
    {
        try
        {
            _logger.LogInformation("Retrieving paginated list of won auctions for current user.");

            var currentUserId = _claimsService.GetCurrentUserId;

            if (currentUserId == Guid.Empty)
            {
                _logger.LogWarning("GetMyWonAuctionsAsync failed: Invalid user ID from claims.");
                return new Pagination<AuctionResponseDto>(new List<AuctionResponseDto>(), 0, pageNumber, pageSize);
            }

            var query = _unitOfWork.Auctions.GetQueryable()
                .Where(a => !a.IsDeleted && a.WinnerId == currentUserId && a.Status == AuctionStatus.Ended);

            search = search?.ToLower();
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(a => a.Title.ToLower().Contains(search) || a.Description.ToLower().Contains(search));
            }

            // Order by end time descending (most recent wins first)
            query = query.OrderByDescending(a => a.EndTime);

            var totalCount = await query.CountAsync();

            var auctions = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var auctionDtos = new List<AuctionResponseDto>();

            foreach (var auction in auctions)
            {
                var dto = await MapToAuctionResponseDto(auction);
                if (dto != null)
                {
                    auctionDtos.Add(dto);
                }
            }

            _logger.LogInformation($"Retrieved {auctionDtos.Count} won auctions for user {currentUserId}.");

            return new Pagination<AuctionResponseDto>(auctionDtos, totalCount, pageNumber, pageSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving won auctions.");
            throw;
        }
    }


    // Helper methods
    private async Task<AuctionResponseDto?> MapToAuctionResponseDto(Auction auction)
    {
        var creator = await _unitOfWork.Users.GetByIdAsync(auction.CreatedBy);
        if (creator == null)
        {
            _logger.LogWarning($"Creator {auction.CreatedBy} not found for auction {auction.Id}.");
            return null;
        }

        VehicleResponseDto? vehicleDto = null;
        BatteryResponseDto? batteryDto = null;

        if (auction.VehicleId.HasValue)
        {
            var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(auction.VehicleId.Value);
            if (vehicle != null)
            {
                vehicleDto = new VehicleResponseDto
                {
                    Id = vehicle.Id,
                    Brand = vehicle.Brand,
                    Model = vehicle.Model,
                    Year = vehicle.Year,
                    OdometerKm = vehicle.OdometerKm,
                    ConditionGrade = vehicle.ConditionGrade
                };
            }
        }

        if (auction.BatteryId.HasValue)
        {
            var battery = await _unitOfWork.Batteries.GetByIdAsync(auction.BatteryId.Value);
            if (battery != null)
            {
                batteryDto = new BatteryResponseDto
                {
                    Id = battery.Id,
                    Manufacturer = battery.Manufacturer,
                    Chemistry = battery.Chemistry,
                    CapacityKwh = battery.CapacityKwh,
                    CycleCount = battery.CycleCount,
                    SohPercent = battery.SohPercent,
                    VoltageV = battery.VoltageV,
                    ConnectorType = battery.ConnectorType
                };
            }
        }

        var totalBids = await _unitOfWork.Bids.GetQueryable()
            .CountAsync(b => b.AuctionId == auction.Id && !b.IsDeleted);

        User? winner = null;
        if (auction.WinnerId.HasValue)
        {
            winner = await _unitOfWork.Users.GetByIdAsync(auction.WinnerId.Value);
        }

        return new AuctionResponseDto
        {
            Id = auction.Id,
            CreatedBy = auction.CreatedBy,
            CreatorName = creator.FullName,
            AuctionType = auction.AuctionType,
            Vehicle = vehicleDto,
            Battery = batteryDto,
            Title = auction.Title,
            Description = auction.Description,
            StartPrice = auction.StartPrice,
            MinIncrement = auction.MinIncrement,
            DepositRate = auction.DepositRate,
            CurrentPrice = auction.CurrentPrice,
            WinnerId = auction.WinnerId,
            WinnerName = winner?.FullName,
            StartTime = auction.StartTime,
            EndTime = auction.EndTime,
            Status = auction.Status,
            PhotoUrl = auction.PhotoUrl,
            TotalBids = totalBids
        };
    }

    private async Task<decimal> GetUserHoldAmountAsync(Guid auctionId, Guid userId)
    {
        var wallet = await _unitOfWork.Wallets.FirstOrDefaultAsync(w => w.UserId == userId && !w.IsDeleted);

        if (wallet == null)
        {
            return 0m;
        }

        var holds = await _unitOfWork.WalletTransactions.GetAllAsync(
            predicate: t => t.WalletId == wallet.Id &&
                           t.AuctionId == auctionId &&
                           t.Type == WalletTransactionType.AuctionHold &&
                           t.Status == WalletTransactionStatus.Succeeded &&
                           !t.IsDeleted
        );

        var releases = await _unitOfWork.WalletTransactions.GetAllAsync(
            predicate: t => t.WalletId == wallet.Id &&
                           t.AuctionId == auctionId &&
                           t.Type == WalletTransactionType.AuctionRelease &&
                           t.Status == WalletTransactionStatus.Succeeded &&
                           !t.IsDeleted
        );

        var totalHold = holds.Sum(h => h.Amount);
        var totalRelease = releases.Sum(r => r.Amount);

        return totalHold - totalRelease;
    }

    private async Task ReleaseUserHoldsAsync(Guid auctionId, Guid userId)
    {
        var holdAmount = await GetUserHoldAmountAsync(auctionId, userId);

        if (holdAmount <= 0)
        {
            return;
        }

        var wallet = await _unitOfWork.Wallets.FirstOrDefaultAsync(w => w.UserId == userId && !w.IsDeleted);

        if (wallet == null)
        {
            _logger.LogWarning($"Cannot release holds: Wallet not found for user {userId}.");
            return;
        }

        var releaseTransaction = new WalletTransaction
        {
            WalletId = wallet.Id,
            Type = WalletTransactionType.AuctionRelease,
            Amount = holdAmount,
            BalanceAfter = wallet.Balance,
            Status = WalletTransactionStatus.Succeeded,
            AuctionId = auctionId
        };

        await _unitOfWork.WalletTransactions.AddAsync(releaseTransaction);

        _logger.LogInformation($"Released hold of {holdAmount:C} for user {userId} on auction {auctionId}.");
    }

    private async Task ReleaseAllHoldsAsync(Guid auctionId)
    {
        var allBidders = await _unitOfWork.Bids.GetQueryable()
            .Where(b => b.AuctionId == auctionId && !b.IsDeleted)
            .Select(b => b.BidderId)
            .Distinct()
            .ToListAsync();

        foreach (var bidderId in allBidders)
        {
            await ReleaseUserHoldsAsync(auctionId, bidderId);
        }

        _logger.LogInformation($"Released all holds for auction {auctionId}.");
    }
}
