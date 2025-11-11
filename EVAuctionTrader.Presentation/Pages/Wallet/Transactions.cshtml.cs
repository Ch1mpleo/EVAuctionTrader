using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EVAuctionTrader.Presentation.Pages.Wallet;

[Authorize(Roles = "Member")]
public sealed class TransactionsModel : PageModel
{
    public void OnGet()
 {
 // TODO: Implement transaction history
    }
}
