namespace Auth.TimeCafe.API.Endpoints.AccountManagement.Phone;

public record PhoneVerificationRequest(
    /// <example>+79001234567</example>
    string PhoneNumber,
    /// <example>123456</example>
    string Code,
    /// <example>captcha-token-xyz</example>
    string? CaptchaToken);

public class PhoneGenerate : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/twilio")
            .RequireAuthorization()
            .WithTags("SMS");

        group.MapPost("generateSMS-mock", async (
            [FromServices] ISender sender,
            ClaimsPrincipal user,
            [FromBody] PhoneVerificationRequest model
        ) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Results.Unauthorized();

            var command = new GeneratePhoneVerificationCommand(userId, model.PhoneNumber, Mock: true);
            var result = await sender.Send(command);

            return result.ToHttpResult(onSuccess: r => Results.Ok(new { phoneNumber = r.PhoneNumber, message = r.Message, token = r.Token }));
        })
        .RequireRateLimiting("OneRequestPerInterval")
        .RequireRateLimiting("MaxRequestPerWindow")
        .WithName("GenerateSmsMock")
        .WithSummary("Mock: генерация SMS-кода для подтверждения телефона")
        .Produces(200)
        .WithDescription("Тестовый endpoint: возвращает SMS-код и токен для подтверждения телефона без реальной отправки сообщения.");

        group.MapPost("generateSMS", async (
            [FromServices] ISender sender,
            ClaimsPrincipal user,
            [FromBody] PhoneVerificationRequest model
        ) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Results.Unauthorized();

            var command = new GeneratePhoneVerificationCommand(userId, model.PhoneNumber, Mock: false);
            var result = await sender.Send(command);

            return result.ToHttpResult(onSuccess: r => Results.Ok(new { phoneNumber = r.PhoneNumber, message = r.Message }));
        })
        .RequireRateLimiting("OneRequestPerInterval")
        .RequireRateLimiting("MaxRequestPerWindow")
        .WithName("GenerateSms")
        .WithSummary("Генерация SMS-кода для подтверждения телефона")
        .Produces(200)
        .WithDescription("Отправляет SMS-код на указанный номер телефона для подтверждения. Используется при регистрации или смене номера.");

    }
}

