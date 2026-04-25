namespace Venue.TimeCafe.API.Endpoints.Tariffs.Commands;

public record UpdateTariffRequest(
    /// <example>Стандарт</example>
    string Name,
    /// <example>Базовый тариф для посещения</example>
    string? Description,
    /// <example>3.50</example>
    decimal PricePerMinute,
    /// <example>0</example>
    int BillingType,
    /// <example>a1b2c3d4-e5f6-7890-abcd-ef1234567890</example>
    Guid? ThemeId,
    /// <example>true</example>
    bool IsActive);

public class UpdateTariff : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/tariffs/{tariffId:guid}", async (
            [FromServices] ISender sender,
            Guid tariffId,
            [FromBody] UpdateTariffRequest request) =>
        {
            var command = new UpdateTariffCommand
            (
                TariffId: tariffId,
                Name: request.Name,
                Description: request.Description!,
                PricePerMinute: request.PricePerMinute,
                BillingType: (BillingType)request.BillingType,
                ThemeId: request.ThemeId,
                IsActive: request.IsActive
            );

            var result = await sender.Send(command);
            return result.ToHttpResult(r => TypedResults.Ok(r));
        })
        .WithTags("Tariffs")
        .WithName("UpdateTariff")
        .WithSummary("Обновить тариф")
        .WithDescription("Обновляет существующий тариф.")
        .Produces(200)
        .Produces(404)
        .RequireAuthorization(policy => policy.RequirePermissions(Permissions.VenueTariffUpdate));
    }
}

