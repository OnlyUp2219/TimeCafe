namespace Venue.TimeCafe.Application.CQRS.Resources.Events;

public record ResourceChangedEvent(Guid ResourceId) : INotification;
