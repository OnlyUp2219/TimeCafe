using Auth.TimeCafe.Application.CQRS.RBAC.Command;

namespace Auth.TimeCafe.API.Endpoints.RBAC;

public class DeleteRoleEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGroup("/rbac")
            .WithTags("RBAC")
            .MapDelete("/roles/{roleName}", async (
                [FromServices] ISender sender,
                [FromRoute] string roleName,
                CancellationToken ct) =>
            {
                var command = new DeleteRoleCommand(roleName);
                var result = await sender.Send(command, ct);

                return result.ToHttpResult(() => Results.Ok(new { message = "Роль успешно удалена" }));
            })
            .WithName("DeleteRole")
            .WithSummary("Удаление роли")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status403Forbidden)
            .RequireAuthorization(policy => policy.RequirePermissions(Permissions.RbacRoleDelete))
            .WithDescription("Удаляет роль по имени.");
    }
}
