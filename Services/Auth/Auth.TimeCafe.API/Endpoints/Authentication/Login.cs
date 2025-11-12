using BuildingBlocks.Extensions;

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

            return result.ToHttpResult(onSuccess: r =>
            {
                var luResult = (LoginUserResult)r;
#if DEBUG
                context.Response.Cookies.Append("Access-Token", luResult.TokensDto?.AccessToken ?? "");
#endif
                if (!luResult.EmailConfirmed ?? false)
                    return Results.Ok(new { emailConfirmed = false });
                else
                    return Results.Ok(new TokensDto(luResult.TokensDto?.AccessToken ?? "", luResult.TokensDto?.RefreshToken ?? ""));
            });


        })
        .WithTags("Authentication")
        .WithName("Login")
        .WithDescription("Вход с jwt-токеном");
    }
}
