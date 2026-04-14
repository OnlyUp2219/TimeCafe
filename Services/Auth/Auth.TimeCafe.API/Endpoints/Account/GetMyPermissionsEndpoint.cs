using BuildingBlocks.Permissions;

namespace Auth.TimeCafe.API.Endpoints.Account;

public class GetMyPermissionsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGroup("/account")
            .WithTags("Account")
            .MapGet("/my-permissions", (ClaimsPrincipal user) =>
            {
                var permissions = user
                    .FindAll(CustomClaimTypes.Permissions)
                    .Select(c => c.Value)
                    .Distinct(StringComparer.Ordinal)
                    .ToList();

                return Results.Ok(new { permissions });
            })
            .WithName("GetMyPermissions")
            .WithSummary("Получение разрешений текущего пользователя")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .RequireAuthorization()
            .WithDescription("Возвращает список permissions текущего авторизованного пользователя.");
    }
}
