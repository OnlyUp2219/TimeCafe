namespace Venue.TimeCafe.API.Endpoints.Tariffs.Queries;

public class GetTariffById : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/tariffs/{tariffId}", async (
            [FromServices] ISender sender,
            [FromRoute] string tariffId) =>
        {
            // TODO : DTO
            var query = new GetTariffByIdQuery(tariffId);
            var result = await sender.Send(query);
            return result.ToHttpResultV2(onSuccess: r => Results.Ok(new { tariff = r.Tariff }));
        })
        .WithTags("Tariffs")
        .WithName("GetTariffById")
        .WithSummary("Получить тариф по ID")
        .WithDescription("Возвращает тариф по идентификатору.");
    }
}
