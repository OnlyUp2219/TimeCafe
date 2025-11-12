namespace Auth.TimeCafe.API.Endpoints.AccountManagement;

public class PhoneVerification : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/twilio")
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

            return result.ToHttpResult(onSuccess: r =>
            {
                var g = (GeneratePhoneVerificationResult)r;
                return Results.Ok(new { phoneNumber = g.PhoneNumber, message = g.Message, token = g.Token });
            });
        })
        .RequireRateLimiting("OneRequestPerInterval")
        .RequireRateLimiting("MaxRequestPerWindow")
        .WithName("GenerateSmsMock");

        group.MapPost("verifySMS-mock", async (
            ISender sender,
            ClaimsPrincipal user,
            [FromBody] PhoneVerificationModel model
        ) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Results.Unauthorized();

            var command = new VerifyPhoneCommand(userId, model.PhoneNumber, model.Code, model.CaptchaToken, Mock: true);
            var result = await sender.Send(command);

            if (!result.Success && result.Errors?.Contains("TooManyAttempts") == true)
                return Results.Json(new { message = result.Message, remainingAttempts = 0 }, statusCode: 429);

            if (!result.Success && (result.Errors?.Contains("UserNotFound") == true))
                return Results.NotFound(new { message = result.Message });

            if (!result.Success && (result.Errors?.Contains("CaptchaRequired") == true || result.Errors?.Contains("CaptchaInvalid") == true || result.Errors?.Contains("InvalidCode") == true))
                return Results.Json(new { message = result.Message, remainingAttempts = result.RemainingAttempts, requiresCaptcha = result.RequiresCaptcha }, statusCode: 400);

            return result.ToHttpResult(onSuccess: r =>
            {
                var v = (VerifyPhoneResult)r;
                return Results.Ok(new { message = v.Message, phoneNumber = v.PhoneNumber });
            });
        })
        .WithName("VerifySmsMock");

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

            return result.ToHttpResult(onSuccess: r =>
            {
                var g = (GeneratePhoneVerificationResult)r;
                return Results.Ok(new { phoneNumber = g.PhoneNumber, message = g.Message });
            });
        })
        .RequireRateLimiting("OneRequestPerInterval")
        .RequireRateLimiting("MaxRequestPerWindow")
        .WithName("GenerateSms");

        group.MapPost("verifySMS", async (
            ISender sender,
            ClaimsPrincipal user,
            [FromBody] PhoneVerificationModel model
        ) =>
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Results.Unauthorized();

            var command = new VerifyPhoneCommand(userId, model.PhoneNumber, model.Code, model.CaptchaToken, Mock: false);
            var result = await sender.Send(command);

            if (!result.Success && result.Errors?.Contains("TooManyAttempts") == true)
                return Results.Json(new { message = result.Message, remainingAttempts = 0 }, statusCode: 429);

            if (!result.Success && (result.Errors?.Contains("UserNotFound") == true))
                return Results.NotFound(new { message = result.Message });

            if (!result.Success && (result.Errors?.Contains("CaptchaRequired") == true || result.Errors?.Contains("CaptchaInvalid") == true || result.Errors?.Contains("InvalidCode") == true))
                return Results.Json(new { message = result.Message, remainingAttempts = result.RemainingAttempts, requiresCaptcha = result.RequiresCaptcha }, statusCode: 400);

            return result.ToHttpResult(onSuccess: r =>
            {
                var v = (VerifyPhoneResult)r;
                return Results.Ok(new { message = v.Message, phoneNumber = v.PhoneNumber });
            });
        })
        .WithName("VerifySms");
    }
}

