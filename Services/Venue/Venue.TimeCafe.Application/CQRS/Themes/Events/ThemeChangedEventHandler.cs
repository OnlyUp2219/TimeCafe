namespace Venue.TimeCafe.Application.CQRS.Themes.Events;

public class ThemeChangedEventHandler(HybridCache cache) : INotificationHandler<ThemeChangedEvent>
{
    public Task Handle(ThemeChangedEvent notification, CancellationToken cancellationToken = default) => Task.WhenAll(
        cache.RemoveByTagAsync(CacheTags.Themes, cancellationToken).AsTask(),
        cache.RemoveByTagAsync(CacheTags.Theme(notification.ThemeId), cancellationToken).AsTask(),
        cache.RemoveByTagAsync(CacheTags.Tariffs, cancellationToken).AsTask() 
    );
}
