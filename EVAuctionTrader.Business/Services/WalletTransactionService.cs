using EVAuctionTrader.Business.Interfaces;
using EVAuctionTrader.Business.Utils;
using EVAuctionTrader.BusinessObject.DTOs.WalletTransactionDTOs;
using EVAuctionTrader.BusinessObject.Enums;
using EVAuctionTrader.DataAccess.Entities;
using EVAuctionTrader.DataAccess.Interfaces;
using Microsoft.Extensions.Logging;

namespace EVAuctionTrader.Business.Services;

public class WalletTransactionService : IWalletTransactionService
{
    private readonly ILogger _logger;
    private readonly IClaimsService _claimsService;
    private readonly IUnitOfWork _unitOfWork;

    public WalletTransactionService(ILogger<WalletTransactionService> logger, IClaimsService claimsService, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _claimsService = claimsService;
        _unitOfWork = unitOfWork;
    }

    public async Task<WalletTransactionResponseDto> CreateWalletTransactionAsync(WalletTransactionRequestDto walletTransactionRequestDto)
    {
        try
        {
            var userId = _claimsService.GetCurrentUserId;
            var user = await _unitOfWork.Users.GetByIdAsync(userId, x => x.Wallets);
            if (user == null)
            {
                _logger.LogWarning("User not found with ID: {UserId}", userId);
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            var wallet = user.Wallets?.FirstOrDefault();
            if (wallet == null)
            {
                _logger.LogWarning("Wallet not found for user ID: {UserId}", userId);
                throw new InvalidOperationException("User does not have a wallet.");
            }

            var walletTransaction = new WalletTransaction
            {
                WalletId = wallet.Id,
                Type = walletTransactionRequestDto.Type,
                Amount = walletTransactionRequestDto.Amount,
                BalanceAfter = walletTransactionRequestDto.BalanceAfter,
                Status = walletTransactionRequestDto.Status,
                PostId = walletTransactionRequestDto.PostId,
                AuctionId = walletTransactionRequestDto.AuctionId,
                PaymentId = walletTransactionRequestDto.PaymentId
            };

            await _unitOfWork.WalletTransactions.AddAsync(walletTransaction);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Wallet transaction created with ID: {TransactionId}", walletTransaction.Id);

            return await MapToWalletTransactionResponseDto(walletTransaction, userId, user.FullName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating wallet transaction");
            throw;
        }
    }

    public async Task<WalletTransactionResponseDto?> GetWalletTransactionByIdAsync(Guid id)
    {
        try
        {
            var userId = _claimsService.GetCurrentUserId;
            var user = await _unitOfWork.Users.GetByIdAsync(userId, x => x.Wallets);

            if (user == null)
            {
                _logger.LogWarning("User not found with ID: {UserId}", userId);
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            var walletTransaction = await _unitOfWork.WalletTransactions.GetByIdAsync(id);

            if (walletTransaction == null || walletTransaction.IsDeleted)
            {
                _logger.LogWarning("Wallet transaction not found with ID: {TransactionId}", id);
                return null;
            }

            // Get the wallet to check ownership
            var wallet = await _unitOfWork.Wallets.GetByIdAsync(walletTransaction.WalletId);

            if (wallet == null)
            {
                _logger.LogWarning("Wallet not found for transaction ID: {TransactionId}", id);
                return null;
            }

            // Non-admin users can only view their own transactions
            if (user.Role != RoleType.Admin && wallet.UserId != userId)
            {
                _logger.LogWarning("User {UserId} attempted to view transaction {TransactionId} that doesn't belong to them", userId, id);
                throw new UnauthorizedAccessException("You can only view your own wallet transactions.");
            }

            // Get the owner's information
            var owner = await _unitOfWork.Users.GetByIdAsync(wallet.UserId);
            if (owner == null)
            {
                _logger.LogWarning("Owner user not found for wallet ID: {WalletId}", wallet.Id);
                throw new InvalidOperationException("Wallet owner not found.");
            }

            return await MapToWalletTransactionResponseDto(walletTransaction, owner.Id, owner.FullName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving wallet transaction");
            throw;
        }
    }

    public async Task<Pagination<WalletTransactionResponseDto>> GetWalletTransactionsAsync(int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var userId = _claimsService.GetCurrentUserId;
            var user = await _unitOfWork.Users.GetByIdAsync(userId, x => x.Wallets);
            if (user == null)
            {
                _logger.LogWarning("User not found with ID: {UserId}", userId);
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            if (user.Role == RoleType.Admin)
            {
                _logger.LogInformation("Admin user {UserId} retrieving all wallet transactions", userId);

                // Get all transactions for admin with Wallet navigation property
                var allTransactions = await _unitOfWork.WalletTransactions.GetAllAsync(null, t => t.Wallet);

                var totalCountAdmin = allTransactions.Count;

                var paginatedAdminTransactions = allTransactions
                    .OrderByDescending(wt => wt.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var adminTransactionDtos = new List<WalletTransactionResponseDto>();
                foreach (var wt in paginatedAdminTransactions)
                {
                    var owner = await _unitOfWork.Users.GetByIdAsync(wt.Wallet.UserId);
                    if (owner != null)
                    {
                        var dto = await MapToWalletTransactionResponseDto(wt, owner.Id, owner.FullName);
                        adminTransactionDtos.Add(dto);
                    }
                }

                return new Pagination<WalletTransactionResponseDto>
                (
                    adminTransactionDtos,
                    totalCountAdmin,
                    pageNumber,
                    pageSize
                );
            }

            // Get user's wallet
            var wallet = user.Wallets?.FirstOrDefault();
            if (wallet == null)
            {
                _logger.LogWarning("Wallet not found for user ID: {UserId}", userId);
                throw new InvalidOperationException("User does not have a wallet.");
            }

            _logger.LogInformation("User {UserId} retrieving their wallet transactions", userId);

            // Get transactions for the specific wallet
            var walletTransactions = await _unitOfWork.WalletTransactions.GetAllAsync(
                wt => wt.WalletId == wallet.Id);

            var totalCount = walletTransactions.Count;

            var paginatedTransactions = walletTransactions
                .OrderByDescending(wt => wt.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var walletTransactionDtos = new List<WalletTransactionResponseDto>();
            foreach (var wt in paginatedTransactions)
            {
                var dto = await MapToWalletTransactionResponseDto(wt, userId, user.FullName);
                walletTransactionDtos.Add(dto);
            }

            return new Pagination<WalletTransactionResponseDto>
            (
                walletTransactionDtos,
                totalCount,
                pageNumber,
                pageSize
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving wallet transactions");
            throw;
        }
    }

    public async Task<bool> DeleteWalletTransactionAsync(Guid id)
    {
        try
        {
            var userId = _claimsService.GetCurrentUserId;
            var user = await _unitOfWork.Users.GetByIdAsync(userId);

            if (user == null)
            {
                _logger.LogWarning("User not found with ID: {UserId}", userId);
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            // Only admins can delete wallet transactions
            if (user.Role != RoleType.Admin)
            {
                _logger.LogWarning("Non-admin user {UserId} attempted to delete wallet transaction {TransactionId}", userId, id);
                throw new UnauthorizedAccessException("Only administrators can delete wallet transactions.");
            }

            var walletTransaction = await _unitOfWork.WalletTransactions.GetByIdAsync(id);

            if (walletTransaction == null || walletTransaction.IsDeleted)
            {
                _logger.LogWarning("Wallet transaction not found or already deleted with ID: {TransactionId}", id);
                return false;
            }

            walletTransaction.IsDeleted = true;
            await _unitOfWork.WalletTransactions.Update(walletTransaction);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Admin {UserId} deleted wallet transaction {TransactionId}", userId, id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting wallet transaction {TransactionId}", id);
            throw;
        }
    }

    // Helper method to map entity to DTO with related data
    private async Task<WalletTransactionResponseDto> MapToWalletTransactionResponseDto(
        WalletTransaction transaction,
        Guid userId,
        string userName)
    {
        var dto = new WalletTransactionResponseDto
        {
            Id = transaction.Id,
            WalletId = transaction.WalletId,
            UserId = userId,
            UserName = userName,
            Type = transaction.Type,
            Amount = transaction.Amount,
            BalanceAfter = transaction.BalanceAfter,
            Status = transaction.Status,
            PostId = transaction.PostId,
            AuctionId = transaction.AuctionId,
            PaymentId = transaction.PaymentId,
            CreatedAt = transaction.CreatedAt
        };

        // Load related Post information if exists
        if (transaction.PostId.HasValue)
        {
            var post = await _unitOfWork.Posts.GetByIdAsync(transaction.PostId.Value);
            if (post != null && !post.IsDeleted)
            {
                dto.PostTitle = post.Title;
            }
        }

        // Load related Auction information if exists
        if (transaction.AuctionId.HasValue)
        {
            var auction = await _unitOfWork.Auctions.GetByIdAsync(transaction.AuctionId.Value);
            if (auction != null && !auction.IsDeleted)
            {
                dto.AuctionTitle = auction.Title;
            }
        }

        // Load related Payment information if exists
        if (transaction.PaymentId.HasValue)
        {
            var payment = await _unitOfWork.Payments.GetByIdAsync(transaction.PaymentId.Value);
            if (payment != null && !payment.IsDeleted)
            {
                dto.PaymentAmount = payment.Amount;
            }
        }

        return dto;
    }
}
