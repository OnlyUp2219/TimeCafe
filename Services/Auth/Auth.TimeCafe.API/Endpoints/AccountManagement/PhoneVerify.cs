namespace Auth.TimeCafe.API.Endpoints.AccountManagement;

public class PhoneVerify : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/twilio")
            .WithTags("SMS");

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

            var extra = new { phoneNumber = result.PhoneNumber, remainingAttempts = result.RemainingAttempts, requiresCaptcha = result.RequiresCaptcha };
            return result.ToHttpResultV2(onSuccess: r =>
            {
                return Results.Ok(new { message = r.Message, phoneNumber = r.PhoneNumber, remainingAttempts = r.RemainingAttempts, requiresCaptcha = r.RequiresCaptcha });
            }, extra);
        })
        .WithName("VerifySmsMock");

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

            var extra = new { phoneNumber = result.PhoneNumber, remainingAttempts = result.RemainingAttempts, requiresCaptcha = result.RequiresCaptcha };
            return result.ToHttpResultV2(onSuccess: r =>
            {
                return Results.Ok(new { message = r.Message, phoneNumber = r.PhoneNumber, remainingAttempts = r.RemainingAttempts, requiresCaptcha = r.RequiresCaptcha });
            }, extra);
        })
        .WithName("VerifySms");
    }
}

