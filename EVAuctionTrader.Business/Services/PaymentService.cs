using EVAuctionTrader.Business.Interfaces;
using EVAuctionTrader.BusinessObject.DTOs.PaymentDTOs;
using EVAuctionTrader.BusinessObject.Enums;
using EVAuctionTrader.DataAccess.Entities;
using EVAuctionTrader.DataAccess.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe.Checkout;

namespace EVAuctionTrader.Business.Services;

public sealed class PaymentService : IPaymentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClaimsService _claimsService;
    private readonly ILogger<PaymentService> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _stripeSecretKey;

    public PaymentService(
        IUnitOfWork unitOfWork,
        IClaimsService claimsService,
        ILogger<PaymentService> logger,
        IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _claimsService = claimsService;
        _logger = logger;
        _configuration = configuration;
        _stripeSecretKey = _configuration["Stripe:SecretKey"]
            ?? throw new InvalidOperationException("Stripe:SecretKey configuration is missing");
    }

    public async Task<PaymentResponseDto> CreateCheckoutSessionAsync(PaymentRequestDto request)
    {
        try
        {
            _logger.LogInformation("Creating Stripe checkout session for amount: ${Amount}", request.Amount);

            if (request.Amount <= 0)
            {
                throw new ArgumentException("Amount must be greater than zero.", nameof(request.Amount));
            }

            var userId = _claimsService.GetCurrentUserId;
            var user = await _unitOfWork.Users.GetByIdAsync(userId);

            if (user == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            // Create payment record with Pending status
            var payment = new Payment
            {
                UserId = userId,
                Amount = request.Amount,
                Status = PaymentStatus.Pending,
                PaymentDate = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };

            await _unitOfWork.Payments.AddAsync(payment);
            await _unitOfWork.SaveChangesAsync();

            var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "https://localhost:7067";

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "Wallet Recharge",
                                Description = $"Add ${request.Amount:N2} to your wallet"
                            },
                            UnitAmount = (long)(request.Amount * 100)
                        },
                        Quantity = 1
                    }
                },
                Mode = "payment",
                SuccessUrl = $"{baseUrl}/Wallet/RechargeSuccess?session_id={{CHECKOUT_SESSION_ID}}&payment_id={payment.Id}",
                CancelUrl = $"{baseUrl}/Wallet/RechargeCancelled?payment_id={payment.Id}",
                ClientReferenceId = payment.Id.ToString(),
                CustomerEmail = user.Email,
                Metadata = new Dictionary<string, string>
                {
                    { "payment_id", payment.Id.ToString() },
                    { "user_id", userId.ToString() },
                    { "amount", request.Amount.ToString() }
                }
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);

            // Update payment with CheckoutSessionId
            payment.CheckoutSessionId = session.Id;
            await _unitOfWork.Payments.Update(payment);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Checkout session created: {SessionId} for payment: {PaymentId} with status Pending",
                session.Id, payment.Id);

            return new PaymentResponseDto
            {
                Id = payment.Id,
                UserId = payment.UserId,
                Amount = payment.Amount,
                PaymentDate = payment.PaymentDate,
                CheckoutSessionId = session.Id,
                CheckoutUrl = session.Url
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating checkout session");
            throw;
        }
    }

    public async Task<bool> ConfirmPaymentAsync(Guid paymentId, string sessionId)
    {
        try
        {
            _logger.LogInformation("Confirming payment {PaymentId} with session {SessionId}", paymentId, sessionId);

            var payment = await _unitOfWork.Payments.GetByIdAsync(paymentId);

            if (payment == null)
            {
                _logger.LogWarning("Payment {PaymentId} not found", paymentId);
                return false;
            }

            // Validate that the session belongs to this payment
            if (payment.CheckoutSessionId != sessionId)
            {
                _logger.LogWarning("Session {SessionId} does not match payment {PaymentId}. Expected: {ExpectedSessionId}",
                    sessionId, paymentId, payment.CheckoutSessionId);
                return false;
            }

            // Check if payment is already completed (idempotency)
            if (payment.Status == PaymentStatus.Completed)
            {
                _logger.LogInformation("Payment {PaymentId} is already completed", paymentId);
                return true;
            }

            // Only process pending payments
            if (payment.Status != PaymentStatus.Pending)
            {
                _logger.LogWarning("Cannot confirm payment {PaymentId} with status {Status}",
                    paymentId, payment.Status);
                return false;
            }

            // Verify with Stripe
            var sessionService = new SessionService();
            var session = await sessionService.GetAsync(sessionId);

            if (session.PaymentStatus == "paid")
            {
                return await CompletePaymentAsync(payment, session.PaymentIntentId);
            }

            _logger.LogWarning("Payment {PaymentId} not yet paid. Session status: {Status}",
                paymentId, session.PaymentStatus);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming payment {PaymentId}", paymentId);
            throw;
        }
    }

    public async Task<bool> CancelPaymentAsync(Guid paymentId)
    {
        try
        {
            _logger.LogInformation("Cancelling payment {PaymentId}", paymentId);

            var payment = await _unitOfWork.Payments.GetByIdAsync(paymentId);

            if (payment == null)
            {
                _logger.LogWarning("Payment {PaymentId} not found", paymentId);
                return false;
            }

            // Only cancel if payment is still pending
            if (payment.Status == PaymentStatus.Pending)
            {
                payment.Status = PaymentStatus.Canceled;
                await _unitOfWork.Payments.Update(payment);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Payment {PaymentId} marked as cancelled", paymentId);
                return true;
            }

            _logger.LogWarning("Cannot cancel payment {PaymentId} with status {Status}",
                paymentId, payment.Status);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling payment {PaymentId}", paymentId);
            throw;
        }
    }

    private async Task<bool> CompletePaymentAsync(Payment payment, string? paymentIntentId)
    {
        try
        {
            // Get wallet first to validate it exists
            var wallet = await _unitOfWork.Wallets.FirstOrDefaultAsync(w => w.UserId == payment.UserId);

            if (wallet == null)
            {
                throw new InvalidOperationException($"Wallet not found for user {payment.UserId}");
            }

            var previousBalance = wallet.Balance;
            var newBalance = previousBalance + payment.Amount;

            // Update payment status to Completed
            payment.Status = PaymentStatus.Completed;
            payment.PaymentIntentId = paymentIntentId;
            await _unitOfWork.Payments.Update(payment);

            // Update user wallet balance
            wallet.Balance = newBalance;
            await _unitOfWork.Wallets.Update(wallet);

            // Save all changes in a single transaction
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Payment {PaymentId} completed successfully. User {UserId} wallet credited with ${Amount}. Balance: ${PreviousBalance} -> ${NewBalance}",
                payment.Id, payment.UserId, payment.Amount, previousBalance, newBalance);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing payment {PaymentId}. Payment status may need manual verification.", payment.Id);
            throw;
        }
    }
}
