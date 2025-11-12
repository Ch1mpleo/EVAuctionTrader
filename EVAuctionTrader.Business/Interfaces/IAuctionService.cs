using EVAuctionTrader.Business.Utils;
using EVAuctionTrader.BusinessObject.DTOs.AuctionDTOs;
using EVAuctionTrader.BusinessObject.Enums;

namespace EVAuctionTrader.Business.Interfaces
{
    public interface IAuctionService
    {
        // Admin
        Task<AuctionResponseDto?> CreateAuctionAsync(AuctionRequestDto createAuctionDto);
        Task<bool> CancelAuctionAsync(Guid auctionId);

        // Public 
        Task<Pagination<AuctionResponseDto>> GetAllAuctionsAsync(
            int pageNumber = 1,
            int pageSize = 10,
            string? search = null,
            AuctionType? auctionType = null,
            AuctionStatus? auctionStatus = null,
            bool priceSort = true);

        Task<AuctionWithBidsResponseDto?> GetAuctionByIdAsync(Guid auctionId);

        // Bidding
        Task<BidResponseDto?> PlaceBidAsync(Guid auctionId, BidRequestDto bidRequest);

        // Background/Scheduled operations
        Task UpdateAuctionStatusesAsync();
        Task FinalizeAuctionAsync(Guid auctionId);
    }
}
