namespace Venue.TimeCafe.Application.CQRS.Resources.Events;

public class ResourceChangedEventHandler(HybridCache cache) : INotificationHandler<ResourceChangedEvent>
{
    private readonly HybridCache _cache = cache;

    public Task Handle(ResourceChangedEvent notification, CancellationToken cancellationToken = default) => Task.WhenAll(
        _cache.RemoveByTagAsync(CacheTags.Resources, cancellationToken).AsTask(),
        _cache.RemoveByTagAsync(CacheTags.Resource(notification.ResourceId), cancellationToken).AsTask()
    );
}
