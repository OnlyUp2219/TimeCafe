namespace Auth.TimeCafe.API.DTOs;

public record CurrentUserResponse(
    Guid UserId,
    string Email,
    bool EmailConfirmed,
    string? PhoneNumber,
    bool PhoneNumberConfirmed
);
