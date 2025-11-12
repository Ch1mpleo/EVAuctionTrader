using EVAuctionTrader.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EVAuctionTrader.Presentation.Pages.Wallet;

[Authorize(Roles = "Member")]
public sealed class RechargeCancelledModel : PageModel
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<RechargeCancelledModel> _logger;

 public RechargeCancelledModel(
 IPaymentService paymentService,
        ILogger<RechargeCancelledModel> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    public Guid PaymentId { get; set; }
    public bool PaymentCancelled { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid? payment_id)
    {
   try
        {
       if (!payment_id.HasValue)
            {
  _logger.LogWarning("Missing payment_id in cancellation callback");
     TempData["ErrorMessage"] = "Invalid cancellation request.";
 return RedirectToPage("/Wallet/Index");
      }

       PaymentId = payment_id.Value;

      // Cancel the payment in the database
       PaymentCancelled = await _paymentService.CancelPaymentAsync(PaymentId);

 if (PaymentCancelled)
       {
     _logger.LogInformation("Payment {PaymentId} cancelled successfully", PaymentId);
 TempData["InfoMessage"] = "Payment was cancelled. No charges were made.";
            }
   else
            {
           _logger.LogWarning("Payment {PaymentId} cancellation failed", PaymentId);
       }

     return Page();
 }
  catch (Exception ex)
        {
       _logger.LogError(ex, "Error processing payment cancellation");
   TempData["ErrorMessage"] = "An error occurred while processing your cancellation.";
          return RedirectToPage("/Wallet/Index");
        }
    }
}
