using Auth.TimeCafe.Application.CQRS.Auth.Commands;

namespace Auth.TimeCafe.API.Endpoints;

public class EmailConfirmation : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/email").WithTags("EmailConfirmation");

        group.MapPost("/confirm", async ([FromBody] ConfirmEmailRequest request, ISender sender) =>
        {
            var command = new ConfirmEmailCommand(request.UserId, request.Token);
            var result = await sender.Send(command);
            if (!result.Success)
                return Results.BadRequest(new { errors = new { token = result.Error } });
            return Results.Ok(new { message = result.Message });
        })
        .WithName("ConfirmEmail");

        group.MapPost("/resend", async ([FromBody] ResendConfirmationRequest request, ISender sender) =>
        {
            var command = new ResendConfirmationCommand(request.Email, SendEmail: true);
            var result = await sender.Send(command);
            if (!result.Success)
                return Results.BadRequest(new { errors = new { email = result.Error } });
            return Results.Ok(new { message = result.Message });
        })
        .RequireRateLimiting("OneRequestPerInterval")
        .RequireRateLimiting("MaxRequestPerWindow")
        .WithName("ResendConfirmation");

        group.MapPost("/resend-mock", async ([FromBody] ResendConfirmationRequest request, ISender sender) =>
        {
            var command = new ResendConfirmationCommand(request.Email, SendEmail: false);
            var result = await sender.Send(command);
            if (!result.Success)
                return Results.BadRequest(new { errors = new { email = result.Error } });
            return Results.Ok(new { callbackUrl = result.CallbackUrl });
        })
        .RequireRateLimiting("OneRequestPerInterval")
        .RequireRateLimiting("MaxRequestPerWindow")
        .WithName("ResendConfirmationMock");
    }
}

public record ConfirmEmailRequest(string UserId, string Token);
public record ResendConfirmationRequest(string Email);
