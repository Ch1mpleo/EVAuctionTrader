namespace EVAuctionTrader.DataAccess.Entities
{

    // Comment trong bài post
    public class PostComment : BaseEntity
    {
        public Guid PostId { get; set; }
        public Guid AuthorId { get; set; }
        public string Body { get; set; }

        public Post Post { get; set; }
        public User Author { get; set; }
    }
}
