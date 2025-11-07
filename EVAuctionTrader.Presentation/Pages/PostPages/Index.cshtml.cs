using EVAuctionTrader.Business.Interfaces;
using EVAuctionTrader.Business.Utils;
using EVAuctionTrader.BusinessObject.DTOs.PostDTOs;
using EVAuctionTrader.BusinessObject.Enums;
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

        public Pagination<PostResponseDto>? Posts { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

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

        [BindProperty(SupportsGet = true)]
        public decimal? MinPrice { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? MaxPrice { get; set; }

        // ✅ MỚI: Danh sách các khoảng giá gợi ý
        public List<PriceRangeOption> PriceRanges { get; } = new()
        {
            new PriceRangeOption { Label = "All Prices", MinPrice = null, MaxPrice = null },
            new PriceRangeOption { Label = "Under 100M", MinPrice = null, MaxPrice = 100000000 },
            new PriceRangeOption { Label = "100M - 300M", MinPrice = 100000000, MaxPrice = 300000000 },
            new PriceRangeOption { Label = "300M - 500M", MinPrice = 300000000, MaxPrice = 500000000 },
            new PriceRangeOption { Label = "500M - 1B", MinPrice = 500000000, MaxPrice = 1000000000 },
            new PriceRangeOption { Label = "1B - 2B", MinPrice = 1000000000, MaxPrice = 2000000000 },
            new PriceRangeOption { Label = "Above 2B", MinPrice = 2000000000, MaxPrice = null }
        };

        public async Task OnGetAsync()
        {
            try
            {
                Posts = await _postService.GetAllPostsAsync(
                    pageNumber: PageNumber,
                    pageSize: 12,
                    search: Search,
                    postType: PostType,
                    postVersion: PostVersion,
                    postStatus: PostStatus,
                    priceSort: PriceSort,
                    minPrice: MinPrice,
                    maxPrice: MaxPrice
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading posts");
                TempData["ErrorMessage"] = "An error occurred while loading posts.";
                Posts = new Pagination<PostResponseDto>(new List<PostResponseDto>(), 0, 1, 12);
            }
        }

        public async Task<IActionResult> OnPostBanAsync(Guid id)
        {
            try
            {
                if (!User.IsInRole("Admin"))
                {
                    TempData["ErrorMessage"] = "You don't have permission to ban posts.";
                    return RedirectToPage();
                }

                var result = await _postService.BanPostAsync(id);

                if (result)
                {
                    TempData["SuccessMessage"] = "Post has been banned successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to ban post. Post may not exist.";
                }
            }
            catch (UnauthorizedAccessException)
            {
                TempData["ErrorMessage"] = "You don't have permission to ban posts.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error banning post {id}");
                TempData["ErrorMessage"] = "An error occurred while banning the post.";
            }

            return RedirectToPage(new
            {
                pageNumber = PageNumber,
                search = Search,
                postType = PostType,
                postVersion = PostVersion,
                postStatus = PostStatus,
                priceSort = PriceSort,
                minPrice = MinPrice,
                maxPrice = MaxPrice
            });
        }
    }

    // ✅ MỚI: Class để định nghĩa khoảng giá
    public class PriceRangeOption
    {
        public string Label { get; set; } = string.Empty;
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
    }
}