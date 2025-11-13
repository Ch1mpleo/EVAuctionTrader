using EVAuctionTrader.Business.Interfaces;
using EVAuctionTrader.Business.Utils;
using EVAuctionTrader.BusinessObject.DTOs.RevenueDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EVAuctionTrader.Presentation.Pages.Admin;

[Authorize(Roles = "Admin")]
public sealed class RevenueModel : PageModel
{
    private readonly IRevenueService _revenueService;
    private readonly ILogger<RevenueModel> _logger;

    public RevenueModel(IRevenueService revenueService, ILogger<RevenueModel> logger)
    {
        _revenueService = revenueService;
        _logger = logger;
    }

    public RevenueSummaryDto RevenueSummary { get; set; } = new();
    public Pagination<RevenueDetailDto>? RevenueDetails { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? Year { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? Month { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public List<int> AvailableYears { get; set; } = new();

    public async Task OnGetAsync()
    {
        try
        {
            // Set default year to current year if not provided
            if (!Year.HasValue)
            {
                Year = DateTime.UtcNow.Year;
            }

            // Generate available years (last 5 years)
            var currentYear = DateTime.UtcNow.Year;
            for (int i = 0; i < 5; i++)
            {
                AvailableYears.Add(currentYear - i);
            }

            // Get revenue summary
            RevenueSummary = await _revenueService.GetRevenueSummaryAsync(Year, Month);

            // Get revenue details
            RevenueDetails = await _revenueService.GetRevenueDetailsAsync(
                pageNumber: PageNumber,
                pageSize: 10,
                year: Year,
                month: Month
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading revenue data");
            TempData["ErrorMessage"] = "Failed to load revenue data.";
            
            // Initialize empty data to prevent null reference
            RevenueSummary = new RevenueSummaryDto();
            RevenueDetails = new Pagination<RevenueDetailDto>(new List<RevenueDetailDto>(), 0, 1, 10);
        }
    }
}
