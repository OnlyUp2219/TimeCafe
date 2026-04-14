namespace BuildingBlocks.Extensions;

public static class SharedConfigurationExtensions
{
    public static WebApplicationBuilder AddSharedConfiguration(this WebApplicationBuilder builder)
    {
        var contentRootPath = builder.Environment.ContentRootPath;

        var sharedSettingsCandidates = new[]
        {
            Path.Combine(contentRootPath, "appsettings.shared.json"),
            Path.GetFullPath(Path.Combine(contentRootPath, "..", "appsettings.shared.json")),
            Path.GetFullPath(Path.Combine(contentRootPath, "..", "..", "appsettings.shared.json")),
            Path.GetFullPath(Path.Combine(contentRootPath, "..", "..", "..", "appsettings.shared.json"))
        };

        var sharedSettingsPath = sharedSettingsCandidates.FirstOrDefault(File.Exists) 
        ?? throw new InvalidOperationException($"appsettings.shared.json not found. Checked paths: {string.Join(", ", sharedSettingsCandidates)}");

        builder.Configuration.AddJsonFile(sharedSettingsPath, optional: false, reloadOnChange: true);

        return builder;
    }
}