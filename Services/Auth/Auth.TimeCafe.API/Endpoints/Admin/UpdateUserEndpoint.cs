using Auth.TimeCafe.Application.CQRS.Admin.Command;

namespace Auth.TimeCafe.API.Endpoints.Admin;

public sealed record UpdateUserRequest(
    string? Email,
    string? UserName,
    bool? EmailConfirmed,
    bool? LockoutEnabled,
    DateTimeOffset? LockoutEnd);

public class UpdateUserEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGroup("/admin")
            .WithTags("Admin")
            .MapPut("/users/{userId:guid}", async (
                Guid userId,
                [FromBody] UpdateUserRequest request,
                [FromServices] ISender sender) =>
            {
                var command = new UpdateUserCommand(
                    userId,
                    request.Email,
                    request.UserName,
                    request.EmailConfirmed,
                    request.LockoutEnabled,
                    request.LockoutEnd);

                var result = await sender.Send(command);
                return result.ToHttpResult(() => Results.Ok(new { message = "Пользователь обновлён" }));
            })
            .WithName("UpdateUser")
            .WithSummary("Обновление пользователя (admin)")
            .WithDescription("Обновляет данные пользователя: email, username, lockout, подтверждение email.")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization(policy => policy.RequirePermissions(Permissions.AccountAdminRead));
    }
}
