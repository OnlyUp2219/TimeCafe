using Audit.TimeCafe.Application.CQRS.AuditLogs.Queries;

namespace Audit.TimeCafe.API.Endpoints.AuditLogs;

public class GetAuditLogByIdEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/logs/{id:guid}", async (
                Guid id,
                [FromServices] ISender sender) =>
            {
                var query = new GetAuditLogByIdQuery(id);
                var result = await sender.Send(query);

                return result.ToHttpResult(data => Results.Ok(data));
            })
            .WithTags("Audit")
            .WithName("GetAuditLogById")
            .WithSummary("Получение audit лога по ID")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization(policy => policy.RequirePermissions(Permissions.AuditLogAdminRead))
            .WithDescription("Возвращает детали audit лога по его идентификатору.");
    }
}
