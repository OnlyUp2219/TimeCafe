using Microsoft.Extensions.Caching.Hybrid;
using Venue.TimeCafe.Domain.Constants;

namespace Venue.TimeCafe.Application.CQRS.Promotions.Events;

public class PromotionChangedEventHandler(HybridCache cache) : INotificationHandler<PromotionChangedEvent>
{
    public Task Handle(PromotionChangedEvent notification, CancellationToken cancellationToken = default) => Task.WhenAll(
        cache.RemoveByTagAsync(CacheTags.Promotions, cancellationToken).AsTask(),
        cache.RemoveByTagAsync(CacheTags.Promotion(notification.PromotionId), cancellationToken).AsTask()
    );
}
