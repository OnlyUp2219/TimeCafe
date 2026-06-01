namespace Venue.TimeCafe.Application.CQRS.ResourceGroups.Events;

public record ResourceGroupChangedEvent(Guid ResourceGroupId) : INotification;
