namespace Auth.TimeCafe.API.Endpoints.AccountManagement.Phone;

public class PhoneVerify : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/twilio")
            .RequireAuthorization(policy => policy.RequirePermissions(Permissions.AccountPhoneVerify))
            .WithTags("SMS");

        group.MapPost("verifySMS-mock", async (
            [FromServices] ISender sender,
            ClaimsPrincipal user,
            [FromBody] PhoneVerificationRequest model
        ) =>
        {
            var userId = user.FindFirst("sub")?.Value;
            if (userId == null)
                return Results.Unauthorized();

            var command = new VerifyPhoneCommand(userId, model.PhoneNumber, model.Code, model.CaptchaToken, Mock: true);
            var result = await sender.Send(command);

            var extra = new { phoneNumber = result.PhoneNumber, remainingAttempts = result.RemainingAttempts, requiresCaptcha = result.RequiresCaptcha };
            return result.ToHttpResult(onSuccess: r => Results.Ok(new { message = r.Message, phoneNumber = r.PhoneNumber, remainingAttempts = r.RemainingAttempts, requiresCaptcha = r.RequiresCaptcha }), extra);
        })
        .WithName("VerifySmsMock")
        .WithSummary("Mock: проверка SMS-кода для подтверждения телефона")
        .Produces(200)
        .WithDescription("Тестовый endpoint: проверяет SMS-код для подтверждения телефона без реального взаимодействия с сервисом. Возвращает результат проверки, количество оставшихся попыток и необходимость капчи.");

        group.MapPost("verifySMS", async (
            [FromServices] ISender sender,
            ClaimsPrincipal user,
            [FromBody] PhoneVerificationRequest model
        ) =>
        {
            var userId = user.FindFirst("sub")?.Value;
            if (userId == null)
                return Results.Unauthorized();

            var command = new VerifyPhoneCommand(userId, model.PhoneNumber, model.Code, model.CaptchaToken, Mock: false);
            var result = await sender.Send(command);

            var extra = new { phoneNumber = result.PhoneNumber, remainingAttempts = result.RemainingAttempts, requiresCaptcha = result.RequiresCaptcha };
            return result.ToHttpResult(onSuccess: r => Results.Ok(new { message = r.Message, phoneNumber = r.PhoneNumber, remainingAttempts = r.RemainingAttempts, requiresCaptcha = r.RequiresCaptcha }), extra);
        })
        .WithName("VerifySms")
        .WithSummary("Проверка SMS-кода для подтверждения телефона")
        .Produces(200)
        .WithDescription("Проверяет введённый пользователем SMS-код для подтверждения номера телефона. Возвращает результат, количество оставшихся попыток и необходимость капчи.");
    }
}

