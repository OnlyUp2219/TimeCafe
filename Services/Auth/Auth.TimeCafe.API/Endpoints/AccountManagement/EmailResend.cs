namespace Auth.TimeCafe.API.Endpoints.AccountManagement;

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

            return result.ToHttpResultV2(onSuccess: r =>
            {
                return Results.Ok(new { message = r.Message });
            });
        })
        .RequireRateLimiting("OneRequestPerInterval")
        .RequireRateLimiting("MaxRequestPerWindow")
        .WithName("ResendConfirmation")
        .WithSummary("Повторная отправка письма для подтверждения email")
        .WithDescription("Отправляет повторно письмо для подтверждения email пользователя. Используется, если первое письмо не дошло или истёк срок действия ссылки.");


        group.MapPost("/resend-mock", async (
            [FromBody] ResendConfirmationRequest request,
            [FromServices] ISender sender) =>
        {
            var command = new ResendConfirmationCommand(request.Email, SendEmail: false);
            var result = await sender.Send(command);

            return result.ToHttpResultV2(onSuccess: r =>
            {
                return Results.Ok(new { callbackUrl = result.CallbackUrl });
            });
        })
        .RequireRateLimiting("OneRequestPerInterval")
        .RequireRateLimiting("MaxRequestPerWindow")
        .WithName("ResendConfirmationMock")
        .WithSummary("Mock: повторная отправка письма для подтверждения email")
        .WithDescription("Тестовый endpoint: возвращает callbackUrl для подтверждения email без реальной отправки письма.");
    }
}


