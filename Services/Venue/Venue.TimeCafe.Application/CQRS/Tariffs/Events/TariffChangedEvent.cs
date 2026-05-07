namespace Venue.TimeCafe.Application.CQRS.Tariffs.Events;

public record TariffChangedEvent(Guid TariffId) : INotification;
