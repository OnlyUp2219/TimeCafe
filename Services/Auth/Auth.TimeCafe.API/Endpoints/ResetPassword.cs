using Auth.TimeCafe.Application.CQRS.Auth.Commands;

namespace Auth.TimeCafe.API.Endpoints;

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

            if (!result.IsSuccess)
                return Results.BadRequest(new { errors = result.Errors });

            return Results.Ok(new { callbackUrl = result.Data!.CallbackUrl, message = result.Data.Message });
        })
        .WithName("ForgotPasswordMock")
        .WithSummary("Mock: Возвращает ссылку для теста без отправки email");

        group.MapPost("/forgot-password-link", async (
            [FromBody] ResetPasswordEmailRequest request,
            ISender sender) =>
        {
            var command = new ForgotPasswordCommand(request.Email, SendEmail: true);
            var result = await sender.Send(command);

            if (!result.IsSuccess)
                return Results.BadRequest(new { errors = result.Errors });

            return Results.Ok(new { message = result.Data!.Message });
        })
        .WithName("ForgotPassword")
        .WithSummary("Отправка ссылки для сброса пароля на email");
    }
}
public record class ResetPasswordEmailRequest(string Email);