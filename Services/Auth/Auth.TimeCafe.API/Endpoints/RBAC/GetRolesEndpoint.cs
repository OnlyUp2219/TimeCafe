using Auth.TimeCafe.Application.CQRS.RBAC.Query;

namespace Auth.TimeCafe.API.Endpoints.RBAC;

public class GetRolesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGroup("/rbac")
            .WithTags("RBAC")
            .MapGet("/roles", async (
                [FromServices] ISender sender,
                CancellationToken ct) =>
            {
                var result = await sender.Send(new GetRolesQuery(), ct);

                return result.ToHttpResult(roles => Results.Ok(new { roles }));
            })
            .WithName("GetRoles")
            .WithSummary("Получение списка ролей")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(policy => policy.RequirePermissions(Permissions.RbacRoleRead))
            .WithDescription("Возвращает все доступные роли.");
    }
}
