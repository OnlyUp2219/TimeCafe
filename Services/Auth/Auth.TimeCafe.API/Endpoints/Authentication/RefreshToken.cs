namespace Auth.TimeCafe.API.Endpoints.Authentication;

public class RefreshToken : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/refresh-token-jwt", async (
        HttpContext context,
        [FromServices] ISender sender,
        [FromBody] JwtRefreshRequest request) =>
        {
            var command = new RefreshTokenCommand(request.RefreshToken);
            var result = await sender.Send(command);

            return result.ToHttpResultV2(r =>
            {
                context.Response.Cookies.Append("Access-Token", r.AccessToken!);
                return Results.Ok(new { accessToken = r.AccessToken, refreshToken = r.RefreshToken });
            });
        })
        .WithTags("Authentication")
        .WithName("RefreshToken")
        .WithSummary("Обновление JWT-токенов пользователя")
        .WithDescription("Обновляет access и refresh JWT-токены пользователя по переданному refresh-токену. Новый access-токен сохраняется в cookie, оба токена возвращаются в JSON.");
    }
}
