namespace Venue.TimeCafe.API.Endpoints.Visits.Commands;

public class DeleteVisit : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/visits/{visitId:int}", async (
            ISender sender,
            int visitId) =>
        {
            var command = new DeleteVisitCommand(visitId);
            var result = await sender.Send(command);
            return result.ToHttpResultV2(onSuccess: r => Results.Ok(new { message = r.Message }));
        })
        .WithTags("Visits")
        .WithName("DeleteVisit")
        .WithSummary("Удалить посещение")
        .WithDescription("Удаляет посещение по его идентификатору.");
    }
}
