namespace Venue.TimeCafe.Application.CQRS.Visits.Events;

public class VisitChangedEventHandler(HybridCache cache) : INotificationHandler<VisitChangedEvent>
{
    public Task Handle(VisitChangedEvent notification, CancellationToken cancellationToken = default)
    {
        var tasks = new List<Task>
        {
            cache.RemoveByTagAsync(CacheTags.Visits, cancellationToken).AsTask(),
            cache.RemoveByTagAsync(CacheTags.Visit(notification.VisitId), cancellationToken).AsTask()
        };

        if (notification.UserId.HasValue)
        {
            tasks.Add(cache.RemoveByTagAsync(CacheTags.VisitByUser(notification.UserId.Value), cancellationToken).AsTask());
        }

        return Task.WhenAll(tasks);
    }
}
