namespace Venue.TimeCafe.Application.CQRS.Loyalty.Events;

public record UserLoyaltyChangedEvent(Guid UserId) : INotification;
