using Auth.TimeCafe.API.Services;

namespace Auth.TimeCafe.API.Endpoints.ExternalAuth;

public class MicrosoftProvider : ICarterModule
{
    private const string Provider = "Microsoft";
    private const string Tag = "ExternalProviders";

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/authenticate/login/microsoft", (
            [FromQuery] string returnUrl,
            [FromServices] SignInManager<ApplicationUser> signInManager) =>
        {
            var callbackUrl = $"/auth/authenticate/login/microsoft/callback?returnUrl={Uri.EscapeDataString(returnUrl)}";
            var properties = signInManager.ConfigureExternalAuthenticationProperties(Provider, callbackUrl);
            properties.Items.Add("prompt", "select_account");
            return Results.Challenge(properties, [Provider]);
        })
        .WithName("MicrosoftLogin")
        .WithSummary("Инициирует вход через Microsoft")
        .Produces(200)
        .WithDescription("Перенаправляет пользователя на страницу аутентификации Microsoft. После успешной аутентификации пользователь будет перенаправлен на callback URL.")
        .WithTags(Tag);

        app.MapGet("/authenticate/login/microsoft/callback", async (
            [FromQuery] string returnUrl,
            HttpContext context,
            [FromServices] IJwtService jwtService,
            [FromServices] UserManager<ApplicationUser> userManager,
            [FromServices] ApplicationDbContext db,
            [FromServices] ILogger<MicrosoftProvider> logger) =>
            await ExternalProviderAuthHandler.HandleCallbackAsync(Provider, returnUrl, context, jwtService, userManager, db, logger))
        .WithName("MicrosoftLoginCallback")
        .WithSummary("Обрабатывает callback от Microsoft после аутентификации")
        .Produces(200)
        .WithDescription("Принимает ответ от Microsoft, создает или обновляет пользователя в системе, генерирует JWT токены и перенаправляет на указанный URL с токенами.")
        .WithTags(Tag);
    }
}
