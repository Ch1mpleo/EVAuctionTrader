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

        [BindProperty]
        public PostCommentRequestDto NewComment { get; set; } = new();

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

        public async Task<IActionResult> OnPostAddCommentAsync()
        {
            try
            {
                if (!User.Identity?.IsAuthenticated ?? true)
                {
                    TempData["ErrorMessage"] = "You must be logged in to comment";
                    return RedirectToPage(new { id = Id });
                }

                if (string.IsNullOrWhiteSpace(NewComment.Body))
                {
                    TempData["ErrorMessage"] = "Comment cannot be empty";
                    return RedirectToPage(new { id = Id });
                }

                // Set the PostId from the route
                NewComment.PostId = Id;

                var result = await _postService.CreateCommentAsync(NewComment);

                if (result != null)
                {
                    TempData["SuccessMessage"] = "Comment added successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to add comment";
                }

                return RedirectToPage(new { id = Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding comment to post {Id}");
                TempData["ErrorMessage"] = "An error occurred while adding your comment";
                return RedirectToPage(new { id = Id });
            }
        }

        public async Task<IActionResult> OnPostDeleteCommentAsync(Guid commentId)
        {
            try
            {
                if (!User.Identity?.IsAuthenticated ?? true)
                {
                    TempData["ErrorMessage"] = "You must be logged in to delete comments";
                    return RedirectToPage(new { id = Id });
                }

                var result = await _postService.DeleteCommentAsync(commentId);

                if (result)
                {
                    TempData["SuccessMessage"] = "Comment deleted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete comment";
                }

                return RedirectToPage(new { id = Id });
            }
            catch (UnauthorizedAccessException)
            {
                TempData["ErrorMessage"] = "You are not authorized to delete this comment";
                return RedirectToPage(new { id = Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting comment {commentId}");
                TempData["ErrorMessage"] = "An error occurred while deleting the comment";
                return RedirectToPage(new { id = Id });
            }
        }

        public async Task<IActionResult> OnPostCreateConversationAsync([FromBody] CreateConversationDto dto)
        {
            try
            {
                if (!User.Identity?.IsAuthenticated ?? true)
                {
                    return new JsonResult(new { error = "You must be logged in to contact the seller" })
                    {
                        StatusCode = 401
                    };
                }

                if (dto == null || dto.PostId == Guid.Empty)
                {
                    return new JsonResult(new { error = "Invalid request data" })
                    {
                        StatusCode = 400
                    };
                }

                var conversation = await _chatService.CreateOrGetConversationAsync(dto);

                if (conversation == null)
                {
                    return new JsonResult(new { error = "Failed to create conversation. You may be trying to contact yourself." })
                    {
                        StatusCode = 400
                    };
                }

                return new JsonResult(conversation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating conversation for post {PostId}", dto?.PostId);
                return new JsonResult(new { error = "An error occurred while creating the conversation" })
                {
                    StatusCode = 500
                };
            }
        }
    }

    // DTO cho request
    public record CreateConversationRequest(Guid PostId, string? InitialMessage);
}