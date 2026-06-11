using Audit.TimeCafe.Application.CQRS.AuditLogs.Queries;

namespace Audit.TimeCafe.API.Endpoints.AuditLogs;

public sealed record GetAuditLogsPageRequest(int Page, int PageSize, string? EventType, string? UserName, Guid? UserId = null);

public class GetAuditLogsPageEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/logs", async (
                [AsParameters] GetAuditLogsPageRequest request,
                [FromServices] ISender sender) =>
            {
                var query = new GetAuditLogsPageQuery(request.Page, request.PageSize, request.EventType, request.UserName, request.UserId);
                var result = await sender.Send(query);

                return result.ToHttpResult(data => Results.Ok(data));
            })
            .WithTags("Audit")
            .WithName("GetAuditLogsPage")
            .WithSummary("Получение страницы audit логов")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(policy => policy.RequirePermissions(Permissions.AuditLogAdminRead))
            .WithDescription("Возвращает страницу audit логов с фильтрами и пагинацией.");
    }
}
