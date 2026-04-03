namespace Venue.TimeCafe.API.Endpoints.Tariffs.Commands;

public class ActivateTariff : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/tariffs/{tariffId:guid}/activate", async (
            [FromServices] ISender sender,
            Guid tariffId) =>
        {
            var command = new ActivateTariffCommand(tariffId);
            var result = await sender.Send(command);
            return result.ToHttpResult(onSuccess: r => Results.Ok(new { message = r.Message }));
        })
        .WithTags("Tariffs")
        .WithName("ActivateTariff")
        .WithSummary("Активировать тариф")
        .WithDescription("Активирует тариф по идентификатору.")
        .Produces(200)
        .Produces(404)
        .RequireAuthorization();
    }
}

