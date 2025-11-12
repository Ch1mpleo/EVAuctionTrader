namespace EVAuctionTrader.BusinessObject.DTOs.AuctionDTOs;

public class AuctionWithBidsResponseDto : AuctionResponseDto
{
    public List<BidResponseDto> Bids { get; set; } = new();
    public decimal? UserCurrentHold { get; set; }       // Current user's held deposit
    public bool UserCanBid { get; set; }                // Whether current user can place a bid
}
