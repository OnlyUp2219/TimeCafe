namespace Auth.TimeCafe.API.Endpoints.Authentication;

public record LoginRequest(
    /// <example>user@example.com</example>
    string Email,
    /// <example>P@ssw0rd123</example>
    string Password);

public class Login : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/login-jwt", async (
            HttpContext context,
            [FromServices] ISender sender,
            [FromServices] IConfiguration configuration,
            [FromBody] LoginRequest request) =>
        {
            var command = new LoginUserCommand(request.Email, request.Password);
            var result = await sender.Send(command);

            return result.ToHttpResult(onSuccess: r =>
            {
                if (!r.EmailConfirmed)
                    return Results.Ok(new { emailConfirmed = r.EmailConfirmed });
                var refreshDaysStr = configuration.GetSection("Jwt")["RefreshTokenExpirationDays"] ?? "30";
                if (!int.TryParse(refreshDaysStr, out var refreshDays))
                    refreshDays = 30;
                context.Response.Cookies.Append("refresh_token", r.TokensDto!.RefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Path = "/",
                    Expires = DateTimeOffset.UtcNow.AddDays(refreshDays)
                });
                return Results.Ok(new
                {
                    accessToken = r.TokensDto.AccessToken,
                    emailConfirmed = r.EmailConfirmed
                });
            });
        })
        .WithTags("Authentication")
        .WithName("Login")
        .WithSummary("Аутентификация пользователя через JWT ")
        .Produces(200)
        .WithDescription("Возвращает только access токен в JSON и устанавливает refresh токен в httpOnly cookie.");
    }
}
