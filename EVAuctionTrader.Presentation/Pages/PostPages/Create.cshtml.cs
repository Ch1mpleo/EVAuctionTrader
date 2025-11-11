using EVAuctionTrader.Business.Interfaces;
using EVAuctionTrader.BusinessObject.DTOs.BatteryDTOs;
using EVAuctionTrader.BusinessObject.DTOs.PostDTOs;
using EVAuctionTrader.BusinessObject.DTOs.VehicleDTOs;
using EVAuctionTrader.BusinessObject.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EVAuctionTrader.Presentation.Pages.PostPages
{
    [Authorize(Roles = "Member")]
    public class CreateModel : PageModel
    {
        private readonly IPostService _postService;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(IPostService postService, ILogger<CreateModel> logger)
        {
            _postService = postService;
            _logger = logger;
        }

        [BindProperty]
        public PostRequestDto PostRequest { get; set; } = new()
        {
            PostType = PostType.Vehicle,
            Version = PostVersion.Free,
            Status = PostStatus.Draft,
            PhotoUrls = new List<string>(),
            Vehicle = new VehicleRequestPostDto(),
            Battery = new BatteryRequestPostDto() 
        };

        [BindProperty]
        public string? PhotoUrlsText { get; set; }

        public string? ErrorMessage { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                // Parse photo URLs
                if (!string.IsNullOrWhiteSpace(PhotoUrlsText))
                {
                    PostRequest.PhotoUrls = PhotoUrlsText
                        .Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(url => url.Trim())
                        .Where(url => !string.IsNullOrWhiteSpace(url))
                        .ToList();
                }

                // Validate based on post type
                if (PostRequest.PostType == PostType.Vehicle)
                {
                    if (PostRequest.Vehicle == null ||
                        string.IsNullOrWhiteSpace(PostRequest.Vehicle.Brand) ||
                        string.IsNullOrWhiteSpace(PostRequest.Vehicle.Model))
                    {
                        ErrorMessage = "Vehicle details are required for vehicle posts.";
                        return Page();
                    }
                    PostRequest.Battery = null; // Clear Battery data
                }
                else if (PostRequest.PostType == PostType.Battery)
                {
                    if (PostRequest.Battery == null ||
                        string.IsNullOrWhiteSpace(PostRequest.Battery.Manufacturer) ||
                        string.IsNullOrWhiteSpace(PostRequest.Battery.Chemistry))
                    {
                        ErrorMessage = "Battery details are required for battery posts.";
                        return Page();
                    }
                    PostRequest.Vehicle = null; // Clear Vehicle data
                }

                var result = await _postService.CreatePostAsync(PostRequest);

                if (result == null)
                {
                    ErrorMessage = "Failed to create post. Please try again.";
                    return Page();
                }

                TempData["SuccessMessage"] = "Post created successfully!";
                return RedirectToPage("/PostPages/Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating post");
                ErrorMessage = $"Error: {ex.Message}";
                return Page();
            }
        }
    }
}