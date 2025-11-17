namespace Auth.TimeCafe.API.Endpoints.AccountManagement;

public record ConfirmEmailRequest(string UserId, string Token);

public class EmailConfirmation : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/email").WithTags("EmailConfirmation");

        group.MapPost("/confirm", async ([FromBody] ConfirmEmailRequest request, ISender sender) =>
        {
            var command = new ConfirmEmailCommand(request.UserId, request.Token);
            var result = await sender.Send(command);

            return result.ToHttpResultV2(onSuccess: r =>
            {
                return Results.Ok(new { message = r.Message });
            });
        })
        .WithName("ConfirmEmail")
        .WithSummary("Подтверждение email пользователя")
        .WithDescription("Позволяет подтвердить email пользователя по токену. Используется после регистрации или смены email. Возвращает сообщение об успехе или ошибке.");
    }
}


