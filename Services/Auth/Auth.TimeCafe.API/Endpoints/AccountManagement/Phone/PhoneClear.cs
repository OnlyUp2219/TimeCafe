namespace Auth.TimeCafe.API.Endpoints.AccountManagement.Phone;

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
            var userId = user.TryGetUserId();
            if (userId == null)
                return Results.Unauthorized();

            var command = new ClearPhoneCommand(userId.Value);
            var result = await sender.Send(command);

            return result.ToHttpResult(onSuccess: () => Results.Ok(new { message = "Номер телефона удален" }));
        })
        .RequireAuthorization(policy => policy.RequirePermissions(Permissions.AccountPhoneClear))
        .WithName("ClearPhone")
        .WithSummary("Удалить номер телефона")
        .Produces(200)
        .WithDescription("Удаляет номер телефона и сбрасывает флаг подтверждения для текущего пользователя.");
    }
}
