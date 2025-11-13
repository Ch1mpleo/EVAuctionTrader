using EVAuctionTrader.Business.Interfaces;
using EVAuctionTrader.BusinessObject.DTOs.ChatDTOs;
using EVAuctionTrader.DataAccess.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace EVAuctionTrader.Presentation.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;
        private readonly IClaimsService _claimsService;
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(IChatService chatService, IClaimsService claimsService, ILogger<ChatHub> logger)
        {
            _chatService = chatService;
            _claimsService = claimsService;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = _claimsService.GetCurrentUserId;
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
            _logger.LogInformation($"User {userId} connected to ChatHub");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = _claimsService.GetCurrentUserId;
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
            _logger.LogInformation($"User {userId} disconnected from ChatHub");
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(SendMessageDto dto)
        {
            try
            {
                var message = await _chatService.SendMessageAsync(dto);

                if (message != null)
                {
                    var conversation = await _chatService.GetConversationByIdAsync(dto.ConversationId);

                    if (conversation != null)
                    {
                        // Send to buyer
                        await Clients.Group($"user_{conversation.BuyerId}")
                            .SendAsync("ReceiveMessage", message);

                        // Send to seller
                        await Clients.Group($"user_{conversation.SellerId}")
                            .SendAsync("ReceiveMessage", message);

                        // Update conversation list
                        await Clients.Group($"user_{conversation.BuyerId}")
                            .SendAsync("ConversationUpdated", conversation);
                        await Clients.Group($"user_{conversation.SellerId}")
                            .SendAsync("ConversationUpdated", conversation);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SendMessage hub method");
                throw;
            }
        }

        public async Task MarkAsRead(Guid conversationId)
        {
            try
            {
                await _chatService.MarkMessagesAsReadAsync(conversationId);

                var conversation = await _chatService.GetConversationByIdAsync(conversationId);

                if (conversation != null)
                {
                    await Clients.Group($"user_{conversation.BuyerId}")
                        .SendAsync("MessagesMarkedAsRead", conversationId);
                    await Clients.Group($"user_{conversation.SellerId}")
                        .SendAsync("MessagesMarkedAsRead", conversationId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MarkAsRead hub method");
                throw;
            }
        }

        public async Task JoinConversation(Guid conversationId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");
            await _chatService.MarkMessagesAsReadAsync(conversationId);
        }

        public async Task LeaveConversation(Guid conversationId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");
        }
    }
}
