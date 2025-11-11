using EVAuctionTrader.Business.Interfaces;
using EVAuctionTrader.BusinessObject.DTOs.AuctionDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EVAuctionTrader.Presentation.Pages.AuctionPages;

[Authorize(Roles = "Admin")]
public sealed class CreateModel : PageModel
{
    private readonly IAuctionService _auctionService;
    private readonly ILogger<CreateModel> _logger;

    public CreateModel(IAuctionService auctionService, ILogger<CreateModel> logger)
    {
        _auctionService = auctionService;
        _logger = logger;
    }

    [BindProperty]
    public AuctionRequestDto AuctionRequest { get; set; } = new();

    public IActionResult OnGet()
    {
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Validate dates
            if (AuctionRequest.StartTime >= AuctionRequest.EndTime)
            {
                ModelState.AddModelError(string.Empty, "Start time must be before end time.");
                return Page();
            }

            if (AuctionRequest.StartTime < DateTime.UtcNow)
            {
                ModelState.AddModelError(string.Empty, "Start time cannot be in the past.");
                return Page();
            }

            // Validate deposit rate
            if (AuctionRequest.DepositRate <= 0 || AuctionRequest.DepositRate > 1)
            {
                ModelState.AddModelError(string.Empty, "Deposit rate must be between 0 and 100%.");
                return Page();
            }

            var result = await _auctionService.CreateAuctionAsync(AuctionRequest);

            if (result != null)
            {
                TempData["SuccessMessage"] = "Auction created successfully!";
                return RedirectToPage("/AuctionPages/Details", new { id = result.Id });
            }

            ModelState.AddModelError(string.Empty, "Failed to create auction.");
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating auction");
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }
    }
}
