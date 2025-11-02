using EVAuctionTrader.BusinessObject.DTOs.AuthDTOs;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVAuctionTrader.Business.Interfaces
{
    public interface IAuthService
    {
        Task<UserDto?> RegisterUserAsync(UserRegistrationDto userRegistrationDto);
        Task<LoginResponseDto?> LoginAsync(LoginRequestDto loginRequestDto, IConfiguration configuration);
        Task<bool> LogoutAsync(Guid userId);
    }
}
