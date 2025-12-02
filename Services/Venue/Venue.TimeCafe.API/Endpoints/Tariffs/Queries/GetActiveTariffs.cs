namespace Venue.TimeCafe.API.Endpoints.Tariffs.Queries;

public class GetActiveTariffs : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/tariffs/active", async (ISender sender) =>
        {
            var query = new GetActiveTariffsQuery();
            var result = await sender.Send(query);
            return result.ToHttpResultV2(onSuccess: r => Results.Ok(new { tariffs = r.Tariffs }));
        })
        .WithTags("Tariffs")
        .WithName("GetActiveTariffs")
        .WithSummary("Получить активные тарифы")
        .WithDescription("Возвращает список всех активных тарифов.");
    }
}
