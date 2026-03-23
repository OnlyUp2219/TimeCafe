namespace Venue.TimeCafe.API.Endpoints.Tariffs.Commands;

public class DeactivateTariff : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/tariffs/{tariffId:guid}/deactivate", async (
            [FromServices] ISender sender,
            Guid tariffId) =>
        {
            var command = new DeactivateTariffCommand(tariffId);
            var result = await sender.Send(command);
            return result.ToHttpResultV2(onSuccess: r => Results.Ok(new { message = r.Message }));
        })
        .WithTags("Tariffs")
        .WithName("DeactivateTariff")
        .WithSummary("Деактивировать тариф")
        .WithDescription("Деактивирует тариф по идентификатору.")
        .RequireAuthorization();
    }
}

