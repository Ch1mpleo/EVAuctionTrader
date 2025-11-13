using EVAuctionTrader.Business.Interfaces;
using EVAuctionTrader.BusinessObject.DTOs.PaymentDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EVAuctionTrader.Presentation.Pages.Wallet;

[Authorize(Roles = "Member")]
public sealed class RechargeModel : PageModel
{
    private readonly IPaymentService _paymentService;
  private readonly IUserService _userService;
private readonly ILogger<RechargeModel> _logger;

    public RechargeModel(
 IPaymentService paymentService,
        IUserService userService,
        ILogger<RechargeModel> logger)
    {
        _paymentService = paymentService;
        _userService = userService;
        _logger = logger;
    }

    [BindProperty]
    public decimal Amount { get; set; } = 10.00m;

    public decimal CurrentBalance { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        try
  {
            CurrentBalance = await _userService.GetMyBalanceAsync();
      return Page();
        }
        catch (Exception ex)
     {
     _logger.LogError(ex, "Error loading recharge page");
     TempData["ErrorMessage"] = "An error occurred while loading the page.";
 return Page();
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
      {
      if (Amount <= 0)
          {
       TempData["ErrorMessage"] = "Amount must be greater than zero.";
                CurrentBalance = await _userService.GetMyBalanceAsync();
      return Page();
      }

            if (Amount > 10000)
            {
     TempData["ErrorMessage"] = "Maximum recharge amount is $10,000.";
     CurrentBalance = await _userService.GetMyBalanceAsync();
    return Page();
    }

            var request = new PaymentRequestDto
     {
          Amount = Amount
            };

    var response = await _paymentService.CreateCheckoutSessionAsync(request);

        if (response != null && !string.IsNullOrEmpty(response.CheckoutUrl))
          {
        _logger.LogInformation("Redirecting to Stripe checkout: {CheckoutUrl}", response.CheckoutUrl);
    return Redirect(response.CheckoutUrl);
}

            TempData["ErrorMessage"] = "Failed to create checkout session. Please try again.";
     CurrentBalance = await _userService.GetMyBalanceAsync();
  return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating checkout session");
        TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
         CurrentBalance = await _userService.GetMyBalanceAsync();
      return Page();
        }
    }
}
