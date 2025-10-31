namespace Auth.TimeCafe.Application.Contracts;

public interface IJwtService
{
    Task<AuthResponse> GenerateTokens(IdentityUser user);
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    Task<AuthResponse?> RefreshTokens(string refreshToken);
}
