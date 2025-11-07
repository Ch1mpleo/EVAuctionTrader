using EVAuctionTrader.Business.Interfaces;
using EVAuctionTrader.Business.Utils;
using EVAuctionTrader.BusinessObject.DTOs.PostDTOs;
using EVAuctionTrader.BusinessObject.Enums;
using EVAuctionTrader.DataAccess.Entities;
using EVAuctionTrader.DataAccess.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EVAuctionTrader.Presentation.Pages.PostPages
{
    public class IndexModel : PageModel
    {
        private readonly IPostService _postService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(IPostService postService, ILogger<IndexModel> logger)
        {
            _postService = postService;
            _logger = logger;
        }

        public Pagination<PostResponseDto> Posts { get; set; } = null!;
        public Guid CurrentUserId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 12;

        [BindProperty(SupportsGet = true)]
        public string? Search { get; set; }

        [BindProperty(SupportsGet = true)]
        public PostType? PostType { get; set; }

        [BindProperty(SupportsGet = true)]
        public PostVersion? PostVersion { get; set; }

        [BindProperty(SupportsGet = true)]
        public PostStatus? PostStatus { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool PriceSort { get; set; } = true;

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                Posts = await _postService.GetAllPostsAsync(
                    PageNumber,
                    PageSize,
                    Search,
                    PostType,
                    PostVersion,
                    PostStatus,
                    PriceSort
                );

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading posts");
                TempData["ErrorMessage"] = "An error occurred while loading posts";
                return Page();
            }
        }
    }
}