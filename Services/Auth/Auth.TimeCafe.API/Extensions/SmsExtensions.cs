namespace Auth.TimeCafe.API.Extensions;

public static class SmsExtensions
{
    public static IServiceCollection AddSmsServices(this IServiceCollection services, IConfiguration configuration)
    {

        var twilioSection = configuration.GetSection("Twilio");
        if (!twilioSection.Exists())
            throw new InvalidOperationException("Twilio configuration section is missing.");

        if (string.IsNullOrWhiteSpace(twilioSection["AccountSid"]))
            throw new InvalidOperationException("Twilio:AccountSid is not configured.");

        if (string.IsNullOrWhiteSpace(twilioSection["AuthToken"]))
            throw new InvalidOperationException("Twilio:AuthToken is not configured.");

        if (string.IsNullOrWhiteSpace(twilioSection["TwilioPhoneNumber"]))
            throw new InvalidOperationException("Twilio:TwilioPhoneNumber is not configured.");


        services.AddScoped<ITwilioSender, TwilioSender>();
        services.AddSingleton<ISmsVerificationAttemptTracker, SmsVerificationAttemptTracker>();
        services.AddHttpClient<ICaptchaValidator, GoogleRecaptchaValidator>();
        services.AddMemoryCache();

        return services;
    }
}
