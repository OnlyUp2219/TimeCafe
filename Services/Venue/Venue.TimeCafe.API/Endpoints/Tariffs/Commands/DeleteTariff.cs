namespace Venue.TimeCafe.API.Endpoints.Tariffs.Commands;

public class DeleteTariff : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/tariffs/{tariffId}", async (
            [FromServices] ISender sender,
            [AsParameters] DeleteTariffDto dto) =>
        {
            var command = new DeleteTariffCommand(dto.TariffId);
            var result = await sender.Send(command);
            return result.ToHttpResultV2(onSuccess: r => Results.Ok(new { message = r.Message }));
        })
        .WithTags("Tariffs")
        .WithName("DeleteTariff")
        .WithSummary("Удалить тариф")
        .WithDescription("Удаляет тариф по идентификатору.");
    }
}

