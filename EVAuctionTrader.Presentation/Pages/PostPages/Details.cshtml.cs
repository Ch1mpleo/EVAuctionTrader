using EVAuctionTrader.Business.Interfaces;
using EVAuctionTrader.BusinessObject.DTOs.PostDTOs;
using EVAuctionTrader.DataAccess.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EVAuctionTrader.Presentation.Pages.PostPages
{
    public class DetailsModel : PageModel
    {
        private readonly IPostService _postService;
        private readonly IClaimsService _claimsService;
        private readonly ILogger<DetailsModel> _logger;

        public DetailsModel(IPostService postService, IClaimsService claimsService, ILogger<DetailsModel> logger)
        {
            _postService = postService;
            _claimsService = claimsService;
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }

        public PostResponseDto? Post { get; set; }
        public bool IsAuthor { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                Post = await _postService.GetPostByIdAsync(Id);

                if (Post == null)
                {
                    _logger.LogWarning($"Post with ID {Id} not found");
                    return Page();
                }

                if (User.Identity?.IsAuthenticated == true)
                {
                    var currentUserId = _claimsService.GetCurrentUserId;
                    IsAuthor = Post.AuthorId == currentUserId;
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading post details for ID {Id}");
                TempData["ErrorMessage"] = "An error occurred while loading the post";
                return RedirectToPage("/PostPages/Index");
            }
        }
    }
}