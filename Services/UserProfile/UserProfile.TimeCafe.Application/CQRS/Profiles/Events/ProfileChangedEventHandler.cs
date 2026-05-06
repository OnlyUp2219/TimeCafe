namespace UserProfile.TimeCafe.Application.CQRS.Profiles.Events;

public class ProfileChangedEventHandler(HybridCache cache) : INotificationHandler<ProfileChangedEvent>
{
    public Task Handle(ProfileChangedEvent notification, CancellationToken cancellationToken = default) => Task.WhenAll(
        cache.RemoveByTagAsync(CacheTags.Profiles, cancellationToken).AsTask(),
        cache.RemoveByTagAsync(CacheTags.Profile(notification.UserId), cancellationToken).AsTask()
    );
}
