namespace Auth.TimeCafe.API.Endpoints.AccountManagement.Phone;

public record SavePhoneRequest(
    /// <example>+79001234567</example>
    string PhoneNumber);

public class PhoneSave : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/account")
            .WithTags("Authentication");

        group.MapPost("/phone", async (
            [FromServices] ISender sender,
            ClaimsPrincipal user,
            [FromBody] SavePhoneRequest model
        ) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Results.Unauthorized();

            var command = new SavePhoneCommand(userId, model.PhoneNumber);
            var result = await sender.Send(command);

            return result.ToHttpResult(onSuccess: r => Results.Ok(new { message = r.Message, phoneNumber = r.PhoneNumber }));
        })
        .RequireAuthorization()
        .WithName("SavePhone")
        .WithSummary("Сохранить номер телефона")
        .Produces(200)
        .WithDescription("Сохраняет неподтвержденный номер телефона для текущего пользователя.");
    }
}
