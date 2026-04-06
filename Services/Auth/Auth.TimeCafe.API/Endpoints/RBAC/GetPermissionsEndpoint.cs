using Auth.TimeCafe.Application.CQRS.RBAC.Query;

namespace Auth.TimeCafe.API.Endpoints.RBAC;

public class GetPermissionsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGroup("/rbac")
            .WithTags("RBAC")
            .RequireAuthorization()
            .MapGet("/permissions", async (
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var result = await sender.Send(new GetPermissionsQuery(), ct);

                return result.ToHttpResult(permissions => Results.Ok(new { permissions }));
            })
            .WithName("GetPermissions")
            .WithSummary("Получение списка разрешений")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .WithDescription("Возвращает список всех доступных разрешений для RBAC.");
    }
}
