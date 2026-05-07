namespace Venue.TimeCafe.Application.CQRS.Themes.Events;

public record ThemeChangedEvent(Guid ThemeId) : INotification;
