using Auth.TimeCafe.Application.CQRS.RBAC.Command;

namespace Auth.TimeCafe.API.Endpoints.RBAC;

public sealed record CreateRoleRequest(string RoleName, List<string> Claims);

public class CreateRoleEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGroup("/rbac")
            .WithTags("RBAC")
            .RequireAuthorization()
            .MapPost("/roles", async (
                [FromServices] ISender sender,
                [FromBody] CreateRoleRequest request,
                CancellationToken ct) =>
            {
                var command = new CreateRoleClaimsCommand(request.RoleName, request.Claims);
                var result = await sender.Send(command, ct);

                return result.ToHttpResult(() =>
                    Results.Json(new { message = "Роль успешно создана" }, statusCode: StatusCodes.Status201Created));
            })
            .WithName("CreateRole")
            .WithSummary("Создание роли и назначение разрешений")
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status403Forbidden)
            .WithDescription("Создает новую роль и назначает ей список разрешений.");
    }
}
