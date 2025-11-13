using EVAuctionTrader.Business.Utils;
using EVAuctionTrader.BusinessObject.DTOs.WalletTransactionDTOs;

namespace EVAuctionTrader.Business.Interfaces;

public interface IWalletTransactionService
{
    Task<WalletTransactionResponseDto> CreateWalletTransactionAsync(WalletTransactionRequestDto walletTransactionRequestDto);
    Task<WalletTransactionResponseDto?> GetWalletTransactionByIdAsync(Guid id);
    Task<Pagination<WalletTransactionResponseDto>> GetWalletTransactionsAsync(int pageNumber = 1, int pageSize = 10);
    Task<bool> DeleteWalletTransactionAsync(Guid id);
}
