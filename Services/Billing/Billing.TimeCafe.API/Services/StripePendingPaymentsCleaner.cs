using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Billing.TimeCafe.Infrastructure.Data;
using Billing.TimeCafe.Domain.Models;
using Billing.TimeCafe.Domain.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Billing.TimeCafe.API.Services;

public class StripePendingPaymentsCleaner(
    IServiceProvider serviceProvider,
    ILogger<StripePendingPaymentsCleaner> logger) : BackgroundService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly ILogger<StripePendingPaymentsCleaner> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("StripePendingPaymentsCleaner background service started.");

        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(15));

        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await CleanStalePaymentsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during stale payments cleanup.");
            }
        }
    }

    private async Task CleanStalePaymentsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var cache = scope.ServiceProvider.GetRequiredService<HybridCache>();

        var expirationTime = DateTimeOffset.UtcNow.AddHours(-24);

        var stalePayments = await dbContext.Payments
            .Where(p => p.Status == PaymentStatus.Pending && p.CreatedAt < expirationTime)
            .ToListAsync(cancellationToken);

        if (stalePayments.Count == 0)
        {
            return;
        }

        _logger.LogInformation("Found {Count} stale pending payments to cancel.", stalePayments.Count);

        foreach (var payment in stalePayments)
        {
            payment.MarkAsCancelled("Stripe session expired (cleanup job)");

            await cache.RemoveByTagAsync(CacheTags.Payment(payment.PaymentId), cancellationToken);
            if (payment.UserId.HasValue)
            {
                await cache.RemoveByTagAsync(CacheTags.PaymentByUser(payment.UserId.Value), cancellationToken);
            }
        }

        await cache.RemoveByTagAsync(CacheTags.Payments, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully cancelled {Count} stale payments.", stalePayments.Count);
    }
}
