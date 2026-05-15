namespace Billing.TimeCafe.Application.CQRS.Balances.Events;

public sealed class BalanceChangedEventHandler(HybridCache cache) : INotificationHandler<BalanceChangedEvent>
{
    private readonly HybridCache _cache = cache;

    public async Task Handle(BalanceChangedEvent notification, CancellationToken cancellationToken = default)
    {
        await Task.WhenAll(
            _cache.RemoveByTagAsync(CacheTags.Balances, cancellationToken).AsTask(),
            _cache.RemoveByTagAsync(CacheTags.Balance(notification.UserId), cancellationToken).AsTask()
        );
    }
}
