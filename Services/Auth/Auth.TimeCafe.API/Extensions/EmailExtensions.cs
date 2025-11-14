namespace Auth.TimeCafe.API.Extensions;

public static class EmailExtensions
{
    public static IServiceCollection AddEmailSender(this IServiceCollection services, IConfiguration configuration)
    {


        services.Configure<PostmarkOptions>(configuration.GetSection("Postmark"));

        var postmarkSection = configuration.GetSection("Postmark");
        if (!postmarkSection.Exists())
            throw new InvalidOperationException("Postmark configuration section is missing.");

        if (string.IsNullOrWhiteSpace(postmarkSection["ServerToken"]))
            throw new InvalidOperationException("Postmark:ServerToken is not configured.");

        if (string.IsNullOrWhiteSpace(postmarkSection["FromEmail"]))
            throw new InvalidOperationException("Postmark:FromEmail is not configured.");

        if (string.IsNullOrWhiteSpace(postmarkSection["MessageStream"]))
            throw new InvalidOperationException("Postmark:MessageStream is not configured.");

        if (string.IsNullOrWhiteSpace(postmarkSection["FrontendBaseUrl"]))
            throw new InvalidOperationException("Postmark:FrontendBaseUrl is not configured.");



        services.AddSingleton<PostmarkEmailSender>();


        services.AddSingleton<IEmailSender<IdentityUser>>(sp =>
            new BackgroundEmailSender(sp.GetRequiredService<PostmarkEmailSender>(), sp.GetRequiredService<ILogger<BackgroundEmailSender>>()));
        services.AddHttpClient();

        return services;
    }
}
