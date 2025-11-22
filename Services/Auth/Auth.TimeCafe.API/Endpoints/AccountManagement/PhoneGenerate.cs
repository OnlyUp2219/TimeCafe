namespace Auth.TimeCafe.API.Endpoints.AccountManagement;

public class PhoneGenerate : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/twilio")
            .RequireAuthorization()
            .WithTags("SMS");

        group.MapPost("generateSMS-mock", async (
            ISender sender,
            ClaimsPrincipal user,
            [FromBody] PhoneVerificationModel model
        ) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Results.Unauthorized();

            var command = new GeneratePhoneVerificationCommand(userId, model.PhoneNumber, Mock: true);
            var result = await sender.Send(command);

            return result.ToHttpResultV2(onSuccess: r =>
            {
                return Results.Ok(new { phoneNumber = r.PhoneNumber, message = r.Message, token = r.Token });
            });
        })
        .RequireRateLimiting("OneRequestPerInterval")
        .RequireRateLimiting("MaxRequestPerWindow")
        .WithName("GenerateSmsMock")
        .WithSummary("Mock: генерация SMS-кода для подтверждения телефона")
        .WithDescription("Тестовый endpoint: возвращает SMS-код и токен для подтверждения телефона без реальной отправки сообщения.");

        group.MapPost("generateSMS", async (
            ISender sender,
            ClaimsPrincipal user,
            [FromBody] PhoneVerificationModel model
        ) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Results.Unauthorized();

            var command = new GeneratePhoneVerificationCommand(userId, model.PhoneNumber, Mock: false);
            var result = await sender.Send(command);

            return result.ToHttpResultV2(onSuccess: r =>
            {
                return Results.Ok(new { phoneNumber = r.PhoneNumber, message = r.Message });
            });
        })
        .RequireRateLimiting("OneRequestPerInterval")
        .RequireRateLimiting("MaxRequestPerWindow")
        .WithName("GenerateSms")
        .WithSummary("Генерация SMS-кода для подтверждения телефона")
        .WithDescription("Отправляет SMS-код на указанный номер телефона для подтверждения. Используется при регистрации или смене номера.");

    }
}

