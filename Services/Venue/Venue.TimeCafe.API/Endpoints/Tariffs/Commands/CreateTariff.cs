namespace Venue.TimeCafe.API.Endpoints.Tariffs.Commands;

public record CreateTariffRequest(
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

public class CreateTariff : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/tariffs", async (
            [FromServices] ISender sender,
            [FromBody] CreateTariffRequest request) =>
        {
            var command = new CreateTariffCommand(request.Name, request.Description, request.PricePerMinute, (BillingType)request.BillingType, request.ThemeId, request.IsActive);
            var result = await sender.Send(command);
            return result.ToHttpResult(r => TypedResults.Created($"/tariffs/{r.TariffId}", r));
        })
        .WithTags("Tariffs")
        .WithName("CreateTariff")
        .WithSummary("Создать тариф")
        .WithDescription("Создаёт новый тариф.")
        .Produces(201)
        .RequireAuthorization(policy => policy.RequirePermissions(Permissions.VenueTariffCreate));
    }
}

