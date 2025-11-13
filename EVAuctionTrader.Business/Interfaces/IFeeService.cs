using EVAuctionTrader.Business.Utils;
using EVAuctionTrader.BusinessObject.DTOs.FeeDTOs;
using EVAuctionTrader.BusinessObject.Enums;

namespace EVAuctionTrader.Business.Interfaces;

public interface IFeeService
{
    Task<FeeResponseDto> CreateFeeAsync(FeeRequestDto feeRequestDto);
    Task<FeeResponseDto?> GetFeeByIdAsync(Guid id);
    Task<FeeResponseDto?> GetFeeByTypeAsync(FeeType type);
    Task<Pagination<FeeResponseDto>> GetAllFeesAsync(int pageNumber = 1, int pageSize = 10);
    Task<FeeResponseDto?> UpdateFeeAsync(Guid id, FeeRequestDto feeRequestDto);
    Task<bool> DeleteFeeAsync(Guid id);
}
