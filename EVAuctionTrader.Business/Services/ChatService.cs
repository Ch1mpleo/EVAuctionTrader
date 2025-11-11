using EVAuctionTrader.Business.Interfaces;
using EVAuctionTrader.BusinessObject.DTOs.ChatDTOs;
using EVAuctionTrader.DataAccess.Entities;
using EVAuctionTrader.DataAccess.Interfaces;
using Microsoft.Extensions.Logging;

namespace EVAuctionTrader.Business.Services
{
    public class ChatService : IChatService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClaimsService _claimsService;
        private readonly ILogger<ChatService> _logger;

        public ChatService(IUnitOfWork unitOfWork, IClaimsService claimsService, ILogger<ChatService> logger)
        {
            _unitOfWork = unitOfWork;
            _claimsService = claimsService;
            _logger = logger;
        }

        public async Task<ConversationDto?> CreateOrGetConversationAsync(CreateConversationDto dto)
        {
            try
            {
                var currentUserId = _claimsService.GetCurrentUserId;
                var post = await _unitOfWork.Posts.GetByIdAsync(dto.PostId);

                if(post ==null || post.IsDeleted)
                {
                    _logger.LogWarning("Post with ID {PostId} not found or is deleted.", dto.PostId);
                    return null;
                }

                if (post.AuthorId == currentUserId)
                {
                    _logger.LogWarning("User {UserId} attempted to create a conversation on their own post {PostId}.", currentUserId, dto.PostId);
                    return null;
                }

                var existingConversation = await _unitOfWork.Conversations
                    .FirstOrDefaultAsync(c => c.PostId == dto.PostId && c.BuyerId == currentUserId && !c.IsDeleted);
                
                if(existingConversation != null)
                {
                    return await GetConversationByIdAsync(existingConversation.Id);
                }

                var conversation = new Conversation
                {
                    PostId = dto.PostId,
                    SellerId = post.AuthorId,
                    BuyerId = currentUserId,
                    Status = "Active",
                };

                await _unitOfWork.Conversations.AddAsync(conversation);

                var message = new Message
                {
                    ConversationId = conversation.Id,
                    SenderId = currentUserId,
                    Body = dto.InitialMessage,
                };

                await _unitOfWork.Messages.AddAsync(message);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"Created conversation {conversation.Id} for post {dto.PostId}");

                return await GetConversationByIdAsync(conversation.Id);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error creating conversation");
                throw;
            }
        }

        public async Task<ConversationDto?> GetConversationByIdAsync(Guid conversationId)
        {
            try
            {
                var currentUserId = _claimsService.GetCurrentUserId;

                var conversation = await _unitOfWork.Conversations.FirstOrDefaultAsync(
                    c => c.Id == conversationId && !c.IsDeleted,
                    c => c.Post,
                    c => c.Seller,
                    c => c.Buyer,
                    c => c.Messages
                );

                if (conversation == null)
                {
                    return null;
                }

                if (conversation.BuyerId != currentUserId && conversation.SellerId != currentUserId)
                {
                    _logger.LogWarning("User {UserId} attempted to access conversation {ConversationId} without permission.", currentUserId, conversationId);
                    return null;
                }

                var lastMessage = conversation.Messages
                    .Where(m => !m.IsDeleted)
                    .OrderByDescending(m => m.CreatedAt)
                    .FirstOrDefault();

                var unreadCount = conversation.Messages
                   .Count(m => !m.IsDeleted && m.SenderId != currentUserId && m.ReadAt == null);

                return new ConversationDto
                {
                    Id = conversation.Id,
                    PostId = conversation.PostId,
                    PostTitle = conversation.Post.Title,
                    SellerId = conversation.SellerId,
                    SellerName = conversation.Seller.FullName,
                    BuyerId = conversation.BuyerId,
                    BuyerName = conversation.Buyer.FullName,
                    Status = conversation.Status,
                    CreatedAt = conversation.CreatedAt,
                    LastMessage = lastMessage != null ? new MessageDto
                    {
                        Id = lastMessage.Id,
                        Body = lastMessage.Body,
                        CreatedAt = lastMessage.CreatedAt,
                        SenderId = lastMessage.SenderId,
                        IsCurrentUser = lastMessage.SenderId == currentUserId
                    } : null,
                    UnreadCount = unreadCount
                };

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting conversation by ID");
                throw;
            }
        }

        public async Task<List<MessageDto>> GetConversationMessagesAsync(Guid conversationId, int pageNumber = 1, int pageSize = 50)
        {
            try
            {
                var currentUserId = _claimsService.GetCurrentUserId;

                var conversation = await _unitOfWork.Conversations.GetByIdAsync(conversationId);

                if (conversation == null || conversation.IsDeleted)
                {
                    return new List<MessageDto>();
                }

                // Verify user is part of conversation
                if (conversation.BuyerId != currentUserId && conversation.SellerId != currentUserId)
                {
                    _logger.LogWarning($"User {currentUserId} not authorized for conversation {conversationId}");
                    return new List<MessageDto>();
                }

                var messages = await _unitOfWork.Messages.GetAllAsync(
                    predicate: m => m.ConversationId == conversationId && !m.IsDeleted,
                    m => m.Sender
                );

                var messageDtos = messages
                    .OrderBy(m => m.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(m => new MessageDto
                    {
                        Id = m.Id,
                        ConversationId = m.ConversationId,
                        SenderId = m.SenderId,
                        SenderName = m.Sender.FullName,
                        Body = m.Body,
                        ReadAt = m.ReadAt,
                        CreatedAt = m.CreatedAt,
                        IsCurrentUser = m.SenderId == currentUserId
                    })
                    .ToList();

                return messageDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting messages for conversation {conversationId}");
                throw;
            }
        }

        public async Task<List<ConversationDto>> GetMyConversationsAsync()
        {
            try
            {
                var currentUserId = _claimsService.GetCurrentUserId;

                var conversations = await _unitOfWork.Conversations.GetAllAsync(
                    predicate: c => (c.BuyerId == currentUserId || c.SellerId == currentUserId) && !c.IsDeleted,
                    c => c.Post,
                    c => c.Seller,
                    c => c.Buyer,
                    c => c.Messages
                );

                var conversationDtos = new List<ConversationDto>();

                foreach (var conv in conversations.OrderByDescending(c => c.UpdatedAt ?? c.CreatedAt))
                {
                    var lastMessage = conv.Messages
                        .Where(m => !m.IsDeleted)
                        .OrderByDescending(m => m.CreatedAt)
                        .FirstOrDefault();
                    
                    var unreadCount = conv.Messages
                        .Count(m => !m.IsDeleted && m.SenderId != currentUserId && m.ReadAt == null);

                    conversationDtos.Add(new ConversationDto
                    {
                        Id = conv.Id,
                        PostId = conv.PostId,
                        PostTitle = conv.Post.Title,
                        SellerId = conv.SellerId,
                        SellerName = conv.Seller.FullName,
                        BuyerId = conv.BuyerId,
                        BuyerName = conv.Buyer.FullName,
                        Status = conv.Status,
                        CreatedAt = conv.CreatedAt,
                        LastMessage = lastMessage != null ? new MessageDto
                        {
                            Id = lastMessage.Id,
                            ConversationId = lastMessage.ConversationId,
                            SenderId = lastMessage.SenderId,
                            Body = lastMessage.Body,
                            CreatedAt = lastMessage.CreatedAt,
                            ReadAt = lastMessage.ReadAt
                        } : null,
                        UnreadCount = unreadCount
                    });
                }

                return conversationDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting conversations");
                throw;
            }
        }

        public async Task<int> GetUnreadCountAsync()
        {
            try
            {
                var currentUserId = _claimsService.GetCurrentUserId;

                var conversations = await _unitOfWork.Conversations.GetAllAsync(
                    predicate: c => (c.BuyerId == currentUserId || c.SellerId == currentUserId) && !c.IsDeleted,
                    c => c.Messages
                );

                var totalUnread = 0;

                foreach (var conv in conversations)
                {
                    totalUnread += conv.Messages.Count(m => !m.IsDeleted && m.SenderId != currentUserId && m.ReadAt == null);
                }

                return totalUnread;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread count");
                throw;
            }
        }

        public async Task<bool> MarkMessagesAsReadAsync(Guid conversationId)
        {
            try
            {
                var currentUserId = _claimsService.GetCurrentUserId;

                var conversation = await _unitOfWork.Conversations.GetByIdAsync(conversationId);

                if (conversation == null || conversation.IsDeleted)
                {
                    return false;
                }

                var unreadMessages = await _unitOfWork.Messages.GetAllAsync(
                    predicate: m => m.ConversationId == conversationId &&
                                    m.SenderId != currentUserId &&
                                    m.ReadAt == null &&
                                    !m.IsDeleted
                );

                foreach (var message in unreadMessages)
                {
                    message.ReadAt = DateTime.UtcNow;
                    await _unitOfWork.Messages.Update(message);
                }

                await _unitOfWork.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error marking messages as read for conversation {conversationId}");
                throw;
            }
        }

        public async Task<MessageDto?> SendMessageAsync(SendMessageDto dto)
        {
            try
            {
                var currentUserId = _claimsService.GetCurrentUserId;

                var conversation = await _unitOfWork.Conversations.GetByIdAsync(dto.ConversationId);

                if (conversation == null || conversation.IsDeleted)
                {
                    _logger.LogWarning($"Conversation {dto.ConversationId} not found");
                    return null;
                }

                // Verify user is part of conversation
                if (conversation.BuyerId != currentUserId && conversation.SellerId != currentUserId)
                {
                    _logger.LogWarning($"User {currentUserId} not authorized for conversation {dto.ConversationId}");
                    return null;
                }

                var message = new Message
                {
                    ConversationId = dto.ConversationId,
                    SenderId = currentUserId,
                    Body = dto.Body
                };

                await _unitOfWork.Messages.AddAsync(message);

                // Update conversation timestamp
                conversation.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.Conversations.Update(conversation);

                await _unitOfWork.SaveChangesAsync();

                var sender = await _unitOfWork.Users.GetByIdAsync(currentUserId);

                return new MessageDto
                {
                    Id = message.Id,
                    ConversationId = message.ConversationId,
                    SenderId = message.SenderId,
                    SenderName = sender?.FullName ?? "Unknown",
                    Body = message.Body,
                    ReadAt = message.ReadAt,
                    CreatedAt = message.CreatedAt,
                    IsCurrentUser = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message");
                throw;
            }
        }
    }
}
