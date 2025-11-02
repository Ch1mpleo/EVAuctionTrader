using EVAuctionTrader.Business.Interfaces;
using EVAuctionTrader.BusinessObject.DTOs.AuthDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace EVAuctionTrader.Presentation.Pages.Auth
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration;

        public RegisterModel(IAuthService authService, IConfiguration configuration)
        {
            _authService = authService;
            _configuration = configuration;
        }

        [BindProperty]
        [Required(ErrorMessage = "Full name is required")]
        public string FullName { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        public string Phone { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string? ReturnUrl { get; set; }

        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public void OnGet()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                if (!string.IsNullOrEmpty(ReturnUrl))
                {
                    Response.Redirect(ReturnUrl);
                }
                else
                {
                    Response.Redirect("/Home/LandingPage");
                }
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var registrationRequest = new UserRegistrationDto
                {
                    FullName = FullName,
                    Email = Email,
                    Password = Password,
                    Phone = Phone
                };

                var result = await _authService.RegisterUserAsync(registrationRequest);

                if (result == null)
                {
                    ErrorMessage = "Registration failed. Please try again.";
                    return Page();
                }

                var loginRequest = new LoginRequestDto
                {
                    Email = Email,
                    Password = Password
                };

                var loginResult = await _authService.LoginAsync(loginRequest, _configuration);

                if (loginResult != null)
                {
                    HttpContext.Session.SetString("AuthToken", loginResult.Token);

                    if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
                    {
                        TempData["SuccessMessage"] = "Registration successful! Welcome to EVDealerSales.";
                        return Redirect(ReturnUrl);
                    }
                }

                TempData["SuccessMessage"] = "Registration successful! You are now logged in.";
                return RedirectToPage("/Home/LandingPage");
            }
            catch (Exception ex)
            {
                if (ex.Data.Contains("StatusCode"))
                {
                    var statusCode = (int)ex.Data["StatusCode"]!;
                    if (statusCode == 409)
                    {
                        ErrorMessage = "This email is already registered. Please use a different email.";
                    }
                    else
                    {
                        ErrorMessage = ex.Message;
                    }
                }
                else
                {
                    ErrorMessage = "An error occurred. Please try again later.";
                }
                return Page();
            }
        }
    }
}
