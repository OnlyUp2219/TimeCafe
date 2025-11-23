namespace Auth.TimeCafe.API.Endpoints.Authentication;

public class RefreshTokenV2 : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/refresh-jwt-v2", async (
            HttpContext context,
            ISender sender,
            IConfiguration configuration) =>
        {
            if (!context.Request.Cookies.TryGetValue("refresh_token", out var refreshToken) || string.IsNullOrWhiteSpace(refreshToken))
                return Results.Unauthorized();

            var command = new RefreshTokenCommand(refreshToken);
            var result = await sender.Send(command);

            return result.ToHttpResultV2(r =>
            {
                var refreshDaysStr = configuration.GetSection("Jwt")["RefreshTokenExpirationDays"] ?? "30";
                if (!int.TryParse(refreshDaysStr, out var refreshDays)) refreshDays = 30;
                context.Response.Cookies.Append("refresh_token", r.RefreshToken!, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None, 
                    Path = "/",
                    Expires = DateTimeOffset.UtcNow.AddDays(refreshDays)
                });
                return Results.Ok(new { accessToken = r.AccessToken });
            });
        })
        .WithTags("Authentication")
        .WithName("RefreshTokenV2")
        .WithSummary("Обновление access токена через refresh cookie (v2)")
        .WithDescription("Читает refresh токен из httpOnly cookie, выполняет ротацию, возвращает новый access токен.");
    }
}