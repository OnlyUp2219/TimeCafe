namespace Venue.TimeCafe.API.Endpoints.Tariffs.Commands;

public class ActivateTariff : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/tariffs/{tariffId:int}/activate", async (
            ISender sender,
            int tariffId) =>
        {
            var command = new ActivateTariffCommand(tariffId);
            var result = await sender.Send(command);
            return result.ToHttpResultV2(onSuccess: r => Results.Ok(new { message = r.Message }));
        })
        .WithTags("Tariffs")
        .WithName("ActivateTariff")
        .WithSummary("Активировать тариф")
        .WithDescription("Активирует тариф по идентификатору.");
    }
}
