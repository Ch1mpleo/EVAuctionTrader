using EVAuctionTrader.BusinessObject.Enums;
using EVAuctionTrader.DataAccess.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EVAuctionTrader.Business.BackgroundServices;

public sealed class PaymentCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<PaymentCleanupService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(5);

    public PaymentCleanupService(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<PaymentCleanupService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Payment Cleanup Service is starting.");

        await CleanupExpiredPaymentsAsync(stoppingToken);

        var timer = new PeriodicTimer(_checkInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await timer.WaitForNextTickAsync(stoppingToken);
                await CleanupExpiredPaymentsAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Payment Cleanup Service is stopping.");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred in Payment Cleanup Service.");
            }
        }
    }

    private async Task CleanupExpiredPaymentsAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var utcNow = DateTime.UtcNow;

            // Find all pending payments that have expired
            var expiredPayments = await unitOfWork.Payments.GetAllAsync(
                predicate: p =>
                p.Status == PaymentStatus.Pending &&
                p.ExpiresAt.HasValue &&
                p.ExpiresAt.Value <= utcNow
                );

            if (!expiredPayments.Any())
            {
                _logger.LogDebug("No expired pending payments found at {Time}.", utcNow);
                return;
            }

            var expiredCount = 0;

            foreach (var payment in expiredPayments)
            {
                payment.Status = PaymentStatus.Failed;
                expiredCount++;
                _logger.LogDebug("Payment {PaymentId} marked as Failed due to expiration.", payment.Id);
            }

            await unitOfWork.Payments.UpdateRange(expiredPayments);
            await unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                     "Payment cleanup completed: {ExpiredCount} expired payments marked as Failed.",
           expiredCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cleanup expired payments.");
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Payment Cleanup Service is stopping.");
        return base.StopAsync(cancellationToken);
    }
}
