namespace Auth.TimeCafe.API.Endpoints.Authentication;

public class LoginV2 : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/login-jwt-v2", async (
            [FromServices] HttpContext context,
            [FromServices] ISender sender,
            [FromServices] IConfiguration configuration,
            [FromBody] LoginDto dto) =>
        {
            var command = new LoginUserCommand(dto.Email, dto.Password);
            var result = await sender.Send(command);

            return result.ToHttpResultV2(onSuccess: r =>
            {
                if (!r.EmailConfirmed)
                    return Results.Ok(new { emailConfirmed = r.EmailConfirmed });
                var refreshDaysStr = configuration.GetSection("Jwt")["RefreshTokenExpirationDays"] ?? "30";
                if (!int.TryParse(refreshDaysStr, out var refreshDays)) refreshDays = 30;
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
        .WithName("LoginV2")
        .WithSummary("Аутентификация пользователя через JWT (v2)")
        .WithDescription("Возвращает только access токен в JSON и устанавливает refresh токен в httpOnly cookie.");
    }
}