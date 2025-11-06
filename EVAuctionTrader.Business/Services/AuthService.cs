using EVAuctionTrader.Business.Interfaces;
using EVAuctionTrader.Business.Utils;
using EVAuctionTrader.BusinessObject.DTOs.AuthDTOs;
using EVAuctionTrader.BusinessObject.Enums;
using EVAuctionTrader.DataAccess.Entities;
using EVAuctionTrader.DataAccess.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;

namespace EVAuctionTrader.Business.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger _logger;

        public AuthService(IUnitOfWork unitOfWork, ILogger<AuthService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto loginRequestDto, IConfiguration configuration)
        {
            try
            {
                _logger.LogInformation($"Attempting login for {loginRequestDto?.Email}");

                if (loginRequestDto == null || string.IsNullOrWhiteSpace(loginRequestDto.Email) || string.IsNullOrWhiteSpace(loginRequestDto.Password))
                {
                    _logger.LogWarning("Login failed: missing email or password.");
                    throw new ArgumentException("Email and password are required.");
                }

                var user = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Email == loginRequestDto.Email && !u.IsDeleted);

                if (user == null)
                {
                    _logger.LogWarning($"Login failed: user {loginRequestDto.Email} not found or inactive.");
                    return null;
                }

                var passwordHasher = new PasswordHasher();
                if (!passwordHasher.VerifyPassword(loginRequestDto.Password, user.PasswordHash))
                {
                    _logger.LogWarning($"Login failed: invalid password for {loginRequestDto.Email}.");
                    return null;
                }

                var jwtToken = JwtUtils.GenerateJwtToken(
                    user.Id,
                    user.Email,
                    user.Role.ToString(),
                    configuration,
                    TimeSpan.FromHours(8)
                );

                var response = new LoginResponseDto
                {
                    Token = jwtToken
                };

                _logger.LogInformation($"Login successful for {user.Email}");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Login error for {loginRequestDto?.Email}: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> LogoutAsync(Guid userId)
        {
            try
            {
                _logger.LogInformation($"User with ID {userId} logged out");
                await Task.CompletedTask;
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Logout error for user {userId}: {ex.Message}");
                throw;
            }
        }

        public async Task<UserDto?> RegisterUserAsync(UserRegistrationDto userRegistrationDto)
        {
            try
            {
                _logger.LogInformation("Registering new user");

                if (await UserExistsAsync(userRegistrationDto.Email))
                {
                    _logger.LogWarning($"Registration failed: email {userRegistrationDto.Email} already in use.");
                    return null;
                }

                var hashedPassword = new PasswordHasher().HashPassword(userRegistrationDto.Password);

                var user = new User
                {
                    FullName = userRegistrationDto.FullName,
                    Email = userRegistrationDto.Email,
                    Phone = userRegistrationDto.Phone,
                    Role = RoleType.Member,
                    Status = "Active",
                    PasswordHash = hashedPassword ?? throw new Exception("Password hashing failed."),
                };

                await _unitOfWork.Users.AddAsync(user);

                var wallet = new Wallet
                {
                    UserId = user.Id,
                    Balance = 0m
                };

                await _unitOfWork.Wallets.AddAsync(wallet);

                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation($"User {user.Email} registered successfully and Wallet {wallet.Id} initialized.");

                var userDto = new UserDto
                {
                    FullName = user.FullName,
                    Email = user.Email,
                    Phone = user.Phone,
                    Role = user.Role,
                };

                return userDto;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating account: {ex.Message}");
                throw;
            }
        }
        private async Task<bool> UserExistsAsync(string email)
        {
            var accounts = await _unitOfWork.Users.GetAllAsync();
            return accounts.Any(a => a.Email == email);
        }
    }
}
