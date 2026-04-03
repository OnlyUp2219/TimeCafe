namespace Auth.TimeCafe.API.Endpoints.AccountManagement.Users;

public record CurrentUserResponse(
    Guid UserId,
    string Email,
    bool EmailConfirmed,
    string? PhoneNumber,
    bool PhoneNumberConfirmed
);

public sealed class CurrentUser : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/account")
            .WithTags("Authentication");

        group.MapGet("/me", async (
            ClaimsPrincipal principal,
            [FromServices] UserManager<ApplicationUser> userManager) => await GetCurrentUserResponse(principal, userManager))
        .RequireAuthorization(policy => policy.RequirePermissions(Permissions.ClientRead))
        .WithName("GetCurrentUser")
        .WithSummary("Текущий пользователь")
        .Produces(200)
        .Produces(404)
        .WithDescription("Возвращает email и статус подтверждения email/телефона для текущего пользователя.");

        group.MapGet("/admin/me", async (
            ClaimsPrincipal principal,
            [FromServices] UserManager<ApplicationUser> userManager) => await GetCurrentUserResponse(principal, userManager))
        .RequireAuthorization(policy => policy.RequirePermissions(Permissions.Admin))
        .WithName("GetCurrentAdminUser")
        .WithSummary("Текущий администратор")
        .Produces(200)
        .Produces(404)
        .WithDescription("Возвращает данные текущего пользователя только при наличии админских прав.");
    }

    private static async Task<IResult> GetCurrentUserResponse(ClaimsPrincipal principal, UserManager<ApplicationUser> userManager)
    {
        var userId = principal.FindFirstValue("sub");
        if (userId == null)
            return Results.Unauthorized();

        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return Results.NotFound(new
            {
                errors = new[] { new { code = "UserNotFound", description = "Пользователь не найден" } }
            });
        }

        return Results.Ok(new CurrentUserResponse(
            user.Id,
            user.Email ?? string.Empty,
            user.EmailConfirmed,
            user.PhoneNumber,
            user.PhoneNumberConfirmed
        ));
    }
}
