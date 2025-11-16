namespace Auth.TimeCafe.API.Endpoints.AccountManagement;

public class PhoneGenerate : ICarterModule
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

            return result.ToHttpResultV2(onSuccess: r =>
            {

                return Results.Ok(new { phoneNumber = r.PhoneNumber, message = r.Message, token = r.Token });
            });
        })
        .RequireRateLimiting("OneRequestPerInterval")
        .RequireRateLimiting("MaxRequestPerWindow")
        .WithName("GenerateSmsMock");

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
        .WithName("GenerateSms");

    }
}

