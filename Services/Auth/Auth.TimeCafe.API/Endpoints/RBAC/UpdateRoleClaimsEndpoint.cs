using Auth.TimeCafe.Application.CQRS.RBAC.Command;

namespace Auth.TimeCafe.API.Endpoints.RBAC;

public sealed record UpdateRoleClaimsRequest(List<string> Claims);

public class UpdateRoleClaimsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGroup("/rbac")
            .WithTags("RBAC")
            .RequireAuthorization()
            .MapPut("/role-claims/{roleName}", async (
                [FromServices] ISender sender,
                [FromRoute] string roleName,
                [FromBody] UpdateRoleClaimsRequest request,
                CancellationToken ct) =>
            {
                var command = new UpdateRoleClaimsCommand(roleName, request.Claims);
                var result = await sender.Send(command, ct);

                return result.ToHttpResult(() => Results.Ok(new { message = "Разрешения роли успешно обновлены" }));
            })
            .WithName("UpdateRoleClaims")
            .WithSummary("Обновление разрешений роли")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status403Forbidden)
            .WithDescription("Полностью заменяет список разрешений указанной роли.");
    }
}
