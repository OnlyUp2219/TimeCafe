namespace Venue.TimeCafe.API.Endpoints.Visits.Commands;

public class CreateVisitRequest
{
    /// <example>550e8400-e29b-41d4-a716-446655440000</example>
    public Guid UserId { get; init; }
    
    /// <example>a1b2c3d4-e5f6-7890-abcd-ef1234567890</example>
    public Guid TariffId { get; init; }

    /// <example>a1b2c3d4-e5f6-7890-abcd-ef1234567890</example>
    public Guid? ResourceId { get; init; }
    
    /// <example>120</example>
    public int? PlannedMinutes { get; init; }
    
    /// <example>false</example>
    public bool? RequirePositiveBalance { get; init; }
    
    /// <example>false</example>
    public bool? RequireEnoughForPlanned { get; init; }
    
    /// <example>1</example>
    public int GuestsCount { get; init; } = 1;
}

public class CreateVisit : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/visits", async (
            [FromServices] ISender sender,
            [FromBody] CreateVisitRequest request) =>
        {
            var command = new CreateVisitCommand(
                UserId: request.UserId,
                TariffId: request.TariffId,
                PlannedMinutes: request.PlannedMinutes,
                GuestsCount: request.GuestsCount,
                RequirePositiveBalance: request.RequirePositiveBalance ?? true,
                RequireEnoughForPlanned: request.RequireEnoughForPlanned ?? false,
                ResourceId: request.ResourceId);
            var result = await sender.Send(command);
            return result.ToHttpResult(r => TypedResults.Created($"/visits/{r.VisitId}", r));
        })
        .WithTags("Visits")
        .WithName("CreateVisit")
        .WithSummary("Создать посещение")
        .WithDescription("Создаёт новое посещение пользователя по тарифу.")
        .Produces(201)
        .RequireAuthorization(policy => policy.RequirePermissions(Permissions.VenueVisitCreate));
    }
}

