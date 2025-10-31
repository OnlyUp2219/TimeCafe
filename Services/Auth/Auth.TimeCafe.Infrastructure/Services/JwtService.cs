namespace Auth.TimeCafe.Infrastructure.Services;

public class JwtService(
    IConfiguration configuration,
    IRefreshTokenRepository refreshTokenRepository,
    IUserRoleService userRoleService) : IJwtService
{
    private readonly IConfiguration _configuration = configuration;
    private readonly IRefreshTokenRepository _refreshTokenRepository = refreshTokenRepository;
    private readonly IUserRoleService _userRoleService = userRoleService;

    public async Task<AuthResponse> GenerateTokens(IdentityUser user)
    {
        var jwtSection = _configuration.GetSection("Jwt");
        var keyBytes = Encoding.UTF8.GetBytes(jwtSection["SigningKey"]!);
        var expires = DateTime.UtcNow.AddMinutes(int.Parse(jwtSection["AccessTokenExpirationMinutes"]!));

        var roles = await _userRoleService.GetUserRolesAsync(user);
        var userRole = roles.FirstOrDefault() ?? "client";

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Role, userRole)
        };

        var token = new JwtSecurityToken(
            issuer: jwtSection["Issuer"],
            audience: jwtSection["Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256)
        );

        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var tokenEntity = new RefreshToken
        {
            Token = refreshToken,
            UserId = user.Id,
            Expires = DateTime.UtcNow.AddDays(30),
            Created = DateTime.UtcNow
        };

        await _refreshTokenRepository.AddAsync(tokenEntity);
        await _refreshTokenRepository.SaveChangesAsync();

        return new AuthResponse(
            AccessToken: new JwtSecurityTokenHandler().WriteToken(token),
            RefreshToken: refreshToken,
            Role: userRole,
            ExpiresIn: (int)(expires - DateTime.UtcNow).TotalSeconds
        );
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var jwtSection = _configuration.GetSection("Jwt");
        var keyBytes = Encoding.UTF8.GetBytes(jwtSection["SigningKey"]!);

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
            ValidateLifetime = false
        };

        var handler = new JwtSecurityTokenHandler();
        var principal = handler.ValidateToken(token, tokenValidationParameters, out var securityToken);

        if (securityToken is not JwtSecurityToken jwt ||
            !jwt.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            throw new SecurityTokenException("Invalid token");

        return principal;
    }

    public async Task<AuthResponse?> RefreshTokens(string refreshToken)
    {
        var tokenEntity = await _refreshTokenRepository.GetActiveTokenAsync(refreshToken);

        if (tokenEntity == null)
            return null;

        tokenEntity.IsRevoked = true;
        await _refreshTokenRepository.UpdateAsync(tokenEntity);

        var newTokens = await GenerateTokens(tokenEntity.User);
        await _refreshTokenRepository.SaveChangesAsync();

        return newTokens;
    }

}
