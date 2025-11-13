namespace Auth.TimeCafe.API.Endpoints.Authentication;

public record JwtRefreshRequest(string RefreshToken);

public class RefreshToken : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/refresh-token-jwt", async (
        HttpContext context,
        ISender sender,
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
        .WithDescription("Обновляет JWT‑токены. Новый access‑токен сохраняется в куке «Access‑Token», оба токена возвращаются в JSON.");
    }
}
