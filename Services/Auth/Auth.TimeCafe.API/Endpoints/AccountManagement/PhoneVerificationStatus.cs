using Auth.TimeCafe.Application.CQRS.Account.Queries;

namespace Auth.TimeCafe.API.Endpoints.AccountManagement;

public sealed class PhoneVerificationStatus : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGroup("/account")
            .WithTags("Authentication")
            .MapGet("/phone-verification-status", async (
                ClaimsPrincipal principal,
                [FromServices] ISender sender) =>
            {
                var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                    return Results.Unauthorized();

                var query = new GetPhoneVerificationStatusQuery(userId);
                var result = await sender.Send(query);

                return result.ToHttpResultV2(r => Results.Ok(new
                {
                    phoneNumber = r.PhoneNumber,
                    phoneNumberConfirmed = r.PhoneNumberConfirmed,
                    hasPendingVerification = r.HasPendingVerification
                }));
            })
            .RequireAuthorization()
            .WithName("GetPhoneVerificationStatus")
            .WithSummary("Статус верификации телефона")
            .Produces(200)
            .WithDescription("Возвращает текущий номер телефона, статус подтверждения и наличие незавершённой верификации (отправленный код).");
    }
}
