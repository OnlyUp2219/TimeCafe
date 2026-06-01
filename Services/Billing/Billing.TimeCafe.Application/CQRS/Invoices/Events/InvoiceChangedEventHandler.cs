namespace Billing.TimeCafe.Application.CQRS.Invoices.Events;

public class InvoiceChangedEventHandler(HybridCache cache) : INotificationHandler<InvoiceChangedEvent>
{
    private readonly HybridCache _cache = cache;

    public async Task Handle(InvoiceChangedEvent notification, CancellationToken cancellationToken)
    {
        var tasks = new List<Task>
        {
            _cache.RemoveByTagAsync(CacheTags.Invoices, cancellationToken).AsTask(),
            _cache.RemoveByTagAsync(CacheTags.Invoice(notification.InvoiceId), cancellationToken).AsTask()
        };

        if (notification.UserId.HasValue)
        {
            tasks.Add(_cache.RemoveByTagAsync(CacheTags.InvoiceByUser(notification.UserId.Value), cancellationToken).AsTask());
        }

        await Task.WhenAll(tasks);
    }
}
