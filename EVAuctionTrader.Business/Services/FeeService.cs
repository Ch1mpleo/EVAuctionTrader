using EVAuctionTrader.Business.Interfaces;
using EVAuctionTrader.Business.Utils;
using EVAuctionTrader.BusinessObject.DTOs.FeeDTOs;
using EVAuctionTrader.BusinessObject.Enums;
using EVAuctionTrader.DataAccess.Entities;
using EVAuctionTrader.DataAccess.Interfaces;
using Microsoft.Extensions.Logging;

namespace EVAuctionTrader.Business.Services;

public sealed class FeeService : IFeeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClaimsService _claimsService;
    private readonly ILogger<FeeService> _logger;

    public FeeService(
        IUnitOfWork unitOfWork,
        IClaimsService claimsService,
        ILogger<FeeService> logger)
    {
        _unitOfWork = unitOfWork;
        _claimsService = claimsService;
        _logger = logger;
    }

    public async Task<FeeResponseDto> CreateFeeAsync(FeeRequestDto feeRequestDto)
    {
        try
        {
            // Validate admin access
            await ValidateAdminAccessAsync();

            _logger.LogInformation("Admin creating new fee of type: {FeeType}", feeRequestDto.Type);

            if (feeRequestDto.Amount <= 0)
            {
                throw new ArgumentException("Fee amount must be greater than zero.", nameof(feeRequestDto.Amount));
            }

            // Check if fee type already exists
            var existingFee = await _unitOfWork.Fees.FirstOrDefaultAsync(f => f.Type == feeRequestDto.Type && !f.IsDeleted);
            if (existingFee != null)
            {
                throw new InvalidOperationException($"A fee of type {feeRequestDto.Type} already exists. Please update the existing fee instead.");
            }

            var fee = new Fee
            {
                Type = feeRequestDto.Type,
                Amount = feeRequestDto.Amount,
                Description = feeRequestDto.Description
            };

            await _unitOfWork.Fees.AddAsync(fee);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Fee created successfully: Type={FeeType}, Amount={Amount}, ID={FeeId}", 
                fee.Type, fee.Amount, fee.Id);

            return MapToFeeResponseDto(fee);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating fee");
            throw;
        }
    }

    public async Task<FeeResponseDto?> GetFeeByIdAsync(Guid id)
    {
        try
        {
            // Validate admin access
            await ValidateAdminAccessAsync();

            _logger.LogInformation("Retrieving fee with ID: {FeeId}", id);

            var fee = await _unitOfWork.Fees.GetByIdAsync(id);

            if (fee == null || fee.IsDeleted)
            {
                _logger.LogWarning("Fee not found with ID: {FeeId}", id);
                return null;
            }

            return MapToFeeResponseDto(fee);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving fee by ID: {FeeId}", id);
            throw;
        }
    }

    public async Task<FeeResponseDto?> GetFeeByTypeAsync(FeeType type)
    {
        try
        {
            _logger.LogInformation("Retrieving fee of type: {FeeType}", type);

            var fee = await _unitOfWork.Fees.FirstOrDefaultAsync(f => f.Type == type && !f.IsDeleted);

            if (fee == null)
            {
                _logger.LogWarning("Fee not found for type: {FeeType}", type);
                return null;
            }

            return MapToFeeResponseDto(fee);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving fee by type: {FeeType}", type);
            throw;
        }
    }

    public async Task<Pagination<FeeResponseDto>> GetAllFeesAsync(int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            // Validate admin access
            await ValidateAdminAccessAsync();

            _logger.LogInformation("Admin retrieving all fees (Page: {PageNumber}, Size: {PageSize})", 
                pageNumber, pageSize);

            var allFees = await _unitOfWork.Fees.GetAllAsync(f => !f.IsDeleted);

            var totalCount = allFees.Count;

            var paginatedFees = allFees
                .OrderBy(f => f.Type)
                .ThenByDescending(f => f.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var feeDtos = paginatedFees.Select(MapToFeeResponseDto).ToList();

            return new Pagination<FeeResponseDto>(
                feeDtos,
                totalCount,
                pageNumber,
                pageSize
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all fees");
            throw;
        }
    }

    public async Task<FeeResponseDto?> UpdateFeeAsync(Guid id, FeeRequestDto feeRequestDto)
    {
        try
        {
            // Validate admin access
            await ValidateAdminAccessAsync();

            _logger.LogInformation("Admin updating fee with ID: {FeeId}", id);

            if (feeRequestDto.Amount <= 0)
            {
                throw new ArgumentException("Fee amount must be greater than zero.", nameof(feeRequestDto.Amount));
            }

            var fee = await _unitOfWork.Fees.GetByIdAsync(id);

            if (fee == null || fee.IsDeleted)
            {
                _logger.LogWarning("Fee not found with ID: {FeeId}", id);
                return null;
            }

            // Check if changing type would conflict with existing fee
            if (fee.Type != feeRequestDto.Type)
            {
                var existingFee = await _unitOfWork.Fees.FirstOrDefaultAsync(
                    f => f.Type == feeRequestDto.Type && f.Id != id && !f.IsDeleted);
                
                if (existingFee != null)
                {
                    throw new InvalidOperationException(
                        $"Cannot change fee type to {feeRequestDto.Type} because a fee of that type already exists.");
                }
            }

            var oldAmount = fee.Amount;
            var oldType = fee.Type;

            fee.Type = feeRequestDto.Type;
            fee.Amount = feeRequestDto.Amount;
            fee.Description = feeRequestDto.Description;

            await _unitOfWork.Fees.Update(fee);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Fee updated successfully: ID={FeeId}, Type={OldType}->{NewType}, Amount={OldAmount}->{NewAmount}",
                fee.Id, oldType, fee.Type, oldAmount, fee.Amount);

            return MapToFeeResponseDto(fee);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating fee with ID: {FeeId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteFeeAsync(Guid id)
    {
        try
        {
            // Validate admin access
            await ValidateAdminAccessAsync();

            _logger.LogInformation("Admin attempting to delete fee with ID: {FeeId}", id);

            var fee = await _unitOfWork.Fees.GetByIdAsync(id);

            if (fee == null || fee.IsDeleted)
            {
                _logger.LogWarning("Fee not found or already deleted with ID: {FeeId}", id);
                return false;
            }

            fee.IsDeleted = true;
            await _unitOfWork.Fees.Update(fee);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Fee soft deleted successfully: ID={FeeId}, Type={FeeType}", 
                fee.Id, fee.Type);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting fee with ID: {FeeId}", id);
            throw;
        }
    }

    // Helper methods
    private async Task ValidateAdminAccessAsync()
    {
        var userId = _claimsService.GetCurrentUserId;
        var user = await _unitOfWork.Users.GetByIdAsync(userId);

        if (user == null)
        {
            _logger.LogWarning("User not found with ID: {UserId}", userId);
            throw new UnauthorizedAccessException("User is not authenticated.");
        }

        if (user.Role != RoleType.Admin)
        {
            _logger.LogWarning("Non-admin user {UserId} attempted to access fee management", userId);
            throw new UnauthorizedAccessException("Only administrators can manage fees.");
        }
    }

    private static FeeResponseDto MapToFeeResponseDto(Fee fee)
    {
        return new FeeResponseDto
        {
            Id = fee.Id,
            Type = fee.Type,
            Amount = fee.Amount,
            Description = fee.Description,
            CreatedAt = fee.CreatedAt,
            UpdatedAt = fee.UpdatedAt
        };
    }
}
