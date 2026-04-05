using Auth.TimeCafe.Application.CQRS.RBAC.Query;
using FluentResults;

namespace Auth.TimeCafe.API.Endpoints.RBAC;

public class AddToRoleEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/rbac")
            .WithTags("RBAC")
            .RequireAuthorization();

        group.MapGet("/permissions", async (
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var query = new GetPermissionsQuery();
            var result = await sender.Send(query, ct);

            return ToHttpResult(result, permissions => Results.Ok(new { permissions }));
        })
        .WithName("GetPermissions")
        .WithSummary("Получение списка разрешений")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .WithDescription("Возвращает список всех доступных разрешений для RBAC.");

        group.MapGet("/permissions/{permission}", async (
            [FromServices] ISender sender,
            [FromRoute] string permission,
            CancellationToken ct) =>
        {
            var query = new GetPermissionQuery(permission);
            var result = await sender.Send(query, ct);

            return ToHttpResult(result, p => Results.Ok(new { permission = p }));
        })
        .WithName("GetPermission")
        .WithSummary("Получение конкретного разрешения")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .WithDescription("Возвращает одно разрешение по его имени.");

        group.MapGet("/roles", async (
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var query = new GetRolesQuery();
            var result = await sender.Send(query, ct);

            return ToHttpResult(result, roles => Results.Ok(new { roles }));
        })
        .WithName("GetRoles")
        .WithSummary("Получение списка ролей")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .WithDescription("Возвращает все доступные роли.");

        group.MapGet("/roles/{roleName}", async (
            [FromServices] ISender sender,
            [FromRoute] string roleName,
            CancellationToken ct) =>
        {
            var query = new GetRoleByNameQuery(roleName);
            var result = await sender.Send(query, ct);

            return ToHttpResult(result, role => Results.Ok(new { role }));
        })
        .WithName("GetRoleByName")
        .WithSummary("Получение роли по имени")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .WithDescription("Возвращает роль по её имени.");

        group.MapGet("/role-claims", async (
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var query = new GetRoleClaimsQuery();
            var result = await sender.Send(query, ct);

            return ToHttpResult(result, roleClaims => Results.Ok(new { roleClaims }));
        })
        .WithName("GetRoleClaims")
        .WithSummary("Получение ролей и их разрешений")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .WithDescription("Возвращает список ролей и связанных с ними разрешений.");

        group.MapGet("/role-claims/{roleName}", async (
            [FromServices] ISender sender,
            [FromRoute] string roleName,
            CancellationToken ct) =>
        {
            var query = new GetRoleClaimsByNameQuery(roleName);
            var result = await sender.Send(query, ct);

            return ToHttpResult(result, roleClaim => Results.Ok(new { roleClaim }));
        })
        .WithName("GetRoleClaimsByName")
        .WithSummary("Получение разрешений роли по имени")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .WithDescription("Возвращает список разрешений для указанной роли.");
    }

    private static IResult ToHttpResult<T>(Result<T> result, Func<T, IResult> onSuccess)
    {
        if (result.IsSuccess)
            return onSuccess(result.Value);

        var firstError = result.Errors.FirstOrDefault();
        var statusCode = StatusCodes.Status400BadRequest;

        if (firstError?.Metadata.TryGetValue("ErrorCode", out var rawStatusCode) == true
            && int.TryParse(rawStatusCode?.ToString(), out var parsedStatusCode))
        {
            statusCode = parsedStatusCode;
        }

        return Results.Json(new
        {
            message = firstError?.Message ?? "Произошла ошибка при обработке запроса",
            statusCode,
            errors = result.Errors.Select(e => new
            {
                message = e.Message,
                metadata = e.Metadata
            })
        }, statusCode: statusCode);
    }
}
