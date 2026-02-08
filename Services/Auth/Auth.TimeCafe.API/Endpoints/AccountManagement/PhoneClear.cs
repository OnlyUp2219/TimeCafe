namespace Auth.TimeCafe.API.Endpoints.AccountManagement;

public class PhoneClear : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/account")
            .WithTags("Authentication");

        group.MapDelete("/phone", async (
            [FromServices] ISender sender,
            ClaimsPrincipal user
        ) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Results.Unauthorized();

            var command = new ClearPhoneCommand(userId);
            var result = await sender.Send(command);

            return result.ToHttpResultV2(onSuccess: r =>
            {
                return Results.Ok(new { message = r.Message });
            });
        })
        .RequireAuthorization()
        .WithName("ClearPhone")
        .WithSummary("Удалить номер телефона")
        .WithDescription("Удаляет номер телефона и сбрасывает флаг подтверждения для текущего пользователя.");
    }
}
