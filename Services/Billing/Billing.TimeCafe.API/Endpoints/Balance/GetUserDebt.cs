
namespace Billing.TimeCafe.API.Endpoints.Balance;

public class GetUserDebt : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/debt/{userId:guid}", async (
            [FromServices] ISender sender,
            Guid userId) =>
        {
            var query = new GetUserDebtQuery(userId);
            var result = await sender.Send(query);
            return result.ToHttpResult(onSuccess: r => Results.Ok(new { debt = r.Debt }));
        })
        .WithTags("Balance")
        .WithName("GetUserDebt")
        .WithSummary("Получить долг пользователя")
        .WithDescription("Возвращает сумму задолженности пользователя.")
        .Produces(200)
        .RequireAuthorization();
    }
}
