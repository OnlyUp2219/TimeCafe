namespace Venue.TimeCafe.Application.CQRS.ResourceGroups.Events;

public class ResourceGroupChangedEventHandler(HybridCache cache) : INotificationHandler<ResourceGroupChangedEvent>
{
    private readonly HybridCache _cache = cache;

    public Task Handle(ResourceGroupChangedEvent notification, CancellationToken cancellationToken = default) => Task.WhenAll(
        _cache.RemoveByTagAsync(CacheTags.ResourceGroups, cancellationToken).AsTask(),
        _cache.RemoveByTagAsync(CacheTags.ResourceGroup(notification.ResourceGroupId), cancellationToken).AsTask(),
        _cache.RemoveByTagAsync(CacheTags.Resources, cancellationToken).AsTask()
    );
}
