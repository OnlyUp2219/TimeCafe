using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Auth.TimeCafe.API.Endpoints;

public sealed class ChangePassword : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/account")
            .RequireAuthorization()
            .WithTags("Authentication");

        group.MapPost("/change-password", async (
            ChangePasswordRequest body,
            UserManager<IdentityUser> userManager,
            ClaimsPrincipal principal,
            ApplicationDbContext db
        ) =>
        {
            if (string.IsNullOrWhiteSpace(body.CurrentPassword) || string.IsNullOrWhiteSpace(body.NewPassword))
                return Results.BadRequest(new { errors = new[] { new { code = "EmptyFields", description = "Требуется заполнить пароли" } } });

            var user = await userManager.GetUserAsync(principal);
            if (user == null)
                return Results.Unauthorized();

            var result = await userManager.ChangePasswordAsync(user, body.CurrentPassword, body.NewPassword);
            if (!result.Succeeded)
            {
                var errs = result.Errors.Select(e => new { code = e.Code, description = e.Description }).ToArray();
                return Results.BadRequest(new { errors = errs });
            }

            var userId = user.Id;
            var tokens = await db.RefreshTokens.Where(t => t.UserId == userId && !t.IsRevoked).ToListAsync();
            if (tokens.Count > 0)
            {
                foreach (var t in tokens)
                    t.IsRevoked = true;
                await db.SaveChangesAsync();
            }

            return Results.Ok(new { message = "Пароль изменён", refreshTokensRevoked = tokens.Count });
        })
        .WithName("ChangePassword")
        .WithSummary("Смена пароля текущего пользователя");
    }

    public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
}
