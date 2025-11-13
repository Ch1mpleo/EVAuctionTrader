namespace EVAuctionTrader.BusinessObject.DTOs.RevenueDTOs;

public sealed class MonthlyRevenueDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public decimal TotalRevenue { get; set; }
    public int TotalTransactions { get; set; }
    public int VipPostCount { get; set; }
    public decimal AverageTransactionAmount { get; set; }
}
