namespace EVAuctionTrader.BusinessObject.DTOs.PaymentDTOs;

public sealed class PaymentResponseDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string? PaymentIntentId { get; set; }
    public string? CheckoutSessionId { get; set; }
    public string? CheckoutUrl { get; set; }
}
