namespace UserProfile.TimeCafe.Domain.Events;

public record UserRegistered(string UserId, string Email, DateTime RegisteredAt);
