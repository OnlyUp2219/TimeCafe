namespace Auth.TimeCafe.Application.DTO;

public record AuthResponse(string AccessToken, string RefreshToken, string Role, int ExpiresIn);
