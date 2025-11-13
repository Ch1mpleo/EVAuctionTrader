using EVAuctionTrader.Business.Interfaces;
using EVAuctionTrader.Business.Utils;
using EVAuctionTrader.BusinessObject.DTOs.RevenueDTOs;
using EVAuctionTrader.BusinessObject.Enums;
using EVAuctionTrader.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace EVAuctionTrader.Business.Services;

public sealed class RevenueService : IRevenueService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClaimsService _claimsService;
    private readonly ILogger<RevenueService> _logger;

    public RevenueService(
        IUnitOfWork unitOfWork,
        IClaimsService claimsService,
        ILogger<RevenueService> logger)
    {
        _unitOfWork = unitOfWork;
        _claimsService = claimsService;
        _logger = logger;
    }

    public async Task<RevenueSummaryDto> GetRevenueSummaryAsync(int? year = null, int? month = null)
    {
        try
        {
            _logger.LogInformation("Getting revenue summary for year: {Year}, month: {Month}", year, month);

            // Verify admin access
            await VerifyAdminAccessAsync();

            var currentDate = DateTime.UtcNow;
            var targetYear = year ?? currentDate.Year;

            // Get all post fee transactions
            var allTransactions = await _unitOfWork.WalletTransactions.GetAllAsync(
                predicate: t => t.Type == WalletTransactionType.PostFee &&
                               t.Status == WalletTransactionStatus.Succeeded &&
                               !t.IsDeleted,
                includes: t => t.Wallet
            );

            // Filter by year and optionally month
            var filteredTransactions = allTransactions
                .Where(t => t.CreatedAt.Year == targetYear);

            if (month.HasValue)
            {
                filteredTransactions = filteredTransactions.Where(t => t.CreatedAt.Month == month.Value);
            }

            var transactionsList = filteredTransactions.ToList();

            // Calculate current month revenue
            var currentMonthTransactions = allTransactions
                .Where(t => t.CreatedAt.Year == currentDate.Year && t.CreatedAt.Month == currentDate.Month)
                .ToList();

            // Calculate last month revenue
            var lastMonth = currentDate.AddMonths(-1);
            var lastMonthTransactions = allTransactions
                .Where(t => t.CreatedAt.Year == lastMonth.Year && t.CreatedAt.Month == lastMonth.Month)
                .ToList();

            var currentMonthRevenue = currentMonthTransactions.Sum(t => t.Amount);
            var lastMonthRevenue = lastMonthTransactions.Sum(t => t.Amount);

            // Calculate growth percentage
            var growthPercentage = lastMonthRevenue > 0
                ? ((currentMonthRevenue - lastMonthRevenue) / lastMonthRevenue) * 100
                : currentMonthRevenue > 0 ? 100 : 0;

            // Get monthly or daily revenues based on filter
            var monthlyRevenues = new List<MonthlyRevenueDto>();
            var dailyRevenues = new List<DailyRevenueDto>();

            if (month.HasValue)
            {
                // Get daily revenues for the specific month
                dailyRevenues = await GetDailyRevenuesAsync(targetYear, month.Value);
            }
            else
            {
                // Get monthly revenues for the year
                monthlyRevenues = await GetMonthlyRevenuesAsync(targetYear);
            }

            return new RevenueSummaryDto
            {
                TotalRevenue = transactionsList.Sum(t => t.Amount),
                CurrentMonthRevenue = currentMonthRevenue,
                LastMonthRevenue = lastMonthRevenue,
                GrowthPercentage = Math.Round(growthPercentage, 2),
                TotalTransactions = transactionsList.Count,
                TotalVipPosts = transactionsList.Count, // All post fee transactions are VIP posts
                MonthlyRevenues = monthlyRevenues,
                DailyRevenues = dailyRevenues
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting revenue summary");
            throw;
        }
    }

    public async Task<Pagination<RevenueDetailDto>> GetRevenueDetailsAsync(
        int pageNumber = 1,
        int pageSize = 10,
        int? year = null,
        int? month = null)
    {
        try
        {
            _logger.LogInformation("Getting revenue details for page: {Page}, year: {Year}, month: {Month}",
                pageNumber, year, month);

            // Verify admin access
            await VerifyAdminAccessAsync();

            // Get all post fee transactions with related data
            var query = _unitOfWork.WalletTransactions.GetQueryable()
                .Where(t => t.Type == WalletTransactionType.PostFee &&
                           t.Status == WalletTransactionStatus.Succeeded &&
                           !t.IsDeleted &&
                           t.PostId != null);

            // Filter by year and month if provided
            if (year.HasValue)
            {
                query = query.Where(t => t.CreatedAt.Year == year.Value);
            }

            if (month.HasValue)
            {
                query = query.Where(t => t.CreatedAt.Month == month.Value);
            }

            // Order by most recent first
            query = query.OrderByDescending(t => t.CreatedAt);

            var totalCount = await query.CountAsync();

            var transactions = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var revenueDtos = new List<RevenueDetailDto>();

            foreach (var transaction in transactions)
            {
                // Get wallet to find user
                var wallet = await _unitOfWork.Wallets.GetByIdAsync(transaction.WalletId);
                if (wallet == null) continue;

                var user = await _unitOfWork.Users.GetByIdAsync(wallet.UserId);
                if (user == null) continue;

                // Get post details
                var post = transaction.PostId.HasValue
                    ? await _unitOfWork.Posts.GetByIdAsync(transaction.PostId.Value)
                    : null;

                revenueDtos.Add(new RevenueDetailDto
                {
                    TransactionId = transaction.Id,
                    PostId = transaction.PostId ?? Guid.Empty,
                    PostTitle = post?.Title ?? "Unknown Post",
                    UserId = user.Id,
                    UserName = user.FullName,
                    Amount = transaction.Amount,
                    CreatedAt = transaction.CreatedAt
                });
            }

            return new Pagination<RevenueDetailDto>(revenueDtos, totalCount, pageNumber, pageSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting revenue details");
            throw;
        }
    }

    public async Task<List<MonthlyRevenueDto>> GetMonthlyRevenuesAsync(int year)
    {
        try
        {
            _logger.LogInformation("Getting monthly revenues for year: {Year}", year);

            // Verify admin access
            await VerifyAdminAccessAsync();

            // Get all post fee transactions for the year
            var transactions = await _unitOfWork.WalletTransactions.GetAllAsync(
                predicate: t => t.Type == WalletTransactionType.PostFee &&
                               t.Status == WalletTransactionStatus.Succeeded &&
                               !t.IsDeleted &&
                               t.CreatedAt.Year == year
            );

            // Group by month
            var monthlyData = transactions
                .GroupBy(t => t.CreatedAt.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    Revenue = g.Sum(t => t.Amount),
                    Count = g.Count()
                })
                .ToList();

            // Create result for all 12 months
            var result = new List<MonthlyRevenueDto>();
            for (int month = 1; month <= 12; month++)
            {
                var data = monthlyData.FirstOrDefault(m => m.Month == month);
                var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month);

                result.Add(new MonthlyRevenueDto
                {
                    Year = year,
                    Month = month,
                    MonthName = monthName,
                    TotalRevenue = data?.Revenue ?? 0,
                    TotalTransactions = data?.Count ?? 0,
                    VipPostCount = data?.Count ?? 0, // All post fee transactions are VIP posts
                    AverageTransactionAmount = data != null && data.Count > 0
                        ? Math.Round(data.Revenue / data.Count, 2)
                        : 0
                });
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting monthly revenues for year {Year}", year);
            throw;
        }
    }

    public async Task<List<DailyRevenueDto>> GetDailyRevenuesAsync(int year, int month)
    {
        try
        {
            _logger.LogInformation("Getting daily revenues for year: {Year}, month: {Month}", year, month);

            // Verify admin access
            await VerifyAdminAccessAsync();

            // Get all post fee transactions for the specific month
            var transactions = await _unitOfWork.WalletTransactions.GetAllAsync(
                predicate: t => t.Type == WalletTransactionType.PostFee &&
                               t.Status == WalletTransactionStatus.Succeeded &&
                               !t.IsDeleted &&
                               t.CreatedAt.Year == year &&
                               t.CreatedAt.Month == month
            );

            // Group by day
            var dailyData = transactions
                .GroupBy(t => t.CreatedAt.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Revenue = g.Sum(t => t.Amount),
                    Count = g.Count()
                })
                .OrderBy(d => d.Date)
                .ToList();

            // Get the number of days in the month
            var daysInMonth = DateTime.DaysInMonth(year, month);

            // Create result for all days in the month
            var result = new List<DailyRevenueDto>();
            for (int day = 1; day <= daysInMonth; day++)
            {
                var date = new DateTime(year, month, day);
                var data = dailyData.FirstOrDefault(d => d.Date == date);

                result.Add(new DailyRevenueDto
                {
                    Year = year,
                    Month = month,
                    Day = day,
                    Date = date,
                    DateLabel = date.ToString("MMM dd"),
                    TotalRevenue = data?.Revenue ?? 0,
                    TotalTransactions = data?.Count ?? 0
                });
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting daily revenues for year {Year}, month {Month}", year, month);
            throw;
        }
    }

    private async Task VerifyAdminAccessAsync()
    {
        var currentUserId = _claimsService.GetCurrentUserId;
        var currentUser = await _unitOfWork.Users.GetByIdAsync(currentUserId);

        if (currentUser == null || currentUser.Role != RoleType.Admin)
        {
            _logger.LogWarning("Non-admin user {UserId} attempted to access revenue data", currentUserId);
            throw new UnauthorizedAccessException("Only administrators can access revenue data.");
        }
    }
}
