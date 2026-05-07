namespace Venue.TimeCafe.Application.CQRS.Promotions.Events;

public record PromotionChangedEvent(Guid PromotionId) : INotification;
