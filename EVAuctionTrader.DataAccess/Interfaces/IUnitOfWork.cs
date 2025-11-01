namespace EVAuctionTrader.DataAccess.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {

        Task<int> SaveChangesAsync();
    }
}
