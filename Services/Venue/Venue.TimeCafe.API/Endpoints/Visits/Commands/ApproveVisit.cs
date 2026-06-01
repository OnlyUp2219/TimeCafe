namespace Venue.TimeCafe.API.Endpoints.Visits.Commands;

public class ApproveVisit : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/visits/{visitId:guid}/approve", async (
            [FromServices] ISender sender,
            Guid visitId,
            ClaimsPrincipal principal) =>
        {
            var approvedByUserIdStr = principal.FindFirstValue("sub")
                ?? principal.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? principal.FindFirstValue("nameid");
            var approvedByUserId = Guid.Parse(approvedByUserIdStr!);
            var command = new ApproveVisitCommand(visitId, approvedByUserId);
            var result = await sender.Send(command);
            return result.ToHttpResult(r => TypedResults.Ok(r));
        })
        .WithTags("Visits")
        .WithName("ApproveVisit")
        .WithSummary("Подтвердить визит")
        .WithDescription("Подтверждает ожидающий визит и переводит его в активный статус.")
        .Produces(200)
        .Produces(404)
        .Produces(409)
        .RequireAuthorization(policy => policy.RequirePermissions(Permissions.VenueVisitApprove));
    }
}
