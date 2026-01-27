namespace Auth.TimeCafe.API.Endpoints.AccountManagement;

public class ResetPassword : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("").WithTags("ResetPassword");

        group.MapPost("/resetPassword", async (
            [FromBody] ResetPasswordRequest request,
            [FromServices] ISender sender) =>
        {
            var command = new ResetPasswordCommand(request.Email, request.ResetCode, request.NewPassword);
            var result = await sender.Send(command);

            return result.ToHttpResultV2(onSuccess: _ => Results.Ok(new { message = result.Message }));
        })
        .WithName("ResetPassword")
        .WithSummary("Сброс пароля по коду из письма")
        .WithDescription("Принимает email, resetCode (Base64Url) и новый пароль. Сбрасывает пароль через Identity reset token.");

        group.MapPost("/forgot-password-link-mock", async (
            [FromBody] ResetPasswordEmailRequest request,
            [FromServices] ISender sender) =>
        {
            var command = new ForgotPasswordCommand(request.Email, SendEmail: false);
            var result = await sender.Send(command);

            return result.ToHttpResultV2(onSuccess: r =>
            {
                if (r.CallbackUrl != null)
                    return Results.Ok(new { callbackUrl = r.CallbackUrl });
                return Results.Ok(new { message = r.Message });
            });
        })
        .RequireRateLimiting("OneRequestPerInterval")
        .RequireRateLimiting("MaxRequestPerWindow")
        .WithName("ForgotPasswordMock")
        .WithSummary("Mock: получение ссылки для сброса пароля без отправки email")
        .WithDescription("Тестовый endpoint: возвращает ссылку для сброса пароля, не отправляя email. Используется для тестирования фронта и интеграций.");

        group.MapPost("/forgot-password-link", async (
            [FromBody] ResetPasswordEmailRequest request,
            [FromServices] ISender sender) =>
        {
            var command = new ForgotPasswordCommand(request.Email, SendEmail: true);
            var result = await sender.Send(command);

            return result.ToHttpResultV2(onSuccess: r =>
            {
                return Results.Ok(new { message = r.Message });
            });
        })
        .RequireRateLimiting("OneRequestPerInterval")
        .RequireRateLimiting("MaxRequestPerWindow")
        .WithName("ForgotPassword")
        .WithSummary("Отправка ссылки для сброса пароля на email")
        .WithDescription("Отправляет письмо со ссылкой для сброса пароля на указанный email. Используется при восстановлении доступа.");
    }
}
