namespace EVAuctionTrader.DataAccess.Entities
{
    public class Message : BaseEntity
    {
        public Guid ConversationId { get; set; }
        public Guid SenderId { get; set; }
        public string Body { get; set; }
        public DateTime? ReadAt { get; set; }

        public Conversation Conversation { get; set; }
        public User Sender { get; set; }
    }
}
