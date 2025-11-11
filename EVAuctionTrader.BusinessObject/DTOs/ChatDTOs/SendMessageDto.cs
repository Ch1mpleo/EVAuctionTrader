using System.ComponentModel.DataAnnotations;

namespace EVAuctionTrader.BusinessObject.DTOs.ChatDTOs
{
    public class SendMessageDto
    {
        [Required]
        public Guid ConversationId { get; set; }

        [Required]
        [MinLength(1)]
        [MaxLength(2000)]
        public string Body { get; set; } = string.Empty;
    }
}