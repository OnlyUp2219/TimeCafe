namespace UserProfile.TimeCafe.Application.CQRS.AdditionalInfos.Events;

public class AdditionalInfoChangedEventHandler(HybridCache cache) : INotificationHandler<AdditionalInfoChangedEvent>
{
    public Task Handle(AdditionalInfoChangedEvent notification, CancellationToken cancellationToken = default) => Task.WhenAll(
        cache.RemoveByTagAsync(CacheTags.AdditionalInfos, cancellationToken).AsTask(),
        cache.RemoveByTagAsync(CacheTags.AdditionalInfoByUser(notification.UserId), cancellationToken).AsTask()
    );
}
