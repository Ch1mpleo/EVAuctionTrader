using EVAuctionTrader.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EVAuctionTrader.Presentation.Pages.Wallet;

[Authorize(Roles = "Member")]
public sealed class RechargeSuccessModel : PageModel
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<RechargeSuccessModel> _logger;

 public RechargeSuccessModel(
    IPaymentService paymentService,
        ILogger<RechargeSuccessModel> logger)
    {
  _paymentService = paymentService;
        _logger = logger;
    }

    public string SessionId { get; set; } = string.Empty;
    public Guid PaymentId { get; set; }
    public bool PaymentConfirmed { get; set; }

    public async Task<IActionResult> OnGetAsync(string? session_id, Guid? payment_id)
    {
  try
        {
            if (string.IsNullOrEmpty(session_id) || !payment_id.HasValue)
  {
        _logger.LogWarning("Missing session_id or payment_id in success callback");
       TempData["ErrorMessage"] = "Invalid payment confirmation.";
  return RedirectToPage("/Wallet/Index");
      }

    SessionId = session_id;
 PaymentId = payment_id.Value;

            // Confirm the payment with Stripe
            PaymentConfirmed = await _paymentService.ConfirmPaymentAsync(PaymentId, SessionId);

      if (PaymentConfirmed)
{
     _logger.LogInformation("Payment {PaymentId} confirmed successfully", PaymentId);
       TempData["SuccessMessage"] = "Payment successful! Your wallet has been recharged.";
  }
  else
    {
          _logger.LogWarning("Payment {PaymentId} confirmation failed", PaymentId);
    TempData["ErrorMessage"] = "Payment confirmation failed. Please contact support if funds were deducted.";
    }

            return Page();
  }
  catch (Exception ex)
     {
         _logger.LogError(ex, "Error processing payment success");
     TempData["ErrorMessage"] = "An error occurred while confirming your payment.";
       return RedirectToPage("/Wallet/Index");
 }
    }
}
