namespace Auth.TimeCafe.API.Endpoints.AccountManagement;

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);

public sealed class ChangePassword : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/account")
        .RequireAuthorization()
        .WithTags("Authentication");

        group.MapPost("/change-password",
        async (
        ChangePasswordRequest request,
        ClaimsPrincipal principal,
        ISender sender
        ) =>
        {
            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Results.BadRequest(new { errors = new[] { new { code = "UserNotFound", description = "Пользователь не найден" } } });

            var command = new ChangePasswordCommand(userId, request.CurrentPassword, request.NewPassword);

            var result = await sender.Send(command);

            return result.ToHttpResult(
                onSuccess: r => {
                    var cpResult = (ChangePasswordResult)r;
                    return Results.Ok(new { message = cpResult.Message, refreshTokensRevoked = cpResult.RefreshTokensRevoked });
                }
            );

        })
        .WithName("ChangePassword")
        .WithSummary("Смена пароля текущего пользователя");
    }
}
