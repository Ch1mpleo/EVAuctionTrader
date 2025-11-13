namespace EVAuctionTrader.BusinessObject.DTOs.RevenueDTOs;

public sealed class RevenueSummaryDto
{
    public decimal TotalRevenue { get; set; }
    public decimal CurrentMonthRevenue { get; set; }
    public decimal LastMonthRevenue { get; set; }
    public decimal GrowthPercentage { get; set; }
    public int TotalTransactions { get; set; }
    public int TotalVipPosts { get; set; }
    public List<MonthlyRevenueDto> MonthlyRevenues { get; set; } = new();
    public List<DailyRevenueDto> DailyRevenues { get; set; } = new();
}
