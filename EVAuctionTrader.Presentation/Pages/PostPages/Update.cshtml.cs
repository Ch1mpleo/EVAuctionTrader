using EVAuctionTrader.Business.Interfaces;
using EVAuctionTrader.BusinessObject.DTOs.BatteryDTOs;
using EVAuctionTrader.BusinessObject.DTOs.PostDTOs;
using EVAuctionTrader.BusinessObject.DTOs.VehicleDTOs;
using EVAuctionTrader.BusinessObject.Enums;
using EVAuctionTrader.DataAccess.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EVAuctionTrader.Presentation.Pages.PostPages
{
    [Authorize(Roles = "Member")]
    public class UpdateModel : PageModel
    {
        private readonly IPostService _postService;
        private readonly IClaimsService _claimsService;
        private readonly ILogger<UpdateModel> _logger;

        public UpdateModel(IPostService postService, IClaimsService claimsService, ILogger<UpdateModel> logger)
        {
            _postService = postService;
            _claimsService = claimsService;
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }

        [BindProperty]
        public PostRequestDto PostRequest { get; set; } = new();

        [BindProperty]
        public string? PhotoUrlsText { get; set; }

        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var post = await _postService.GetPostByIdAsync(Id);

                if (post == null)
                {
                    _logger.LogWarning($"Post with ID {Id} not found");
                    TempData["ErrorMessage"] = "Post not found";
                    return RedirectToPage("/PostPages/Index");
                }

                // Check authorization using UserId instead of Name
                var currentUserId = _claimsService.GetCurrentUserId;
                if (post.AuthorId != currentUserId)
                {
                    _logger.LogWarning($"User {currentUserId} attempted to edit post {Id} owned by {post.AuthorId}");
                    TempData["ErrorMessage"] = "You are not authorized to edit this post";
                    return RedirectToPage("/PostPages/Details", new { id = Id });
                }

                // Map to request DTO
                PostRequest = new PostRequestDto
                {
                    PostType = post.PostType,
                    Title = post.Title,
                    Description = post.Description,
                    Price = post.Price,
                    LocationAddress = post.LocationAddress,
                    Version = post.Version,
                    Status = post.Status,
                    PublishedAt = post.PublishedAt,
                    PhotoUrls = post.PhotoUrls ?? new List<string>()
                };

                // Khởi tạo Vehicle hoặc Battery dựa trên PostType
                if (post.PostType == PostType.Vehicle)
                {
                    PostRequest.Vehicle = post.Vehicle != null
                        ? new VehicleRequestPostDto
                        {
                            Brand = post.Vehicle.Brand,
                            Model = post.Vehicle.Model,
                            Year = post.Vehicle.Year,
                            OdometerKm = post.Vehicle.OdometerKm,
                            ConditionGrade = post.Vehicle.ConditionGrade
                        }
                        : new VehicleRequestPostDto(); // Khởi tạo rỗng nếu null
                }
                else if (post.PostType == PostType.Battery)
                {
                    PostRequest.Battery = post.Battery != null
                        ? new BatteryRequestPostDto
                        {
                            Manufacturer = post.Battery.Manufacturer,
                            Chemistry = post.Battery.Chemistry,
                            CapacityKwh = post.Battery.CapacityKwh,
                            CycleCount = post.Battery.CycleCount,
                            SohPercent = post.Battery.SohPercent,
                            VoltageV = post.Battery.VoltageV,
                            ConnectorType = post.Battery.ConnectorType
                        }
                        : new BatteryRequestPostDto(); // Khởi tạo rỗng nếu null
                }

                // Convert photo URLs to text
                if (post.PhotoUrls != null && post.PhotoUrls.Any())
                {
                    PhotoUrlsText = string.Join(Environment.NewLine, post.PhotoUrls);
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading post {Id} for update");
                TempData["ErrorMessage"] = "An error occurred while loading the post";
                return RedirectToPage("/PostPages/Index");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                // Đảm bảo khởi tạo Vehicle/Battery nếu null khi validation fail
                if (PostRequest.PostType == PostType.Vehicle && PostRequest.Vehicle == null)
                {
                    PostRequest.Vehicle = new VehicleRequestPostDto();
                }
                else if (PostRequest.PostType == PostType.Battery && PostRequest.Battery == null)
                {
                    PostRequest.Battery = new BatteryRequestPostDto();
                }
                return Page();
            }

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
                        PostRequest.Vehicle ??= new VehicleRequestPostDto();
                        return Page();
                    }
                    PostRequest.Battery = null;
                }
                else if (PostRequest.PostType == PostType.Battery)
                {
                    if (PostRequest.Battery == null ||
                        string.IsNullOrWhiteSpace(PostRequest.Battery.Manufacturer) ||
                        string.IsNullOrWhiteSpace(PostRequest.Battery.Chemistry))
                    {
                        ErrorMessage = "Battery details are required for battery posts.";
                        PostRequest.Battery ??= new BatteryRequestPostDto();
                        return Page();
                    }
                    PostRequest.Vehicle = null;
                }

                var result = await _postService.UpdatePostAsync(Id, PostRequest);

                if (result == null)
                {
                    ErrorMessage = "Failed to update post. Please try again.";
                    return Page();
                }

                TempData["SuccessMessage"] = "Post updated successfully!";
                return RedirectToPage("/PostPages/Details", new { id = Id });
            }
            catch (UnauthorizedAccessException)
            {
                ErrorMessage = "You are not authorized to update this post.";
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating post {Id}");
                ErrorMessage = ex.Message;

                // Đảm bảo khởi tạo để tránh lỗi khi render lại page
                if (PostRequest.PostType == PostType.Vehicle && PostRequest.Vehicle == null)
                {
                    PostRequest.Vehicle = new VehicleRequestPostDto();
                }
                else if (PostRequest.PostType == PostType.Battery && PostRequest.Battery == null)
                {
                    PostRequest.Battery = new BatteryRequestPostDto();
                }

                return Page();
            }
        }
    }
}