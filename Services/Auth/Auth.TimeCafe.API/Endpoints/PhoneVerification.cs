namespace Auth.TimeCafe.API.Endpoints;

public class PhoneVerification : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("generateSMS", async (
            IMediator mediator,
            UserManager<IdentityUser> userManager,
            ClaimsPrincipal user,
            IConfiguration configuration,
            ISmsRateLimiter rateLimiter,
            ILogger<PhoneVerification> logger,
            [FromBody] PhoneVerificationModel model
            )
         =>
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null) return Results.Unauthorized();

                var u = await userManager.FindByIdAsync(userId);
                if (u == null) return Results.NotFound();

                if (!rateLimiter.CanSendSms(userId))
                {
                    logger.LogWarning("Rate limit превышен для пользователя {UserId} при попытке отправки SMS", userId);
                    return Results.BadRequest("Слишком частые запросы. Попробуйте через минуту.");
                }

                logger.LogInformation("Генерация кода подтверждения для номера {PhoneNumber}, пользователь {UserId}", model.PhoneNumber, userId);

                var token = await userManager.GenerateChangePhoneNumberTokenAsync(u, model.PhoneNumber);

                string accountSid = configuration["Twilio:AccountSid"] ?? "";
                string authToken = configuration["Twilio:AuthToken"] ?? "";
                string twilioPhoneNumber = configuration["Twilio:TwilioPhoneNumber"] ?? "";

                var query = new SendCommand(accountSid, authToken, twilioPhoneNumber, model.PhoneNumber, token);
                var result = await mediator.Send(query);

                if (result != null)
                {
                    rateLimiter.RecordSmsSent(userId);
                    return Results.Ok(new { phoneNumber = result.PhoneNumber, message = "SMS отправлено" });
                }

                logger.LogError("Ошибка при отправке SMS для номера {PhoneNumber}, пользователь {UserId}", model.PhoneNumber, userId);
                return Results.BadRequest("Ошибка при отправке SMS");
            });


        app.MapPost("verifySMS", async (
            UserManager<IdentityUser> userManager,
            ClaimsPrincipal user,
            ILogger<PhoneVerification> logger,
            [FromBody] PhoneVerificationModel model
            ) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Results.Unauthorized();

            var u = await userManager.FindByIdAsync(userId);
            if (u == null) return Results.NotFound();

            logger.LogInformation("Попытка подтверждения номера {PhoneNumber} для пользователя {UserId}", model.PhoneNumber, userId);

            var result = await userManager.ChangePhoneNumberAsync(u, model.PhoneNumber, model.Code);

            if (result.Succeeded)
            {
                logger.LogInformation("Номер телефона {PhoneNumber} успешно подтвержден для пользователя {UserId}", model.PhoneNumber, userId);
                return Results.Ok(new { message = "Номер телефона успешно подтвержден", phoneNumber = model.PhoneNumber });
            }

            logger.LogWarning("Неудачная попытка подтверждения номера {PhoneNumber} для пользователя {UserId}", model.PhoneNumber, userId);
            return Results.BadRequest(new { message = "Неверный код подтверждения или истек срок действия" });
        });
    }
}

