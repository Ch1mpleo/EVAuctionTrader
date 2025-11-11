using EVAuctionTrader.Business.Interfaces;
using EVAuctionTrader.Business.Utils;
using EVAuctionTrader.BusinessObject.DTOs.AuctionDTOs;
using EVAuctionTrader.BusinessObject.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EVAuctionTrader.Presentation.Pages.AuctionPages;

public sealed class IndexModel : PageModel
{
    private readonly IAuctionService _auctionService;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(IAuctionService auctionService, ILogger<IndexModel> logger)
    {
        _auctionService = auctionService;
        _logger = logger;
    }

    public Pagination<AuctionResponseDto>? Auctions { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public AuctionType? AuctionType { get; set; }

    [BindProperty(SupportsGet = true)]
    public AuctionStatus? AuctionStatus { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool PriceSort { get; set; } = true;

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            Auctions = await _auctionService.GetAllAuctionsAsync(
                pageNumber: PageNumber,
                pageSize: 12,
                search: Search,
                auctionType: AuctionType,
                auctionStatus: AuctionStatus,
                priceSort: PriceSort
            );

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving auctions");
            TempData["ErrorMessage"] = "Failed to load auctions. Please try again.";
            return Page();
        }
    }

    public async Task<IActionResult> OnPostCancelAsync(Guid id)
    {
        try
        {
            if (!User.IsInRole("Admin"))
            {
                TempData["ErrorMessage"] = "Only admins can cancel auctions.";
                return RedirectToPage();
            }

            var result = await _auctionService.CancelAuctionAsync(id);

            if (result)
            {
                TempData["SuccessMessage"] = "Auction canceled successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to cancel auction.";
            }

            return RedirectToPage();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error canceling auction {id}");
            TempData["ErrorMessage"] = "An error occurred while canceling the auction.";
            return RedirectToPage();
        }
    }
}
