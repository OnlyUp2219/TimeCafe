namespace Billing.TimeCafe.Application.CQRS.Transactions.Events;

public sealed class TransactionChangedEventHandler(HybridCache cache) : INotificationHandler<TransactionChangedEvent>
{
    private readonly HybridCache _cache = cache;

    public async Task Handle(TransactionChangedEvent notification, CancellationToken cancellationToken = default)
    {
        await Task.WhenAll(
            _cache.RemoveByTagAsync(CacheTags.Transactions, cancellationToken).AsTask(),
            _cache.RemoveByTagAsync(CacheTags.Transaction(notification.TransactionId), cancellationToken).AsTask(),
            _cache.RemoveByTagAsync(CacheTags.TransactionByUser(notification.UserId), cancellationToken).AsTask(),
            _cache.RemoveByTagAsync(CacheTags.Balances, cancellationToken).AsTask(),
            _cache.RemoveByTagAsync(CacheTags.Balance(notification.UserId), cancellationToken).AsTask()
        );
    }
}
