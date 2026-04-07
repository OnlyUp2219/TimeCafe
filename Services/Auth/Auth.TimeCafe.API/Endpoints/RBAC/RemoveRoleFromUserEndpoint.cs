using Auth.TimeCafe.Application.CQRS.RBAC.Command;

namespace Auth.TimeCafe.API.Endpoints.RBAC;

public class RemoveRoleFromUserEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGroup("/rbac")
            .WithTags("RBAC")
            .MapDelete("/users/{userId:guid}/roles/{roleName}", async (
                [FromServices] ISender sender,
                [FromRoute] Guid userId,
                [FromRoute] string roleName,
                CancellationToken ct) =>
            {
                var command = new RemoveRoleFromUserCommand(userId, roleName);
                var result = await sender.Send(command, ct);

                return result.ToHttpResult(() => Results.Ok(new { message = "Роль успешно снята с пользователя" }));
            })
            .WithName("RemoveRoleFromUser")
            .WithSummary("Снятие роли у пользователя")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization(policy => policy.RequirePermissions(Permissions.RbacUserRoleRemove))
            .WithDescription("Снимает у пользователя роль по userId и roleName.");
    }
}
