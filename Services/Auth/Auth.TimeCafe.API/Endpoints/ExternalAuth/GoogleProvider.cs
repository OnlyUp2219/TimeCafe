using Auth.TimeCafe.API.Services;

namespace Auth.TimeCafe.API.Endpoints.ExternalAuth;

public class GoogleProvider : ICarterModule
{
    private const string Provider = "Google";
    private const string Tag = "ExternalProviders";

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/authenticate/login/google", (
            [FromQuery] string returnUrl,
            [FromServices] SignInManager<ApplicationUser> signInManager) =>
        {
            var callbackUrl = $"/auth/authenticate/login/google/callback?returnUrl={Uri.EscapeDataString(returnUrl)}";
            var properties = signInManager.ConfigureExternalAuthenticationProperties(Provider, callbackUrl);
            properties.Items.Add("prompt", "select_account");
            return Results.Challenge(properties, [Provider]);
        })
        .WithName("GoogleLogin")
        .WithSummary("Инициирует вход через Google")
        .Produces(200)
        .WithDescription("Перенаправляет пользователя на страницу аутентификации Google. После успешной аутентификации пользователь будет перенаправлен на callback URL.")
        .WithTags(Tag);

        app.MapGet("/authenticate/login/google/callback", async (
            [FromQuery] string returnUrl,
            HttpContext context,
            [FromServices] IJwtService jwtService,
            [FromServices] UserManager<ApplicationUser> userManager,
            [FromServices] ApplicationDbContext db,
            [FromServices] ILogger<GoogleProvider> logger) =>
            await ExternalProviderAuthHandler.HandleCallbackAsync(Provider, returnUrl, context, jwtService, userManager, db, logger))
        .WithName("GoogleLoginCallback")
        .WithSummary("Обрабатывает callback от Google после аутентификации")
        .Produces(200)
        .WithDescription("Принимает ответ от Google, создает или обновляет пользователя в системе, генерирует JWT токены и перенаправляет на указанный URL с токенами.")
        .WithTags(Tag);
    }
}
