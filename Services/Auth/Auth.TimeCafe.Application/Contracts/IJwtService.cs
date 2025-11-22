namespace Auth.TimeCafe.Application.Contracts;

public interface IJwtService
{
    Task<AuthResponse> GenerateTokens(IdentityUser user);
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    Task<AuthResponse?> RefreshTokens(string refreshToken);
    Task<int> RevokeUserTokensAsync(string userId, CancellationToken cancellationToken = default);
    Task<bool> RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task RevokeFamilyTokensAsync(string refreshToken, CancellationToken cancellationToken = default);
}
