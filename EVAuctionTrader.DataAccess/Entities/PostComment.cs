namespace EVAuctionTrader.DataAccess.Entities
{

    // Comment trong bài post
    public class PostComment : BaseEntity
    {
        public Guid PostId { get; set; }
        public Guid AuthorId { get; set; }
        public string Body { get; set; }
        
        // Nullable - nếu null thì là comment gốc, nếu có giá trị thì là reply
        public Guid? ParentCommentId { get; set; }

        public Post Post { get; set; }
        public User Author { get; set; }
        
        // Navigation property cho comment cha
        public PostComment? ParentComment { get; set; }
        
        // Danh sách các reply (comment con)
        public ICollection<PostComment> Replies { get; set; }
    }
}
