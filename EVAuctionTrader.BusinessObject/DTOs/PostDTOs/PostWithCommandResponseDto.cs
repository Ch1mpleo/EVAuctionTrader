namespace EVAuctionTrader.BusinessObject.DTOs.PostDTOs;

public class PostWithCommentResponseDto : PostResponseDto
{
    public List<PostCommentResponseDto> Comments { get; set; } = new();
}
