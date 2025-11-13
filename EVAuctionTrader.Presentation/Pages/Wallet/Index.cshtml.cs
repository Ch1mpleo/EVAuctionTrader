using EVAuctionTrader.Business.Interfaces;
using EVAuctionTrader.BusinessObject.DTOs.UserDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EVAuctionTrader.Presentation.Pages.Wallet;

[Authorize(Roles = "Member")]
public sealed class IndexModel : PageModel
{
    private readonly IUserService _userService;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(IUserService userService, ILogger<IndexModel> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    public WalletResponseDto? Wallet { get; set; }
    public decimal Balance { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            Wallet = await _userService.GetMyWalletAsync();
            Balance = await _userService.GetMyBalanceAsync();

            if (Wallet == null)
            {
                _logger.LogWarning("Wallet not found for current user");
                TempData["ErrorMessage"] = "Wallet not found. Please contact support.";
            }

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading wallet");
            TempData["ErrorMessage"] = "An error occurred while loading your wallet.";
            return Page();
        }
    }
}
