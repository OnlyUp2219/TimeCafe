namespace Auth.TimeCafe.API.Endpoints.AccountManagement;

public record ConfirmEmailRequest(
    /// <example>550e8400-e29b-41d4-a716-446655440000</example>
    string UserId,
    /// <example>Q2ZESjhOVGtMWUZ5...</example>
    string Token);

public class EmailConfirmation : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/email").WithTags("EmailConfirmation");

        group.MapPost("/confirm", async (
            [FromBody] ConfirmEmailRequest request,
            [FromServices] ISender sender) =>
        {
            var command = new ConfirmEmailCommand(request.UserId, request.Token);
            var result = await sender.Send(command);

            return result.ToHttpResult(onSuccess: r => Results.Ok(new { message = r.Message }));
        })
        .WithName("ConfirmEmail")
        .WithSummary("Подтверждение email пользователя")
        .Produces(200)
        .WithDescription("Позволяет подтвердить email пользователя по токену. Используется после регистрации или смены email. Возвращает сообщение об успехе или ошибке.");
    }
}


