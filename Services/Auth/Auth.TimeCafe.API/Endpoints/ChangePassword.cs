using Auth.TimeCafe.Domain.Enum;

namespace Auth.TimeCafe.API.Endpoints;

public sealed class ChangePassword : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/account")
            .RequireAuthorization()
            .WithTags("Authentication");

        group.MapPost("/change-password", async (
            ChangePasswordRequest request,
            UserManager<IdentityUser> userManager,
            ClaimsPrincipal principal,
            ApplicationDbContext db
            , ISender sender
        ) =>
        {
            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Results.BadRequest(new { errors = new[] { new { code = "UserNotFound", description = "Пользователь не найден" } } } );

            var command = new ChangePasswordCommand(userId, request.CurrentPassword, request.NewPassword);

            var result = await sender.Send(command);

            if (!result.Success)
            {
                var errs = result.Errors?.Select(e => new {
                    code = e.Split(":", StringSplitOptions.RemoveEmptyEntries)[0],
                    description = e.Split(":", StringSplitOptions.RemoveEmptyEntries)[1]
                }).ToArray();


                return result.TypeError switch
                {
                    ETypeError.Unauthorized => Results.Unauthorized(),
                    ETypeError.IdentityError => Results.BadRequest(new { errors = errs }),
                    ETypeError.BadRequest => Results.BadRequest(new { errors = result.Errors }),
                    _ => Results.BadRequest(new { message = result.Message })
                };
            }

            return Results.Ok(new { message = "Пароль изменён", refreshTokensRevoked = result.RefreshTokensRevoked });
        })
        .WithName("ChangePassword")
        .WithSummary("Смена пароля текущего пользователя");
    }

    public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
}
