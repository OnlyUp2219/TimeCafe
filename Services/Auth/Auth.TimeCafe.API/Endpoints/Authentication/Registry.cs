namespace Auth.TimeCafe.API.Endpoints.Authentication;

public class CreateRegistry : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {


        app.MapPost("/refresh-token-jwt", async (
            IJwtService jwtService,
            HttpContext context,
            [FromBody] JwtRefreshRequest request) =>
        {
            var tokens = await jwtService.RefreshTokens(request.RefreshToken);
            if (tokens == null) return Results.Unauthorized();
            context.Response.Cookies.Append("Access-Token", tokens.AccessToken);

            return Results.Ok(new TokensDto(tokens.AccessToken, tokens.RefreshToken));
        })
            .WithTags("Authentication")
            .WithName("RefreshToken");

        app.MapGet("/protected-test",
        async (
            UserManager<IdentityUser> userManager,
            ClaimsPrincipal user) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Results.Unauthorized();
            var u = await userManager.FindByIdAsync(userId);
            return Results.Ok($"Protected OK. User: {u?.Email} ({userId})");
        })
            .RequireAuthorization()
            .WithTags("Authentication")
            .WithName("Test401");


        app.MapPost("/logout", async (
            [FromBody] LogoutRequest request,
            ApplicationDbContext db,
            UserManager<IdentityUser> userManager,
            ClaimsPrincipal principal) =>
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
                return Results.BadRequest(new { errors = new { refreshToken = "Refresh token is required" } });

            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var entity = await db.RefreshTokens
                .FirstOrDefaultAsync(t => t.Token == request.RefreshToken && !t.IsRevoked);

            if (entity != null)
            {
                if (userId != null && entity.UserId != userId)
                    return Results.Unauthorized();

                entity.IsRevoked = true;
                await db.SaveChangesAsync();
            }

            return Results.Ok(new { message = "Logged out", revoked = entity != null });
        })
            .RequireAuthorization()
            .WithTags("Authentication")
            .WithName("Logout");

    }
}

public record class LogoutRequest(string RefreshToken);
public record JwtRefreshRequest(string RefreshToken);
