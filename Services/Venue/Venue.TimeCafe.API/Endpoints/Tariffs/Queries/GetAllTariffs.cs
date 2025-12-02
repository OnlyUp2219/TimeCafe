namespace Venue.TimeCafe.API.Endpoints.Tariffs.Queries;

public class GetAllTariffs : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/tariffs", async (ISender sender) =>
        {
            var query = new GetAllTariffsQuery();
            var result = await sender.Send(query);
            return result.ToHttpResultV2(onSuccess: r => Results.Ok(new { tariffs = r.Tariffs }));
        })
        .WithTags("Tariffs")
        .WithName("GetAllTariffs")
        .WithSummary("Получить все тарифы")
        .WithDescription("Возвращает список всех тарифов.");
    }
}
