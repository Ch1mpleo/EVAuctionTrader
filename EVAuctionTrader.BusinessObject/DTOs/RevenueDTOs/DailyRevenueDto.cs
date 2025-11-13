namespace EVAuctionTrader.BusinessObject.DTOs.RevenueDTOs;

public sealed class DailyRevenueDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int Day { get; set; }
    public DateTime Date { get; set; }
    public string DateLabel { get; set; } = string.Empty;
    public decimal TotalRevenue { get; set; }
    public int TotalTransactions { get; set; }
}
