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
            return result.ToHttpResult(onSuccess: r => Results.Ok(new { message = r.Message }));
        })
        .WithTags("Tariffs")
        .WithName("DeactivateTariff")
        .WithSummary("Деактивировать тариф")
        .WithDescription("Деактивирует тариф по идентификатору.")
        .Produces(200)
        .Produces(404)
        .RequireAuthorization(policy => policy.RequirePermissions(Permissions.VenueTariffDeactivate));
    }
}

