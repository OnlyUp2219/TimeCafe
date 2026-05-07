namespace Venue.TimeCafe.Application.CQRS.Visits.Events;

public record VisitChangedEvent(Guid VisitId, Guid UserId) : INotification;
