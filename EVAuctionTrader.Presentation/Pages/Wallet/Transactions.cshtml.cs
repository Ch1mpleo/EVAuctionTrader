using EVAuctionTrader.Business.Interfaces;
using EVAuctionTrader.Business.Utils;
using EVAuctionTrader.BusinessObject.DTOs.WalletTransactionDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace EVAuctionTrader.Presentation.Pages.Wallet;

[Authorize(Roles = "Member")]
public sealed class TransactionsModel : PageModel
{
    private readonly IWalletTransactionService _walletTransactionService;
    private readonly ILogger<TransactionsModel> _logger;

    public TransactionsModel(
        IWalletTransactionService walletTransactionService,
        ILogger<TransactionsModel> logger)
    {
        _walletTransactionService = walletTransactionService;
        _logger = logger;
    }

    public Pagination<WalletTransactionResponseDto>? Transactions { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public int PageSize { get; set; } = 10;

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            Transactions = await _walletTransactionService.GetWalletTransactionsAsync(PageNumber, PageSize);
            return Page();
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access to wallet transactions");
            TempData["ErrorMessage"] = "You don't have permission to view this page.";
            return RedirectToPage("/Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading wallet transactions");
            TempData["ErrorMessage"] = "An error occurred while loading your transactions.";
            Transactions = new Pagination<WalletTransactionResponseDto>(
                new List<WalletTransactionResponseDto>(), 0, 1, 10);
            return Page();
        }
    }
}
