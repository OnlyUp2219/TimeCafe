namespace Auth.TimeCafe.API.Extensions;

public static class EmailExtensions
{
    public static IServiceCollection AddEmailSender(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<PostmarkOptions>(configuration.GetSection("Postmark"));
        services.AddSingleton<IEmailSender<IdentityUser>, PostmarkEmailSender>();
        services.AddHttpClient();

        return services;
    }
}
