namespace Auth.TimeCafe.API.Endpoints.Authentication;

public record class LogoutRequest(string RefreshToken);

public class Logout : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/logout", async (
            [FromBody] LogoutRequest request,
            ISender sender) =>
        {
            var command = new LogoutCommand(request.RefreshToken);
            var result = await sender.Send(command);

            return result.ToHttpResultV2(
                onSuccess: r => {
                    return Results.Ok(new { message = r.Message, revoked = r.Revoked });
                });
        })
            .WithTags("Authentication")
            .WithName("Logout");
    }
}


