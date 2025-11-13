namespace EVAuctionTrader.BusinessObject.DTOs.PostDTOs;

public class PostCommentRequestDto
{
    public Guid PostId { get; set; }
    public string Body { get; set; } = string.Empty;
    
    /// <summary>
    /// Nullable - n?u null th? là comment g?c, n?u có giá tr? th? là reply comment
    /// </summary>
    public Guid? ParentCommentId { get; set; }
}
