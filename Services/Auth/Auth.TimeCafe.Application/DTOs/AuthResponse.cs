namespace Auth.TimeCafe.Application.DTOs;

public record AuthResponse(string AccessToken, string RefreshToken, List<string> Role, int ExpiresIn);
