namespace Venue.TimeCafe.API.Endpoints.Visits.Commands;

public class SelfFixateVisitTime : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/visits/{visitId:guid}/self-fixate-time", async (
            [FromServices] ISender sender,
            ClaimsPrincipal user,
            Guid visitId) =>
        {
            var userId = user.GetUserId();
            var command = new SelfFixateVisitTimeCommand(visitId, userId);
            var result = await sender.Send(command);
            return result.ToHttpResult(r => TypedResults.Ok(r));
        })
        .WithTags("Visits")
        .WithName("SelfFixateVisitTime")
        .WithSummary("Самостоятельно зафиксировать время визита (Self-Checkout)")
        .WithDescription("Пользователь самостоятельно фиксирует время визита и переходит к оплате.")
        .Produces(200)
        .Produces(400)
        .Produces(403)
        .Produces(404)
        .RequireAuthorization()
        .RequireAuthorization(policy => policy.RequirePermissions(BuildingBlocks.Permissions.Permissions.VenueVisitSelfFixate));
    }
}
