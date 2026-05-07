namespace Venue.TimeCafe.Application.CQRS.Visits.Events;

public class VisitChangedEventHandler(HybridCache cache) : INotificationHandler<VisitChangedEvent>
{
    public Task Handle(VisitChangedEvent notification, CancellationToken cancellationToken = default) => Task.WhenAll(
        cache.RemoveByTagAsync(CacheTags.Visits, cancellationToken).AsTask(),
        cache.RemoveByTagAsync(CacheTags.Visit(notification.VisitId), cancellationToken).AsTask(),
        cache.RemoveByTagAsync(CacheTags.VisitByUser(notification.UserId), cancellationToken).AsTask()
    );
}
