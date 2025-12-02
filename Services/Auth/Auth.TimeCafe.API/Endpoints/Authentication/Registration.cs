namespace Auth.TimeCafe.API.Endpoints.Authentication;

public class Registration : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/registerWithUsername", async (
            ISender sender,
            [FromBody] RegisterDto dto) =>
        {
            var command = new RegisterUserCommand(dto.Username, dto.Email, dto.Password, SendEmail: true);
            var result = await sender.Send(command);

            return result.ToHttpResultV2(onSuccess: r =>
            {
                return Results.Ok(new { message = r.Message });
            });
        })
            .WithTags("Authentication")
            .WithName("Register")
            .WithSummary("Регистрация пользователя с логином и email")
            .WithDescription("Регистрирует нового пользователя по логину, email и паролю. Отправляет письмо для подтверждения email.");

        app.MapPost("/registerWithUsername-mock", async (
            ISender sender,
            [FromBody] RegisterDto dto) =>
        {
            var command = new RegisterUserCommand(dto.Username, dto.Email, dto.Password, SendEmail: false);
            var result = await sender.Send(command);

            return result.ToHttpResultV2(onSuccess: r =>
            {
                return Results.Ok(new { message = r.Message, callbackUrl = r.CallbackUrl });
            });
        })
            .WithTags("Authentication")
            .WithName("RegisterMock")
            .RequireRateLimiting("OneRequestPerInterval")
            .RequireRateLimiting("MaxRequestPerWindow")
            .WithSummary("Mock: регистрация пользователя с логином и email")
            .WithDescription("Тестовый endpoint: регистрирует пользователя, но не отправляет письмо. Возвращает callbackUrl для подтверждения email.");
    }
}
