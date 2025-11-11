using EVAuctionTrader.BusinessObject.Enums;
using EVAuctionTrader.DataAccess.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EVAuctionTrader.Business.BackgroundServices;

public sealed class PostStatusUpdaterService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<PostStatusUpdaterService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(30);

    public PostStatusUpdaterService(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<PostStatusUpdaterService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Post Status Updater Service is starting.");

        await UpdatePostStatusesAsync(stoppingToken);

        var timer = new PeriodicTimer(_checkInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await timer.WaitForNextTickAsync(stoppingToken);
                await UpdatePostStatusesAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Post Status Updater Service is stopping.");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in Post Status Updater Service.");
            }
        }
    }

    private async Task UpdatePostStatusesAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var utcNow = DateTime.UtcNow;

            var postsToUpdate = await unitOfWork.Posts.GetAllAsync(
                predicate: p =>
                !p.IsDeleted &&
                ((p.PublishedAt <= utcNow && p.Status == PostStatus.Draft) ||
                (p.ExpiresAt <= utcNow && p.Status == PostStatus.Active))
            );

            if (!postsToUpdate.Any())
            {
                _logger.LogDebug("No posts require status updates at {Time}.", utcNow);
                return;
            }

            var draftToActiveCount = 0;
            var activeToClosedCount = 0;

            foreach (var post in postsToUpdate)
            {
                if (post.PublishedAt <= utcNow && post.Status == PostStatus.Draft)
                {
                    post.Status = PostStatus.Active;
                    draftToActiveCount++;
                    _logger.LogDebug("Post {PostId} status changed from Draft to Active.", post.Id);
                }

                if (post.ExpiresAt <= utcNow && post.Status == PostStatus.Active)
                {
                    post.Status = PostStatus.Closed;
                    activeToClosedCount++;
                    _logger.LogDebug("Post {PostId} status changed from Active to Closed.", post.Id);
                }
            }

            await unitOfWork.Posts.UpdateRange(postsToUpdate);
            await unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
            "Post status update completed: {DraftToActive} Draft→Active, {ActiveToClosed} Active→Closed, Total: {Total}",
             draftToActiveCount,
             activeToClosedCount,
             postsToUpdate.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update post statuses.");
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Post Status Updater Service is stopping.");
        return base.StopAsync(cancellationToken);
    }
}
