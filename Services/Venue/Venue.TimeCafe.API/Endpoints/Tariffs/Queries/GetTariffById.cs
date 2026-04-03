namespace Venue.TimeCafe.API.Endpoints.Tariffs.Queries;

public class GetTariffById : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/tariffs/{tariffId:guid}", async (
            [FromServices] ISender sender,
            Guid tariffId) =>
        {
            var query = new GetTariffByIdQuery(tariffId);
            var result = await sender.Send(query);
            return result.ToHttpResult(onSuccess: r => Results.Ok(new { tariff = r.Tariff }));
        })
        .WithTags("Tariffs")
        .WithName("GetTariffById")
        .WithSummary("Получить тариф по ID")
        .WithDescription("Возвращает тариф по идентификатору.")
        .Produces(200)
        .Produces(404)
        .RequireAuthorization();
    }
}

