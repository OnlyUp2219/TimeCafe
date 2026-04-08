using Microsoft.Extensions.Options;

namespace BuildingBlocks.Extensions;

public static class OptionsExtensions
{
    public static OptionsBuilder<TOptions> AddValidatedOptions<TOptions>(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName,
        Func<TOptions, bool>? customValidation = null,
        string? customValidationFailureMessage = null)
        where TOptions : class
    {
        var section = configuration.GetSection(sectionName);

        var builder = services
            .AddOptions<TOptions>()
            .Bind(section)
            .Validate(_ => section.Exists(), $"Configuration section '{sectionName}' is missing.")
            .ValidateDataAnnotations();

        if (customValidation is not null)
        {
            builder = builder.Validate(
                customValidation,
                customValidationFailureMessage ?? $"Configuration section '{sectionName}' is invalid.");
        }

        return builder.ValidateOnStart();
    }
}