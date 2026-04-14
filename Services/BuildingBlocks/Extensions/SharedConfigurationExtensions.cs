namespace BuildingBlocks.Extensions;

public static class SharedConfigurationExtensions
{
    public static WebApplicationBuilder AddSharedConfiguration(this WebApplicationBuilder builder)
    {
        var contentRootPath = builder.Environment.ContentRootPath;

        var sharedSettingsCandidates = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Path.Combine(contentRootPath, "appsettings.shared.json"),
            Path.Combine(AppContext.BaseDirectory, "appsettings.shared.json")
        };

        var currentPath = contentRootPath;
        for (var level = 0; level < 8; level++)
        {
            var parent = Directory.GetParent(currentPath);
            if (parent is null)
            {
                break;
            }

            currentPath = parent.FullName;
            sharedSettingsCandidates.Add(Path.Combine(currentPath, "appsettings.shared.json"));
        }

        var sharedSettingsPath = sharedSettingsCandidates.FirstOrDefault(File.Exists);
        if (!string.IsNullOrWhiteSpace(sharedSettingsPath))
        {
            builder.Configuration.AddJsonFile(sharedSettingsPath, optional: false, reloadOnChange: true);
        }
        else
        {
            builder.Configuration.AddJsonFile("appsettings.shared.json", optional: true, reloadOnChange: true);
        }

        builder.Configuration.AddEnvironmentVariables();

        return builder;
    }
}