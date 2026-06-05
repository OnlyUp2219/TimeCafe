namespace Billing.TimeCafe.Application.CQRS.Payments.Events;

public sealed class PaymentChangedEventHandler(HybridCache cache) : INotificationHandler<PaymentChangedEvent>
{
    private readonly HybridCache _cache = cache;

    public async Task Handle(PaymentChangedEvent notification, CancellationToken cancellationToken = default)
    {
        var tasks = new List<Task>
        {
            _cache.RemoveByTagAsync(CacheTags.Payments, cancellationToken).AsTask(),
            _cache.RemoveByTagAsync(CacheTags.Payment(notification.PaymentId), cancellationToken).AsTask()
        };

        if (notification.UserId.HasValue)
        {
            tasks.Add(_cache.RemoveByTagAsync(CacheTags.PaymentByUser(notification.UserId.Value), cancellationToken).AsTask());
        }

        if (notification.TransactionId.HasValue && notification.TransactionId.Value != Guid.Empty)
        {
            tasks.Add(_cache.RemoveByTagAsync(CacheTags.Transactions, cancellationToken).AsTask());
            tasks.Add(_cache.RemoveByTagAsync(CacheTags.Transaction(notification.TransactionId.Value), cancellationToken).AsTask());
            
            if (notification.UserId.HasValue)
            {
                tasks.Add(_cache.RemoveByTagAsync(CacheTags.TransactionByUser(notification.UserId.Value), cancellationToken).AsTask());
                tasks.Add(_cache.RemoveByTagAsync(CacheTags.Balances, cancellationToken).AsTask());
                tasks.Add(_cache.RemoveByTagAsync(CacheTags.Balance(notification.UserId.Value), cancellationToken).AsTask());
            }
        }

        await Task.WhenAll(tasks);
    }
}
