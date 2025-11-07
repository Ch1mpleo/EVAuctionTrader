using EVAuctionTrader.Business.Utils;
using EVAuctionTrader.BusinessObject.DTOs.PostDTOs;
using EVAuctionTrader.BusinessObject.Enums;

namespace EVAuctionTrader.Business.Interfaces
{
    public interface IPostService
    {
        Task<PostResponseDto?> CreatePostAsync(PostRequestDto createPostDto);
        Task<PostResponseDto?> UpdatePostAsync(Guid postId, PostRequestDto updatePostDto);
        Task<bool> UpdatePostStatusAsync(Guid postId, PostStatus newStatus);
        Task<Pagination<PostResponseDto>> GetAllPostsAsync(
            int pageNumber = 1,
            int pageSize = 10,
            string? search = null,
            PostType? postType = null,
            PostVersion? postVersion = null,
            PostStatus? postStatus = null,
            bool priceSort = true,
            decimal? minPrice = null,
            decimal? maxPrice = null);
        Task<Pagination<PostResponseDto>> GetAllMemberPostsAsync(
            int pageNumber = 1,
            int pageSize = 10,
            string? search = null,
            PostType? postType = null,
            PostVersion? postVersion = null,
            PostStatus? postStatus = null,
            bool priceSort = true);
        Task<PostWithCommentResponseDto?> GetPostByIdAsync(Guid postId);
        Task<bool> DeletePostAsync(Guid postId);

        Task<bool> BanPostAsync(Guid postId);
    }
}
