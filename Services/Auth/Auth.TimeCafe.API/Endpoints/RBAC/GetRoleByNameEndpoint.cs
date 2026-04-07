using Auth.TimeCafe.Application.CQRS.RBAC.Query;

namespace Auth.TimeCafe.API.Endpoints.RBAC;

public class GetRoleByNameEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGroup("/rbac")
            .WithTags("RBAC")
            .MapGet("/roles/{roleName}", async (
                [FromServices] ISender sender,
                [FromRoute] string roleName,
                CancellationToken ct) =>
            {
                var result = await sender.Send(new GetRoleByNameQuery(roleName), ct);

                return result.ToHttpResult(role => Results.Ok(new { role }));
            })
            .WithName("GetRoleByName")
            .WithSummary("Получение роли по имени")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization(policy => policy.RequirePermissions(Permissions.RbacRoleRead))
            .WithDescription("Возвращает роль по ее имени.");
    }
}
