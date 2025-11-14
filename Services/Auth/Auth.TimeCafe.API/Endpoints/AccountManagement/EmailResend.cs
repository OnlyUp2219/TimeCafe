namespace Auth.TimeCafe.API.Endpoints.AccountManagement;
public record ResendConfirmationRequest(string Email);

public class EmailResend : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/email").WithTags("EmailConfirmation");

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
        .WithName("ResendConfirmation")
        .WithDescription("Повторная отправка почты");


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
        .WithName("ResendConfirmationMock")
        .WithDescription("Повторная отправка почты, mock");
    }
}


