namespace Auth.TimeCafe.API.Extensions;

public static class SmsExtensions
{
    public static IServiceCollection AddSmsServices(this IServiceCollection services)
    {
        services.AddScoped<ITwilioSender, TwilioSender>();
        
        services.AddSingleton<ISmsRateLimiter, SmsRateLimiter>();
        services.AddSingleton<ISmsVerificationAttemptTracker, SmsVerificationAttemptTracker>();
        
        services.AddHttpClient<ICaptchaValidator, GoogleRecaptchaValidator>();

        services.AddMemoryCache();

        return services;
    }
}
