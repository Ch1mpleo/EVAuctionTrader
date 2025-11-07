using EVAuctionTrader.Business.Interfaces;
using EVAuctionTrader.Business.Utils;
using EVAuctionTrader.BusinessObject.DTOs.PostDTOs;
using EVAuctionTrader.BusinessObject.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EVAuctionTrader.Presentation.Pages.PostPages
{
    [Authorize(Roles = "Member")]
    public class MyPostsModel : PageModel
    {
        private readonly IPostService _postService;
        private readonly ILogger<MyPostsModel> _logger;

        public MyPostsModel(IPostService postService, ILogger<MyPostsModel> logger)
        {
            _postService = postService;
            _logger = logger;
        }

        public Pagination<PostResponseDto>? Posts { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public string? Search { get; set; }

        [BindProperty(SupportsGet = true)]
        public PostType? PostType { get; set; }

        [BindProperty(SupportsGet = true)]
        public PostStatus? PostStatus { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool PriceSort { get; set; } = true;

        public async Task OnGetAsync()
        {
            try
            {
                Posts = await _postService.GetAllMemberPostsAsync(
                    pageNumber: PageNumber,
                    pageSize: 10,
                    search: Search,
                    postType: PostType,
                    postStatus: PostStatus,
                    priceSort: PriceSort
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading member posts");
                TempData["ErrorMessage"] = "An error occurred while loading your posts.";
                Posts = new Pagination<PostResponseDto>(new List<PostResponseDto>(), 0, 1, 10);
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid id)
        {
            try
            {
                var result = await _postService.DeletePostAsync(id);

                if (result)
                {
                    TempData["SuccessMessage"] = "Post deleted successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete post. Post may not exist.";
                }
            }
            catch (UnauthorizedAccessException)
            {
                TempData["ErrorMessage"] = "You can only delete your own posts.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting post {id}");
                TempData["ErrorMessage"] = "An error occurred while deleting the post.";
            }

            return RedirectToPage(new
            {
                pageNumber = PageNumber,
                search = Search,
                postType = PostType,
                postStatus = PostStatus,
                priceSort = PriceSort
            });
        }
    }
}