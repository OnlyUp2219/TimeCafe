namespace Venue.TimeCafe.API.Endpoints.Visits.Queries;

public class HasActiveVisit : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/visits/has-active/{userId}", async (
            [FromServices] ISender sender,
            [AsParameters] HasActiveVisitDto userId) =>
        {
            var query = new HasActiveVisitQuery(userId);
            var result = await sender.Send(query);
            return result.ToHttpResultV2(onSuccess: r => Results.Ok(new { hasActiveVisit = r.HasActiveVisit }));
        })
        .WithTags("Visits")
        .WithName("HasActiveVisit")
        .WithSummary("Проверить наличие активного посещения")
        .WithDescription("Проверяет, есть ли активное посещение у пользователя.");
    }
}
