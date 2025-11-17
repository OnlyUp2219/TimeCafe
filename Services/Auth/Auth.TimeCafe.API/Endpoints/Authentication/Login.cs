namespace Auth.TimeCafe.API.Endpoints.Authentication;

public class Login : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/login-jwt", async (
        HttpContext context,
        ISender sender,
        [FromBody] LoginDto dto) =>
        {
            var command = new LoginUserCommand(dto.Email, dto.Password);
            var result = await sender.Send(command);

            return result.ToHttpResultV2(onSuccess: r =>
            {
                if (!r.EmailConfirmed)
                    return Results.Ok(new { emailConfirmed = r.EmailConfirmed });
                else
                {
                    context.Response.Cookies.Append("Access-Token", r.TokensDto!.AccessToken);
                    return Results.Ok(new { accessToken = r.TokensDto!.AccessToken, refreshToken = r.TokensDto!.RefreshToken, emailConfirmed = r.EmailConfirmed });
                }

            });


        })
        .WithTags("Authentication")
        .WithName("Login")
        .WithSummary("Аутентификация пользователя через JWT")
        .WithDescription("Вход пользователя по email и паролю. Возвращает access и refresh токены, а также статус подтверждения email. Access-токен сохраняется в cookie.");
    }
}
