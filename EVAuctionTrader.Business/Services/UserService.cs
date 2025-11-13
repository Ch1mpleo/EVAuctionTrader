using EVAuctionTrader.Business.Interfaces;
using EVAuctionTrader.Business.Utils;
using EVAuctionTrader.BusinessObject.DTOs.UserDTOs;
using EVAuctionTrader.BusinessObject.Enums;
using EVAuctionTrader.DataAccess.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVAuctionTrader.Business.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClaimsService _claimsService;
        private readonly ILogger _logger;

        public UserService(IUnitOfWork unitOfWork, IClaimsService claimsService, ILogger<UserService> logger)
        {
            _unitOfWork = unitOfWork;
            _claimsService = claimsService;
            _logger = logger;
        }

        public async Task<decimal> GetMyBalanceAsync()
        {
            try
            {
                var userId = _claimsService.GetCurrentUserId;

                if (userId == Guid.Empty)
                {
                    _logger.LogWarning("GetMyBalanceAsync failed: Invalid user ID from claims.");
                    return 0m;
                }

                var wallet = await _unitOfWork.Wallets.FirstOrDefaultAsync(w => w.UserId == userId && !w.IsDeleted);
                _logger.LogInformation($"GetMyBalanceAsync: User {userId} has balance {wallet?.Balance ?? 0m}");

                return wallet?.Balance ?? 0m;
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetMyBalanceAsync error: {ex.Message}");
                throw;
            }
        }

        public async Task<decimal> GetMyHeldBalanceAsync()
        {
            try
            {
                var userId = _claimsService.GetCurrentUserId;

                if (userId == Guid.Empty)
                {
                    _logger.LogWarning("GetMyHeldBalanceAsync failed: Invalid user ID from claims.");
                    return 0m;
                }

                var wallet = await _unitOfWork.Wallets.FirstOrDefaultAsync(w => w.UserId == userId && !w.IsDeleted);

                if (wallet == null)
                {
                    _logger.LogWarning($"GetMyHeldBalanceAsync: Wallet not found for user {userId}");
                    return 0m;
                }

                // Get all auction hold transactions
                var holds = await _unitOfWork.WalletTransactions.GetAllAsync(
                    predicate: t => t.WalletId == wallet.Id &&
                                   t.Type == WalletTransactionType.AuctionHold &&
                                   t.Status == WalletTransactionStatus.Succeeded &&
                                   !t.IsDeleted
                );

                // Get all auction release transactions
                var releases = await _unitOfWork.WalletTransactions.GetAllAsync(
                    predicate: t => t.WalletId == wallet.Id &&
                                   t.Type == WalletTransactionType.AuctionRelease &&
                                   t.Status == WalletTransactionStatus.Succeeded &&
                                   !t.IsDeleted
                );

                var totalHeld = holds.Sum(h => h.Amount);
                var totalReleased = releases.Sum(r => r.Amount);
                var netHeld = totalHeld - totalReleased;

                _logger.LogInformation($"GetMyHeldBalanceAsync: User {userId} has {netHeld} held in auctions");

                return netHeld > 0 ? netHeld : 0m;
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetMyHeldBalanceAsync error: {ex.Message}");
                throw;
            }
        }

        public async Task<UserResponseDto?> GetMyProfileAsync()
        {
            try
            {
                var userId = _claimsService.GetCurrentUserId;

                if (userId == Guid.Empty)
                {
                    _logger.LogWarning("GetMyProfileAsync failed: Invalid user ID from claims.");
                    return null;
                }

                var user = await _unitOfWork.Users.GetByIdAsync(userId);

                if (user == null || user.IsDeleted)
                {
                    _logger.LogWarning($"GetMyProfileAsync failed: User with ID {userId} not found or inactive.");
                    return null;
                }

                return new UserResponseDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    Phone = user.Phone,
                    FullName = user.FullName,
                    Role = user.Role,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt,
                    IsDeleted = user.IsDeleted
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetMyProfileAsync error: {ex.Message}");
                throw;
            }
        }

        public async Task<WalletResponseDto?> GetMyWalletAsync()
        {
            try
            {
                var userId = _claimsService.GetCurrentUserId;

                if (userId == Guid.Empty)
                {
                    _logger.LogWarning("GetMyWalletAsync failed: Invalid user ID from claims.");
                    return null;
                }

                var wallet = await _unitOfWork.Wallets.FirstOrDefaultAsync(w => w.UserId == userId && !w.IsDeleted);

                if (wallet == null)
                {
                    _logger.LogWarning($"GetMyWalletAsync failed: Wallet for user ID {userId} not found.");
                    return null;
                }

                return new WalletResponseDto
                {
                    Id = wallet.Id,
                    UserId = wallet.UserId,
                    Balance = wallet.Balance
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetMyWalletAsync error: {ex.Message}");
                throw;
            }
        }

        public async Task<UserResponseDto?> UpdateMyProfileAsync(UpdateProfileRequestDto updateProfileRequestDto)
        {
            try
            {
                var userId = _claimsService.GetCurrentUserId;

                if (userId == Guid.Empty)
                {
                    _logger.LogWarning("UpdateMyProfileAsync failed: Invalid user ID from claims.");
                    return null;
                }

                var user = await _unitOfWork.Users.GetByIdAsync(userId);

                if (user == null || user.IsDeleted)
                {
                    _logger.LogWarning($"UpdateMyProfileAsync failed: User with ID {userId} not found or inactive.");
                    return null;
                }

                if (!string.IsNullOrEmpty(updateProfileRequestDto.CurrentPassword) &&
                    !string.IsNullOrEmpty(updateProfileRequestDto.NewPassword))
                {
                    var passwordHasher = new PasswordHasher();

                    if (!passwordHasher.VerifyPassword(updateProfileRequestDto.CurrentPassword, user.PasswordHash))
                    {
                        throw new UnauthorizedAccessException("Current password is incorrect.");
                    }

                    user.PasswordHash = passwordHasher.HashPassword(updateProfileRequestDto.NewPassword);
                }

                user.FullName = updateProfileRequestDto.FullName;
                user.Phone = updateProfileRequestDto.Phone;

                await _unitOfWork.Users.Update(user);
                await _unitOfWork.SaveChangesAsync();

                return new UserResponseDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    Phone = user.Phone,
                    FullName = user.FullName,
                    Role = user.Role,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt,
                    IsDeleted = user.IsDeleted
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"UpdateMyProfileAsync error: {ex.Message}");
                throw;
            }
        }
    }
}
