namespace Auth.TimeCafe.API.Endpoints.AccountManagement;

public record class ResetPasswordEmailRequest(string Email);
public class ResetPassword : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("").WithTags("ResetPassword");

        group.MapPost("/forgot-password-link-mock", async (
            [FromBody] ResetPasswordEmailRequest request,
            ISender sender) =>
        {
            var command = new ForgotPasswordCommand(request.Email, SendEmail: false);
            var result = await sender.Send(command);

            if (!result.Success)
                return Results.BadRequest(new { errors = new { email = result.Error } });

            return Results.Ok(new { callbackUrl = result.CallbackUrl });
        })
        .RequireRateLimiting("OneRequestPerInterval")
        .RequireRateLimiting("MaxRequestPerWindow")
        .WithName("ForgotPasswordMock")
        .WithSummary("Mock: Возвращает ссылку для теста без отправки email");

        group.MapPost("/forgot-password-link", async (
            [FromBody] ResetPasswordEmailRequest request,
            ISender sender) =>
        {
            var command = new ForgotPasswordCommand(request.Email, SendEmail: true);
            var result = await sender.Send(command);

            if (!result.Success)
                return Results.BadRequest(new { errors = new { email = result.Error } });

            return Results.Ok(new { message = result.Message });
        })
        .RequireRateLimiting("OneRequestPerInterval")
        .RequireRateLimiting("MaxRequestPerWindow")
        .WithName("ForgotPassword")
        .WithSummary("Отправка ссылки для сброса пароля на email");
    }
}
