
namespace Billing.TimeCafe.API.Endpoints.Balance;

public sealed class GetUserDebt : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/balance/{userId:guid}/debt", async (
            [FromServices] ISender sender,
            Guid userId) =>
        {
            var query = new GetUserDebtQuery(userId);
            var result = await sender.Send(query);
            return result.ToHttpResult(onSuccess: r => TypedResults.Ok(new { debt = r.Debt }));
        })
        .WithTags("Balance")
        .WithName("GetUserDebt")
        .WithSummary("Получить долг пользователя")
        .WithDescription("Возвращает текущую сумму долга пользователя.")
        .Produces(200)
        .RequireAuthorization(policy => policy.RequirePermissions(Permissions.BillingBalanceRead));
    }
}
