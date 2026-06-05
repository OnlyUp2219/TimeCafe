namespace Venue.TimeCafe.API.Endpoints.Visits.Commands;

public class RepublishBillingEvent : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/visits/{visitId:guid}/republish-billing", async (
            [FromServices] ISender sender,
            Guid visitId) =>
        {
            var command = new RepublishBillingEventCommand(visitId);
            var result = await sender.Send(command);
            return result.ToHttpResult(onSuccess: () => TypedResults.Ok(new { message = "Событие для генерации инвойса успешно отправлено" }));
        })
        .WithTags("Visits")
        .WithName("RepublishBillingEvent")
        .WithSummary("Переотправить событие для биллинга")
        .WithDescription("Администратор может принудительно переотправить событие для генерации счета, если оно потерялось.")
        .Produces(200)
        .Produces(404)
        .Produces(400)
        .RequireAuthorization(policy => policy.RequirePermissions(Permissions.VenueVisitUpdate));
    }
}
