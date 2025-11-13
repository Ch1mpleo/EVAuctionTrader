using EVAuctionTrader.BusinessObject.DTOs.UserDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVAuctionTrader.Business.Interfaces
{
    public interface IUserService
    {
        Task<UserResponseDto?> GetMyProfileAsync();
        Task<UserResponseDto?> UpdateMyProfileAsync(UpdateProfileRequestDto updateProfileRequestDto);
        Task<WalletResponseDto?> GetMyWalletAsync();
        Task<decimal> GetMyBalanceAsync();
        Task<decimal> GetMyHeldBalanceAsync();
    }
}
