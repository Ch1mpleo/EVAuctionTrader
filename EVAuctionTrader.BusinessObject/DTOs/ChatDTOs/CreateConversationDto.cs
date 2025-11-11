using System.ComponentModel.DataAnnotations;

namespace EVAuctionTrader.BusinessObject.DTOs.ChatDTOs
{
    public class CreateConversationDto
    {
        [Required]
        public Guid PostId { get; set; }

        [Required]
        [MinLength(1)]
        [MaxLength(2000)]
        public string InitialMessage { get; set; } = string.Empty;
    }
}