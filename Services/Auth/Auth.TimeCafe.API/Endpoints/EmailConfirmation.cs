namespace Auth.TimeCafe.API.Endpoints;

public class EmailConfirmation : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth").WithTags("EmailConfirmation");

        group.MapGet("/confirm", async (
            [FromQuery] string userId,
            [FromQuery] string token,
            UserManager<IdentityUser> userManager,
            [FromQuery] string? redirect) =>
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Results.BadRequest(new { errors = new[] { "Неверный email или пароль" } }); // унифицированно
            }
            if (user.EmailConfirmed)
            {
                return redirect is not null
                    ? Results.Redirect(redirect)
                    : Results.Ok(new { status = "already_confirmed" });
            }

            var result = await userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
            {
                return Results.BadRequest(new { errors = new[] { "Неверный email или пароль" } });
            }

            return redirect is not null
                ? Results.Redirect(redirect)
                : Results.Ok(new { status = "confirmed" });
        })
        .WithName("ConfirmEmail")
        .WithSummary("Подтверждение email по токену");

        group.MapPost("/resend-confirmation", async (
            [FromBody] ResendConfirmationRequest body,
            UserManager<IdentityUser> userManager,
            IEmailSender<IdentityUser> emailSender,
            IConfiguration configuration) =>
        {
            var user = await userManager.FindByEmailAsync(body.Email);
            if (user == null)
            {
                return Results.Ok(new { message = "Если аккаунт существует — письмо отправлено" });
            }

            if (user.EmailConfirmed)
            {
                return Results.Ok(new { message = "Если аккаунт существует — письмо отправлено" });
            }

            // TODO(THROTTLE): Интегрировать RateLimiter (п.5) для ограничения частоты

            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var frontendBase = configuration["Frontend:BaseUrl"] ?? "http://localhost:5173";
            var confirmLink = $"{frontendBase}/confirm-email?userId={Uri.EscapeDataString(user.Id)}&token={Uri.EscapeDataString(token)}";
            await emailSender.SendConfirmationLinkAsync(user, user.Email!, confirmLink);

            return Results.Ok(new { message = "Если аккаунт существует — письмо отправлено" });
        })
        .WithName("ResendConfirmation")
        .WithSummary("Повторная отправка письма подтверждения (унифицированный ответ)");
    }
}

public record ResendConfirmationRequest(string Email);