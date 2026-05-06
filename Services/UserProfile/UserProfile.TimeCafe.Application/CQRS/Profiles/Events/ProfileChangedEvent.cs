namespace UserProfile.TimeCafe.Application.CQRS.Profiles.Events;

public record ProfileChangedEvent(Guid UserId) : INotification;
