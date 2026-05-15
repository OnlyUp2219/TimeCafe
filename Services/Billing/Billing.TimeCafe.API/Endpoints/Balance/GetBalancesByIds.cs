namespace Billing.TimeCafe.API.Endpoints.Balance;

public sealed class GetBalancesByIds : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/balance/batch", async (
            [FromServices] ISender sender,
            [FromBody] List<Guid> userIds) =>
        {
            var query = new GetBalancesByIdsQuery(userIds);
            var result = await sender.Send(query);

            return result.ToHttpResult(onSuccess: r => TypedResults.Ok(new { balances = r.Balances }));
        })
        .WithTags("Balance")
        .WithName("GetBalancesByIds")
        .WithSummary("Получить список балансов по массиву UserId")
        .WithDescription("Возвращает список балансов для указанных идентификаторов пользователей.")
        .Produces(200)
        .RequireAuthorization(policy => policy.RequirePermissions(Permissions.BillingBalanceRead));
    }
}
