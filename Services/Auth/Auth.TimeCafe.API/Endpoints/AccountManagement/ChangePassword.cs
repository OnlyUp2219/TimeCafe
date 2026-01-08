namespace Auth.TimeCafe.API.Endpoints.AccountManagement;

public sealed class ChangePassword : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/account")
        .RequireAuthorization()
        .WithTags("Authentication");

        group.MapPost("/change-password",
        async (
        [FromBody] ChangePasswordRequest request,
        [FromServices] ClaimsPrincipal principal,
        [FromServices] ISender sender
        ) =>
        {
            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Results.BadRequest(new { errors = new[] { new { code = "UserNotFound", description = "Пользователь не найден" } } });

            var command = new ChangePasswordCommand(userId, request.CurrentPassword, request.NewPassword);

            var result = await sender.Send(command);

            return result.ToHttpResultV2(
                onSuccess: r =>
                {
                    return Results.Ok(new { message = r.Message, refreshTokensRevoked = r.RefreshTokensRevoked });
                }
            );

        })
        .WithName("ChangePassword")
        .WithSummary("Смена пароля текущего пользователя")
        .WithDescription("Позволяет авторизованному пользователю сменить свой пароль. Требует текущий и новый пароль. Возвращает сообщение об успехе и количество отозванных refresh-токенов.");
    }
}
