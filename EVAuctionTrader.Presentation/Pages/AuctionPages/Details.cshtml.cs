using EVAuctionTrader.Business.Interfaces;
using EVAuctionTrader.BusinessObject.DTOs.AuctionDTOs;
using EVAuctionTrader.BusinessObject.Enums;
using EVAuctionTrader.Presentation.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;

namespace EVAuctionTrader.Presentation.Pages.AuctionPages;

public sealed class DetailsModel : PageModel
{
    private readonly IAuctionService _auctionService;
    private readonly IHubContext<AuctionHub> _hubContext;
    private readonly ILogger<DetailsModel> _logger;

    public DetailsModel(
        IAuctionService auctionService,
        IHubContext<AuctionHub> hubContext,
        ILogger<DetailsModel> logger)
    {
        _auctionService = auctionService;
        _hubContext = hubContext;
        _logger = logger;
    }

    public AuctionWithBidsResponseDto? Auction { get; set; }

    [BindProperty]
    public decimal BidAmount { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        try
        {
            Auction = await _auctionService.GetAuctionByIdAsync(id);

            if (Auction == null)
            {
                TempData["ErrorMessage"] = "Auction not found.";
                return RedirectToPage("/AuctionPages/Index");
            }

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving auction {id}");
            TempData["ErrorMessage"] = "Failed to load auction details.";
            return RedirectToPage("/AuctionPages/Index");
        }
    }

    public async Task<IActionResult> OnPostPlaceBidAsync(Guid id)
    {
        try
        {
            if (!User.Identity?.IsAuthenticated ?? false)
            {
                TempData["ErrorMessage"] = "You must be logged in to place a bid.";
                return RedirectToPage("/Auth/Login");
            }

            if (User.IsInRole("Admin"))
            {
                TempData["ErrorMessage"] = "Admins cannot place bids.";
                return RedirectToPage("/AuctionPages/Details", new { id });
            }

            var bidRequest = new BidRequestDto
            {
                Amount = BidAmount
            };

            var bidResponse = await _auctionService.PlaceBidAsync(id, bidRequest);

            if (bidResponse != null)
            {
                // Notify all users in the auction room via SignalR
                await _hubContext.Clients.Group($"auction_{id}")
                    .SendAsync("ReceiveBid", new
                    {
                        bidderName = bidResponse.BidderName,
                        amount = bidResponse.Amount,
                        timestamp = bidResponse.CreatedAt.ToString("HH:mm:ss")
                    });

                TempData["SuccessMessage"] = $"Bid placed successfully: ${bidResponse.Amount:N0}";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to place bid.";
            }

            return RedirectToPage("/AuctionPages/Details", new { id });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, $"Bid validation failed for auction {id}");
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToPage("/AuctionPages/Details", new { id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error placing bid on auction {id}");
            TempData["ErrorMessage"] = "An error occurred while placing your bid.";
            return RedirectToPage("/AuctionPages/Details", new { id });
        }
    }
}
