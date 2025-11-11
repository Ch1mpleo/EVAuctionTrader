using EVAuctionTrader.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EVAuctionTrader.Presentation.Pages.Chat
{
    [Authorize]
    public class ConversationsModel : PageModel
    {
        private readonly IChatService _chatService;
        private readonly ILogger<ConversationsModel> _logger;

        public ConversationsModel(IChatService chatService, ILogger<ConversationsModel> logger)
        {
            _chatService = chatService;
            _logger = logger;
        }

        // Default GET: /Chat/Conversations
        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var conversations = await _chatService.GetMyConversationsAsync();
                return new JsonResult(conversations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting conversations");
                return StatusCode(500, new { error = "Failed to load conversations" });
            }
        }

        // GET: /Chat/Conversations?handler=Messages&id={conversationId}
        public async Task<IActionResult> OnGetMessagesAsync(Guid id)
        {
            try
            {
                var messages = await _chatService.GetConversationMessagesAsync(id);
                return new JsonResult(messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting messages for conversation {id}");
                return StatusCode(500, new { error = "Failed to load messages" });
            }
        }

        // GET: /Chat/Conversations?handler=Details&id={conversationId}
        public async Task<IActionResult> OnGetDetailsAsync(Guid id)
        {
            try
            {
                var conversation = await _chatService.GetConversationByIdAsync(id);
                if (conversation == null)
                    return NotFound(new { error = "Conversation not found" });

                return new JsonResult(conversation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting conversation {id}");
                return StatusCode(500, new { error = "Failed to load conversation" });
            }
        }
    }
}
