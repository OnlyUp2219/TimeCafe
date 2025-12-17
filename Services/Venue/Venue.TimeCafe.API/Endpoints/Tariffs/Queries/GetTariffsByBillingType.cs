namespace Venue.TimeCafe.API.Endpoints.Tariffs.Queries;

public class GetTariffsByBillingType : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/tariffs/billing-type/{billingType:int}", async (
            [FromServices] ISender sender,
            [FromRoute] int billingType) =>
        {
            // TODO : DTO
            var query = new GetTariffsByBillingTypeQuery((BillingType)billingType);
            var result = await sender.Send(query);
            return result.ToHttpResultV2(onSuccess: r => Results.Ok(new { tariffs = r.Tariffs }));
        })
        .WithTags("Tariffs")
        .WithName("GetTariffsByBillingType")
        .WithSummary("Получить тарифы по типу биллинга")
        .WithDescription("Возвращает список тарифов по типу биллинга.");
    }
}
