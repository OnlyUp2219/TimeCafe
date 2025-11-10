namespace Auth.TimeCafe.API.Endpoints.Authentication;

public class CreateRegistry : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
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
            .WithTags("Authentication")
            .WithName("Logout");

    }
}

public record class LogoutRequest(string RefreshToken);

