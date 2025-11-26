namespace Auth.TimeCafe.Application.DTOs;

public record AuthResponse(string AccessToken, string RefreshToken, string Role, int ExpiresIn);
