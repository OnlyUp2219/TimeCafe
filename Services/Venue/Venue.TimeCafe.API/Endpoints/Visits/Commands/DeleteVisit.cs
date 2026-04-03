namespace Venue.TimeCafe.API.Endpoints.Visits.Commands;

public class DeleteVisit : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/visits/{visitId:guid}", async (
            [FromServices] ISender sender,
            Guid visitId) =>
        {
            var command = new DeleteVisitCommand(visitId);
            var result = await sender.Send(command);
            return result.ToHttpResult(onSuccess: r => Results.Ok(new { message = r.Message }));
        })
        .WithTags("Visits")
        .WithName("DeleteVisit")
        .WithSummary("Удалить посещение")
        .WithDescription("Удаляет посещение по его идентификатору.")
        .Produces(200)
        .Produces(404)
        .RequireAuthorization();
    }
}
