using EVAuctionTrader.BusinessObject.DTOs.ChatDTOs;

namespace EVAuctionTrader.Business.Interfaces
{
    public interface IChatService
    {
        Task<ConversationDto?> CreateOrGetConversationAsync(CreateConversationDto dto);
        Task<List<ConversationDto>> GetMyConversationsAsync();
        Task<ConversationDto?> GetConversationByIdAsync(Guid conversationId);
        Task<List<MessageDto>> GetConversationMessagesAsync(Guid conversationId, int pageNumber = 1, int pageSize = 50);
        Task<MessageDto?> SendMessageAsync(SendMessageDto dto);
        Task<bool> MarkMessagesAsReadAsync(Guid conversationId);
        Task<int> GetUnreadCountAsync();
    }
}
