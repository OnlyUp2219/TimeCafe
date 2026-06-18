namespace Venue.TimeCafe.API.Endpoints.Visits.Commands;

public record RequestEndVisitDto(bool PayFromBalance = false);

public class RequestEndVisit : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/visits/{visitId:guid}/request-end", async (
            [FromServices] ISender sender,
            Guid visitId,
            [FromBody(EmptyBodyBehavior = Microsoft.AspNetCore.Mvc.ModelBinding.EmptyBodyBehavior.Allow)] RequestEndVisitDto? request,
            ClaimsPrincipal principal) =>
        {
            var userId = principal.GetUserId();
            var pay = request?.PayFromBalance ?? false;
            var command = new RequestEndVisitCommand(visitId, userId, pay);
            var result = await sender.Send(command);
            return result.ToHttpResult(r => TypedResults.Ok(r));
        })
        .WithTags("Visits")
        .WithName("RequestEndVisit")
        .WithSummary("Запросить завершение")
        .WithDescription("Пользователь запрашивает завершение визита. Таймер продолжает идти.")
        .Produces(200)
        .Produces(404)
        .Produces(403)
        .Produces(409)
        .RequireAuthorization(policy => policy.RequirePermissions(Permissions.VenueVisitEnd));
    }
}

