namespace UserProfile.TimeCafe.Domain.Events;

public record UserRegistered(Guid UserId, string Email, DateTime RegisteredAt);
