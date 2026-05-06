namespace UserProfile.TimeCafe.Application.CQRS.AdditionalInfos.Events;

public record AdditionalInfoChangedEvent(Guid UserId) : INotification;
