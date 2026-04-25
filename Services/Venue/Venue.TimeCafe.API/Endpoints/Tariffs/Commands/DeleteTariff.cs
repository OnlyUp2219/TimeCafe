namespace Venue.TimeCafe.API.Endpoints.Tariffs.Commands;

public class DeleteTariff : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/tariffs/{tariffId:guid}", async (
            [FromServices] ISender sender,
            Guid tariffId) =>
        {
            var command = new DeleteTariffCommand(tariffId);
            var result = await sender.Send(command);
            return result.ToHttpResult(() => TypedResults.NoContent());
        })
        .WithTags("Tariffs")
        .WithName("DeleteTariff")
        .WithSummary("Удалить тариф")
        .WithDescription("Удаляет тариф по идентификатору.")
        .Produces(200)
        .Produces(404)
        .RequireAuthorization(policy => policy.RequirePermissions(Permissions.VenueTariffDelete));
    }
}


