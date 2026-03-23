namespace Auth.TimeCafe.API.Endpoints.Authentication;

public record RegisterRequest(string Username, string Email, string Password);

public class Registration : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/registerWithUsername", async (
            [FromServices] ISender sender,
            [FromBody] RegisterRequest request) =>
        {
            var command = new RegisterUserCommand(request.Username, request.Email, request.Password, SendEmail: true);
            var result = await sender.Send(command);

            return result.ToHttpResultV2(onSuccess: r => Results.Ok(new { message = r.Message }));
        })
            .WithTags("Authentication")
            .WithName("Register")
            .WithSummary("Регистрация пользователя с логином и email")
            .WithDescription("Регистрирует нового пользователя по логину, email и паролю. Отправляет письмо для подтверждения email.");

        app.MapPost("/registerWithUsername-mock", async (
            [FromServices] ISender sender,
            [FromBody] RegisterRequest request) =>
        {
            var command = new RegisterUserCommand(request.Username, request.Email, request.Password, SendEmail: false);
            var result = await sender.Send(command);

            return result.ToHttpResultV2(onSuccess: r => Results.Ok(new { message = r.Message, callbackUrl = r.CallbackUrl }));
        })
            .WithTags("Authentication")
            .WithName("RegisterMock")
            .RequireRateLimiting("OneRequestPerInterval")
            .RequireRateLimiting("MaxRequestPerWindow")
            .WithSummary("Mock: регистрация пользователя с логином и email")
            .WithDescription("Тестовый endpoint: регистрирует пользователя, но не отправляет письмо. Возвращает callbackUrl для подтверждения email.");
    }
}
