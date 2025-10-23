namespace Auth.TimeCafe.Domain.Events;
public record UserRegisteredEvent(string UserId, string Email);