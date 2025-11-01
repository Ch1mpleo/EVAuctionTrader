using EVAuctionTrader.DataAccess.Interfaces;

namespace EVAuctionTrader.DataAccess.Commons
{
    public class CurrentTime : ICurrentTime
    {
        public DateTime GetCurrentTime()
        {
            return DateTime.UtcNow; // Đảm bảo trả về thời gian UTC
        }
    }
}
