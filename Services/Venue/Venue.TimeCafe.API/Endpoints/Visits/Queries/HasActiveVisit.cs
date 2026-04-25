namespace Venue.TimeCafe.API.Endpoints.Visits.Queries;

public class HasActiveVisit : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/visits/has-active/{userId:guid}", async (
            [FromServices] ISender sender,
            Guid userId) =>
        {
            var query = new HasActiveVisitQuery(userId);
            var result = await sender.Send(query);
            return result.ToHttpResult(r => TypedResults.Ok(new { hasActiveVisit = r }));
        })
        .WithTags("Visits")
        .WithName("HasActiveVisit")
        .WithSummary("Проверить наличие активного посещения")
        .WithDescription("Проверяет, есть ли активное посещение у пользователя.")
        .Produces(200)
        .RequireAuthorization(policy => policy.RequirePermissions(Permissions.VenueVisitRead));
    }
}

