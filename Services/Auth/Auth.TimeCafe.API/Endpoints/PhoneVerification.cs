namespace Auth.TimeCafe.API.Endpoints;

public class PhoneVerification : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/twilio")
            .WithTags("SMS");

        group.MapPost("generateSMS-mock", async (
            UserManager<IdentityUser> userManager,
            ClaimsPrincipal user,
            ISmsVerificationAttemptTracker attemptTracker,
            ILogger<PhoneVerification> logger,
            [FromBody] PhoneVerificationModel model
            ) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Results.Unauthorized();

            var u = await userManager.FindByIdAsync(userId);
            if (u == null) return Results.NotFound();

            logger.LogInformation("[MOCK] Генерация кода подтверждения для номера {PhoneNumber}, пользователь {UserId}", model.PhoneNumber, userId);

            var token = await userManager.GenerateChangePhoneNumberTokenAsync(u, model.PhoneNumber);

            logger.LogWarning("[MOCK] Код подтверждения для {PhoneNumber}: {Token}", model.PhoneNumber, token);
            attemptTracker.ResetAttempts(userId, model.PhoneNumber);

            return Results.Ok(new { phoneNumber = model.PhoneNumber, message = "SMS отправлено (mock)", token });
        })
        .RequireRateLimiting("OneRequestPerInterval")
        .RequireRateLimiting("MaxRequestPerWindow");

        group.MapPost("verifySMS-mock", async (
            UserManager<IdentityUser> userManager,
            ClaimsPrincipal user,
            ISmsVerificationAttemptTracker attemptTracker,
            ICaptchaValidator captchaValidator,
            ILogger<PhoneVerification> logger,
            [FromBody] PhoneVerificationModel model
            ) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Results.Unauthorized();

            var u = await userManager.FindByIdAsync(userId);
            if (u == null) return Results.NotFound();

            if (!attemptTracker.CanVerifyCode(userId, model.PhoneNumber))
            {
                logger.LogWarning("[MOCK] Превышено количество попыток проверки кода для пользователя {UserId}, телефон {PhoneNumber}", userId, model.PhoneNumber);
                return Results.Json(
                    new { message = "Превышено количество попыток. Запросите новый код.", remainingAttempts = 0 },
                    statusCode: 429
                );
            }

            var remaining = attemptTracker.GetRemainingAttempts(userId, model.PhoneNumber);

            if (remaining == 3)
            {
                if (string.IsNullOrEmpty(model.CaptchaToken))
                {
                    return Results.Json(
                        new { message = "Пройдите проверку капчи", requiresCaptcha = true, remainingAttempts = remaining },
                        statusCode: 400
                    );
                }

                if (!await captchaValidator.ValidateAsync(model.CaptchaToken))
                {
                    return Results.Json(
                        new { message = "Пройдите проверку капчи", requiresCaptcha = true, remainingAttempts = remaining },
                        statusCode: 400
                    );
                }
            }

            logger.LogInformation("[MOCK] Попытка подтверждения номера {PhoneNumber} для пользователя {UserId}", model.PhoneNumber, userId);

            var result = await userManager.ChangePhoneNumberAsync(u, model.PhoneNumber, model.Code);

            if (result.Succeeded)
            {
                attemptTracker.ResetAttempts(userId, model.PhoneNumber);
                logger.LogInformation("[MOCK] Номер телефона {PhoneNumber} успешно подтвержден для пользователя {UserId}", model.PhoneNumber, userId);
                return Results.Ok(new { message = "Номер телефона успешно подтвержден (mock)", phoneNumber = model.PhoneNumber });
            }

            attemptTracker.RecordFailedAttempt(userId, model.PhoneNumber);
            remaining = attemptTracker.GetRemainingAttempts(userId, model.PhoneNumber);

            logger.LogWarning("[MOCK] Неудачная попытка подтверждения номера {PhoneNumber} для пользователя {UserId}. Осталось попыток: {Remaining}",
                model.PhoneNumber, userId, remaining);

            return Results.Json(
                new { message = "Неверный код подтверждения или истек срок действия", remainingAttempts = remaining, requiresCaptcha = remaining == 3 },
                statusCode: 400
            );
        });


        group.MapPost("generateSMS", async (
            IMediator mediator,
            UserManager<IdentityUser> userManager,
            ClaimsPrincipal user,
            IConfiguration configuration,
            ISmsVerificationAttemptTracker attemptTracker,
            ILogger<PhoneVerification> logger,
            [FromBody] PhoneVerificationModel model
            )
         =>
            {
                var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null) return Results.Unauthorized();

                var u = await userManager.FindByIdAsync(userId);
                if (u == null) return Results.NotFound();

                logger.LogInformation("Генерация кода подтверждения для номера {PhoneNumber}, пользователь {UserId}", model.PhoneNumber, userId);

                var token = await userManager.GenerateChangePhoneNumberTokenAsync(u, model.PhoneNumber);

                string accountSid = configuration["Twilio:AccountSid"] ?? "";
                string authToken = configuration["Twilio:AuthToken"] ?? "";
                string twilioPhoneNumber = configuration["Twilio:TwilioPhoneNumber"] ?? "";

                var query = new SendSmsCommand(accountSid, authToken, twilioPhoneNumber, model.PhoneNumber, token);
                var result = await mediator.Send(query);

                if (result != null)
                {
                    attemptTracker.ResetAttempts(userId, model.PhoneNumber);
                    return Results.Ok(new { phoneNumber = result.PhoneNumber, message = "SMS отправлено" });
                }

                logger.LogError("Ошибка при отправке SMS для номера {PhoneNumber}, пользователь {UserId}", model.PhoneNumber, userId);
                return Results.BadRequest("Ошибка при отправке SMS");
            })
        .RequireRateLimiting("OneRequestPerInterval")
        .RequireRateLimiting("MaxRequestPerWindow");

        group.MapPost("verifySMS", async (
            UserManager<IdentityUser> userManager,
            ClaimsPrincipal user,
            ISmsVerificationAttemptTracker attemptTracker,
            ICaptchaValidator captchaValidator,
            ILogger<PhoneVerification> logger,
            [FromBody] PhoneVerificationModel model
            ) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Results.Unauthorized();

            var u = await userManager.FindByIdAsync(userId);
            if (u == null) return Results.NotFound();

            if (!attemptTracker.CanVerifyCode(userId, model.PhoneNumber))
            {
                logger.LogWarning("Превышено количество попыток проверки кода для пользователя {UserId}, телефон {PhoneNumber}", userId, model.PhoneNumber);
                return Results.Json(
                    new { message = "Превышено количество попыток. Запросите новый код.", remainingAttempts = 0 },
                    statusCode: 429
                );
            }

            var remaining = attemptTracker.GetRemainingAttempts(userId, model.PhoneNumber);

            if (remaining == 3)
            {
                if (string.IsNullOrEmpty(model.CaptchaToken))
                {
                    return Results.Json(
                        new { message = "Пройдите проверку капчи", requiresCaptcha = true, remainingAttempts = remaining },
                        statusCode: 400
                    );
                }

                if (!await captchaValidator.ValidateAsync(model.CaptchaToken))
                {
                    return Results.Json(
                        new { message = "Пройдите проверку капчи", requiresCaptcha = true, remainingAttempts = remaining },
                        statusCode: 400
                    );
                }
            }

            logger.LogInformation("Попытка подтверждения номера {PhoneNumber} для пользователя {UserId}", model.PhoneNumber, userId);

            var result = await userManager.ChangePhoneNumberAsync(u, model.PhoneNumber, model.Code);

            if (result.Succeeded)
            {
                attemptTracker.ResetAttempts(userId, model.PhoneNumber);
                logger.LogInformation("Номер телефона {PhoneNumber} успешно подтвержден для пользователя {UserId}", model.PhoneNumber, userId);
                return Results.Ok(new { message = "Номер телефона успешно подтвержден", phoneNumber = model.PhoneNumber });
            }

            attemptTracker.RecordFailedAttempt(userId, model.PhoneNumber);
            remaining = attemptTracker.GetRemainingAttempts(userId, model.PhoneNumber);

            return Results.Json(
                new { message = "Неверный код подтверждения или истек срок действия", remainingAttempts = remaining, requiresCaptcha = remaining == 3 },
                statusCode: 400
            );
        });
    }
}

