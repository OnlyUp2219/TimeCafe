namespace Venue.TimeCafe.API.Endpoints.Tariffs.Queries;

public class GetActiveTariffs : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/tariffs/active", async (
            [FromServices] ISender sender) =>
        {
            var query = new GetActiveTariffsQuery();
            var result = await sender.Send(query);
            return result.ToHttpResult(r => TypedResults.Ok(r));
        })
        .WithTags("Tariffs")
        .WithName("GetActiveTariffs")
        .WithSummary("Получить активные тарифы")
        .WithDescription("Возвращает список всех активных тарифов.")
        .Produces(200)
        .RequireAuthorization(policy => policy.RequirePermissions(Permissions.VenueTariffRead));
    }
}

