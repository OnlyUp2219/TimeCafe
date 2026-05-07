namespace Venue.TimeCafe.Application.CQRS.Tariffs.Events;

public class TariffChangedEventHandler(HybridCache cache) : INotificationHandler<TariffChangedEvent>
{
    public Task Handle(TariffChangedEvent notification, CancellationToken cancellationToken = default) => Task.WhenAll(
        cache.RemoveByTagAsync(CacheTags.Tariffs, cancellationToken).AsTask(),
        cache.RemoveByTagAsync(CacheTags.Tariff(notification.TariffId), cancellationToken).AsTask(),
        cache.RemoveByTagAsync(CacheTags.Visits, cancellationToken).AsTask()
    );
}
