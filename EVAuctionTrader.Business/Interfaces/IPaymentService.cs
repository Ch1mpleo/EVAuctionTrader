using EVAuctionTrader.BusinessObject.DTOs.PaymentDTOs;

namespace EVAuctionTrader.Business.Interfaces;

public interface IPaymentService
{
    Task<PaymentResponseDto> CreateCheckoutSessionAsync(PaymentRequestDto request);
    Task<bool> ConfirmPaymentAsync(Guid paymentId, string sessionId);
    Task<bool> CancelPaymentAsync(Guid paymentId);
}
