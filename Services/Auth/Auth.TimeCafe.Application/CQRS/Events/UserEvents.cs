namespace Auth.TimeCafe.Application.CQRS.Events;

public record UserChangedEvent(Guid UserId) : INotification;

public record UserRolesChangedEvent(Guid UserId, string RoleName) : INotification;

public record RoleClaimsChangedEvent(string RoleName) : INotification;
