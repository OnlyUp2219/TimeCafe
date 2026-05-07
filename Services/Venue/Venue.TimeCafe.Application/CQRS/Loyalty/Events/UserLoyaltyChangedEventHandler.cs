namespace Venue.TimeCafe.Application.CQRS.Loyalty.Events;

public class UserLoyaltyChangedEventHandler(HybridCache cache) : INotificationHandler<UserLoyaltyChangedEvent>
{
    public Task Handle(UserLoyaltyChangedEvent notification, CancellationToken cancellationToken = default) => Task.WhenAll(
        cache.RemoveByTagAsync(CacheTags.UserLoyalties, cancellationToken).AsTask(),
        cache.RemoveByTagAsync(CacheTags.UserLoyalty(notification.UserId), cancellationToken).AsTask()
    );
}
