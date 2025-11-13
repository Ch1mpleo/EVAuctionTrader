using EVAuctionTrader.Business.Interfaces;
using EVAuctionTrader.Business.Utils;
using EVAuctionTrader.BusinessObject.DTOs.AuctionDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EVAuctionTrader.Presentation.Pages.AuctionPages;

[Authorize(Roles = "Member")]
public sealed class MyWonAuctionsModel : PageModel
{
    private readonly IAuctionService _auctionService;
    private readonly ILogger<MyWonAuctionsModel> _logger;

    public MyWonAuctionsModel(IAuctionService auctionService, ILogger<MyWonAuctionsModel> logger)
    {
        _auctionService = auctionService;
        _logger = logger;
    }

    public Pagination<AuctionResponseDto>? WonAuctions { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    public async Task OnGetAsync()
    {
        try
        {
            WonAuctions = await _auctionService.GetMyWonAuctionsAsync(
                pageNumber: PageNumber,
                pageSize: 12,
                search: Search
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading won auctions");
            TempData["ErrorMessage"] = "An error occurred while loading your won auctions.";
            WonAuctions = new Pagination<AuctionResponseDto>(new List<AuctionResponseDto>(), 0, 1, 12);
        }
    }
}
