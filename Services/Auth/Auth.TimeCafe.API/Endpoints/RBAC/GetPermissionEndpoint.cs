using Auth.TimeCafe.Application.CQRS.RBAC.Query;

namespace Auth.TimeCafe.API.Endpoints.RBAC;

public class GetPermissionEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGroup("/rbac")
            .WithTags("RBAC")
            .RequireAuthorization()
            .MapGet("/permissions/{permission}", async (
                [FromServices] ISender sender,
                [FromRoute] string permission,
                CancellationToken ct) =>
            {
                var result = await sender.Send(new GetPermissionQuery(permission), ct);

                return result.ToHttpResult(value => Results.Ok(new { permission = value }));
            })
            .WithName("GetPermission")
            .WithSummary("Получение разрешения по имени")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .WithDescription("Возвращает одно разрешение по его имени.");
    }
}
