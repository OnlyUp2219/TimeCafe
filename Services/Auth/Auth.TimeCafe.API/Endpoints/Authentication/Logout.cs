namespace Auth.TimeCafe.API.Endpoints.Authentication;

public record class LogoutRequest(string RefreshToken);

public class Logout : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/logout", async (
            [FromBody] LogoutRequest request,
            ISender sender) =>
        {
            var command = new LogoutCommand(request.RefreshToken);
            var result = await sender.Send(command);

            object extra = new { revoked = result.Revoked };
            return result.ToHttpResultV2(
                onSuccess: r =>
                {
                    return Results.Ok(new { message = r.Message, revoked = r.Revoked });
                }, extra);
        })
            .WithTags("Authentication")
            .WithName("Logout")
            .WithSummary("Выход пользователя и отзыв refresh-токена")
            .WithDescription("Выход пользователя из системы. Отзывает переданный refresh-токен, удаляет связанные сессии. Возвращает сообщение и статус отзыва.");
    }
}


