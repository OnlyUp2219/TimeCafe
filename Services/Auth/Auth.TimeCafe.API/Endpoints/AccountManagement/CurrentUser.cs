using Auth.TimeCafe.API.DTOs;
using Auth.TimeCafe.Domain.Models;

using Microsoft.AspNetCore.Identity;

using System.Security.Claims;

namespace Auth.TimeCafe.API.Endpoints.AccountManagement;

public sealed class CurrentUser : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/account")
            .WithTags("Authentication");

        group.MapGet("/me", async (
            ClaimsPrincipal principal,
            [FromServices] UserManager<ApplicationUser> userManager) =>
        {
            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Results.Unauthorized();

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
        })
        .RequireAuthorization()
        .WithName("GetCurrentUser")
        .WithSummary("Текущий пользователь")
        .WithDescription("Возвращает email и статус подтверждения email/телефона для текущего пользователя.");
    }
}
