using Auth.TimeCafe.Application.CQRS.RBAC.Command;

namespace Auth.TimeCafe.API.Endpoints.RBAC;

public sealed record UpdateRoleNameRequest(string NewRoleName);

public class UpdateRoleNameEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGroup("/rbac")
            .WithTags("RBAC")
            .MapPut("/roles/{oldRoleName}/name", async (
                [FromServices] ISender sender,
                [FromRoute] string oldRoleName,
                [FromBody] UpdateRoleNameRequest request,
                CancellationToken ct) =>
            {
                var command = new UpdateRoleNameCommand(oldRoleName, request.NewRoleName);
                var result = await sender.Send(command, ct);

                return result.ToHttpResult(() =>
                    Results.Ok(new { message = "Имя роли успешно обновлено" }));
            })
            .WithName("UpdateRoleName")
            .WithSummary("Переименование роли")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status403Forbidden)
            .RequireAuthorization(policy => policy.RequirePermissions(Permissions.RbacRoleUpdate))
            .WithDescription("Изменяет имя существующей роли.");
    }
}
