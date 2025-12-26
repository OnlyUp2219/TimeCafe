namespace Venue.TimeCafe.API.Endpoints.Tariffs.Queries;

public class GetTariffsByBillingType : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/tariffs/billing-type/{billingType}", async (
            [FromServices] ISender sender,
            [AsParameters] GetTariffsByBillingTypeDto dto) =>
        {
            var query = new GetTariffsByBillingTypeQuery(dto.BillingType);
            var result = await sender.Send(query);
            return result.ToHttpResultV2(onSuccess: r => Results.Ok(new { tariffs = r.Tariffs }));
        })
        .WithTags("Tariffs")
        .WithName("GetTariffsByBillingType")
        .WithSummary("Получить тарифы по типу биллинга")
        .WithDescription("Возвращает список тарифов по типу биллинга.")
        .RequireAuthorization();
    }
}