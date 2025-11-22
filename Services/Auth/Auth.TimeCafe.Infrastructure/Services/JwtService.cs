namespace Auth.TimeCafe.Infrastructure.Services;

public class JwtService(IConfiguration configuration, ApplicationDbContext context, IUserRoleService userRoleService) : IJwtService
{
    private readonly IConfiguration _configuration = configuration;
    private readonly ApplicationDbContext _context = context;
    private readonly IUserRoleService _userRoleService = userRoleService;

    public async Task<AuthResponse> GenerateTokens(IdentityUser user)
    {
        var jwtSection = _configuration.GetSection("Jwt");
        var keyBytes = Encoding.UTF8.GetBytes(jwtSection["SigningKey"]!);
        var expires = DateTimeOffset.UtcNow.AddMinutes(int.Parse(jwtSection["AccessTokenExpirationMinutes"]!));

        var roles = await _userRoleService.GetUserRolesAsync(user);
        var userRole = roles.FirstOrDefault() ?? "client";

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.Role, userRole)
        };

        var token = new JwtSecurityToken(
            issuer: jwtSection["Issuer"],
            audience: jwtSection["Audience"],
            claims: claims,
            expires: expires.UtcDateTime,
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256)
        );

        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var tokenEntity = new RefreshToken
        {
            Token = refreshToken,
            UserId = user.Id,
            Expires = DateTimeOffset.UtcNow.AddDays(30),
            Created = DateTimeOffset.UtcNow
        };

        _context.RefreshTokens.Add(tokenEntity);
        await _context.SaveChangesAsync();


        return new AuthResponse(
            AccessToken: new JwtSecurityTokenHandler().WriteToken(token),
            RefreshToken: refreshToken,
            Role: userRole,
            ExpiresIn: (int)(expires - DateTimeOffset.UtcNow).TotalSeconds
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
        var tokenEntity = await _context.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == refreshToken);

        // Token not found or expired
        if (tokenEntity == null || tokenEntity.Expires < DateTimeOffset.UtcNow)
            return null;

        // Suspicious activity: trying to reuse a revoked token (token theft detected)
        if (tokenEntity.IsRevoked)
        {
            // Revoke entire token family to protect user
            await RevokeFamilyTokensAsync(refreshToken);
            return null;
        }

        // Token rotation: generate new tokens first
        var newTokens = await GenerateTokens(tokenEntity.User);

        // Get the newly created token from database to link them
        var newTokenEntity = await _context.RefreshTokens
            .OrderByDescending(t => t.Created)
            .FirstAsync(t => t.UserId == tokenEntity.UserId && !t.IsRevoked);

        // Mark old token as revoked and link to new token
        tokenEntity.IsRevoked = true;
        tokenEntity.ReplacedByToken = newTokenEntity.Token;

        await _context.SaveChangesAsync();

        return newTokens;
    }

    public async Task<int> RevokeUserTokensAsync(string userId, CancellationToken cancellationToken = default)
    {
        var tokens = await _context.RefreshTokens.Where(t => t.UserId == userId && !t.IsRevoked).ToListAsync(cancellationToken);
        if (tokens.Count > 0)
        {
            foreach (var t in tokens)
                t.IsRevoked = true;
            await _context.SaveChangesAsync(cancellationToken);
        }
        return tokens.Count;
    }

    public async Task<bool> RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var tokenEntity = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.Token == refreshToken, cancellationToken);
        if (tokenEntity == null || tokenEntity.IsRevoked)
            return false;
        tokenEntity.IsRevoked = true;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task RevokeFamilyTokensAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var tokenEntity = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.Token == refreshToken, cancellationToken);
        if (tokenEntity == null)
            return;

        var rootToken = tokenEntity;
        while (rootToken != null)
        {
            var parentToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(t => t.ReplacedByToken == rootToken.Token, cancellationToken);
            if (parentToken == null)
                break;
            rootToken = parentToken;
        }

        var tokensToRevoke = new List<RefreshToken>();
        var currentToken = rootToken;

        while (currentToken != null)
        {
            if (!tokensToRevoke.Any(t => t.Id == currentToken.Id))
                tokensToRevoke.Add(currentToken);

            if (currentToken.ReplacedByToken != null)
            {
                currentToken = await _context.RefreshTokens
                    .FirstOrDefaultAsync(t => t.Token == currentToken.ReplacedByToken, cancellationToken);
            }
            else
            {
                break;
            }
        }

        foreach (var token in tokensToRevoke)
        {
            token.IsRevoked = true;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

}
