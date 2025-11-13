using EVAuctionTrader.Business.Interfaces;
using EVAuctionTrader.BusinessObject.DTOs.ChatDTOs;
using EVAuctionTrader.BusinessObject.DTOs.PostDTOs;
using EVAuctionTrader.BusinessObject.Enums;
using EVAuctionTrader.DataAccess.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EVAuctionTrader.Presentation.Pages.PostPages
{
    public class DetailsModel : PageModel
    {
        private readonly IPostService _postService;
        private readonly IChatService _chatService;
        private readonly IClaimsService _claimsService;
        private readonly ILogger<DetailsModel> _logger;

        public DetailsModel(
            IPostService postService,
            IChatService chatService,
            IClaimsService claimsService,
            ILogger<DetailsModel> logger)
        {
            _postService = postService;
            _chatService = chatService;
            _claimsService = claimsService;
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }

        public PostWithCommentResponseDto? Post { get; set; }
        public bool IsAuthor { get; set; }
        public bool IsBanned { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                Post = await _postService.GetPostByIdAsync(Id);

                if (Post == null)
                {
                    _logger.LogWarning($"Post with ID {Id} not found");
                    TempData["ErrorMessage"] = "Post not found";
                    return Page();
                }

                IsBanned = Post.Status == PostStatus.Removed;

                if (User.Identity?.IsAuthenticated == true)
                {
                    var currentUserId = _claimsService.GetCurrentUserId;
                    IsAuthor = Post.AuthorId == currentUserId;

                    if (IsBanned && !IsAuthor && !User.IsInRole("Admin"))
                    {
                        _logger.LogWarning($"Non-authorized user {currentUserId} attempted to view banned post {Id}");
                        TempData["ErrorMessage"] = "This post has been removed and is no longer available";
                        return RedirectToPage("/PostPages/Index");
                    }
                }
                else
                {
                    if (IsBanned)
                    {
                        _logger.LogWarning($"Anonymous user attempted to view banned post {Id}");
                        TempData["ErrorMessage"] = "This post has been removed and is no longer available";
                        return RedirectToPage("/PostPages/Index");
                    }
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

        // ✅ THÊM PAGE HANDLER MỚI
        public async Task<IActionResult> OnPostCreateConversationAsync([FromBody] CreateConversationRequest request)
        {
            try
            {
                if (!User.Identity?.IsAuthenticated ?? true)
                {
                    return new JsonResult(new { error = "You must be logged in" }) { StatusCode = 401 };
                }

                var dto = new CreateConversationDto
                {
                    PostId = request.PostId,
                    InitialMessage = string.IsNullOrWhiteSpace(request.InitialMessage)
                        ? "Hi, I'm interested in this post."
                        : request.InitialMessage
                };

                var conversation = await _chatService.CreateOrGetConversationAsync(dto);

                if (conversation == null)
                {
                    return new JsonResult(new { error = "Cannot create conversation for your own post or post not found" })
                    {
                        StatusCode = 400
                    };
                }

                return new JsonResult(conversation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating conversation");
                return new JsonResult(new { error = ex.Message }) { StatusCode = 500 };
            }
        }
    }

    // DTO cho request
    public record CreateConversationRequest(Guid PostId, string? InitialMessage);
}