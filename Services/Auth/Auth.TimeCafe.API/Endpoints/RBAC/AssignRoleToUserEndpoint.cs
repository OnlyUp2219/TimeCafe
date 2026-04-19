using Auth.TimeCafe.Application.CQRS.RBAC.Command;

namespace Auth.TimeCafe.API.Endpoints.RBAC;

public class AssignRoleToUserEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGroup("/rbac")
            .WithTags("RBAC")
            .MapPost("/users/{userId:guid}/roles/{roleName}", async (
                [FromServices] ISender sender,
                [FromRoute] Guid userId,
                [FromRoute] string roleName,
                ClaimsPrincipal user,
                CancellationToken ct) =>
            {
                if (roleName.Equals(BuildingBlocks.Permissions.Roles.Admin, StringComparison.OrdinalIgnoreCase) ||
                    roleName.Equals(BuildingBlocks.Permissions.Roles.SuperAdmin, StringComparison.OrdinalIgnoreCase))
                {
                    if (!user.HasClaim(c => c.Type == CustomClaimTypes.Permissions && c.Value == Permissions.RbacSuperAdmin))
                    {
                        // TODO : формат возвращения не соответствует стандарту. Добавить в архитектуру
                        return Results.Forbid();
                    }
                }

                var command = new AssignRoleToUserCommand(userId, roleName);
                var result = await sender.Send(command, ct);

                return result.ToHttpResult(() => Results.Ok(new { message = "Роль успешно назначена пользователю" }));
            })
            .WithName("AssignRoleToUser")
            .WithSummary("Назначение роли пользователю")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization(policy => policy.RequirePermissions(Permissions.RbacUserRoleAssign))
            .WithDescription("Назначает пользователю роль по userId и roleName.");
    }
}
