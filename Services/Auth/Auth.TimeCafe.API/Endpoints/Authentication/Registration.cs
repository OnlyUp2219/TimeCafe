namespace Auth.TimeCafe.API.Endpoints.Authentication;

public class Registration : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/registerWithUsername", async (
            ISender sender,
            [FromBody] RegisterDto dto) =>
        {
            var command = new RegisterUserCommand(dto.Username, dto.Email, dto.Password, SendEmail: true);
            var result = await sender.Send(command);
            if (!result.Success)
                return Results.BadRequest(new { errors = result.Errors });
            return Results.Ok(new { message = result.Message });
        })
            .WithTags("Authentication")
            .WithName("Register")
            .WithDescription("Регистрация с логином");

        app.MapPost("/registerWithUsername-mock", async (
            ISender sender,
            [FromBody] RegisterDto dto) =>
        {
            var command = new RegisterUserCommand(dto.Username, dto.Email, dto.Password, SendEmail: false);
            var result = await sender.Send(command);
            if (!result.Success)
                return Results.BadRequest(new { errors = result.Errors });
            return Results.Ok(new { callbackUrl = result.CallbackUrl });
        })
            .WithTags("Authentication")
            .WithName("RegisterMock")
            .RequireRateLimiting("OneRequestPerInterval")
            .RequireRateLimiting("MaxRequestPerWindow")
            .WithDescription("Регистрация с логином");
    }
}
