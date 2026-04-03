namespace Auth.TimeCafe.API.Endpoints.ExternalAuth;

public abstract class ExternalProviderBase(string provider) : ICarterModule
{
    private const string Tag = "ExternalProviders";
    private readonly string _routeSlug = provider.ToLowerInvariant();

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet($"/authenticate/login/{_routeSlug}", (
            [FromQuery] string returnUrl,
            [FromServices] SignInManager<ApplicationUser> signInManager) =>
        {
            var callbackUrl = $"/auth/authenticate/login/{_routeSlug}/callback?returnUrl={Uri.EscapeDataString(returnUrl)}";
            var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, callbackUrl);
            properties.Items.Add("prompt", "select_account");
            return Results.Challenge(properties, [provider]);
        })
        .WithName($"{provider}Login")
        .WithSummary($"Инициирует вход через {provider}")
        .Produces(200)
        .WithDescription($"Перенаправляет пользователя на страницу аутентификации {provider}. После успешной аутентификации пользователь будет перенаправлен на callback URL.")
        .WithTags(Tag);

        app.MapGet($"/authenticate/login/{_routeSlug}/callback", async (
            [FromQuery] string returnUrl,
            HttpContext context,
            [FromServices] IJwtService jwtService,
            [FromServices] UserManager<ApplicationUser> userManager,
            [FromServices] ApplicationDbContext db,
            [FromServices] ILogger<ExternalProviderBase> logger) =>
            await ExternalProviderAuthHandler.HandleCallbackAsync(provider, returnUrl, context, jwtService, userManager, db, logger))
        .WithName($"{provider}LoginCallback")
        .WithSummary($"Обрабатывает callback от {provider} после аутентификации")
        .Produces(200)
        .WithDescription($"Принимает ответ от {provider}, создает или обновляет пользователя в системе, генерирует JWT токены и перенаправляет на указанный URL с токенами.")
        .WithTags(Tag);
    }
}

public class GoogleProvider() : ExternalProviderBase("Google");
public class MicrosoftProvider() : ExternalProviderBase("Microsoft");
