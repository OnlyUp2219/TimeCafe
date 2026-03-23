namespace Auth.TimeCafe.API.Endpoints.AccountManagement;

public record ResendConfirmationRequest(
    /// <example>user@example.com</example>
    string Email);

public class EmailResend : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/email").WithTags("EmailConfirmation");

        group.MapPost("/resend", async (
            [FromBody] ResendConfirmationRequest request,
            [FromServices] ISender sender) =>
        {
            var command = new ResendConfirmationCommand(request.Email, SendEmail: true);
            var result = await sender.Send(command);

            return result.ToHttpResultV2(onSuccess: r => Results.Ok(new { message = r.Message }));
        })
        .RequireRateLimiting("OneRequestPerInterval")
        .RequireRateLimiting("MaxRequestPerWindow")
        .WithName("ResendConfirmation")
        .WithSummary("Повторная отправка письма для подтверждения email")
        .Produces(200)
        .WithDescription("Отправляет повторно письмо для подтверждения email пользователя. Используется, если первое письмо не дошло или истёк срок действия ссылки.");


        group.MapPost("/resend-mock", async (
            [FromBody] ResendConfirmationRequest request,
            [FromServices] ISender sender) =>
        {
            var command = new ResendConfirmationCommand(request.Email, SendEmail: false);
            var result = await sender.Send(command);

            return result.ToHttpResultV2(onSuccess: r => Results.Ok(new { callbackUrl = r.CallbackUrl }));
        })
        .RequireRateLimiting("OneRequestPerInterval")
        .RequireRateLimiting("MaxRequestPerWindow")
        .WithName("ResendConfirmationMock")
        .WithSummary("Mock: повторная отправка письма для подтверждения email")
        .Produces(200)
        .WithDescription("Тестовый endpoint: возвращает callbackUrl для подтверждения email без реальной отправки письма.");
    }
}


