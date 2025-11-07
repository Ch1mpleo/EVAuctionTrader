namespace EVAuctionTrader.BusinessObject.DTOs.PostDTOs;

public class PostCommentResponseDto
{
    public Guid Id { get; set; }
    public Guid PostId { get; set; }
    public Guid AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
