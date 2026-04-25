namespace Auth.TimeCafe.API.Extensions;

public static class DatabaseExtensions
{
    public static async Task ApplyMigrationsAsync(this WebApplication app)
    {
        if (app.Configuration.GetValue<bool>("SkipMigrations"))
            return;

        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        var retries = 5;
        while (retries > 0)
        {
            try
            {
                if (dbContext.Database.IsRelational())
                {
                    await dbContext.Database.MigrateAsync();
                }
                else
                {
                    await dbContext.Database.EnsureCreatedAsync();
                }
                break;
            }
            catch (Exception ex)
            {
                retries--;
                if (retries == 0)
                {
                    logger.LogCritical(ex, "Failed to apply migrations after several retries.");
                    throw;
                }
                logger.LogWarning("Failed to apply migrations, retrying in 2 seconds... ({Retries} retries left)", retries);
                await Task.Delay(2000);
            }
        }
    }

    public static async Task SeedDevelopmentUsersAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        await SeedData.SeedAdminAsync(scope.ServiceProvider);
        await SeedData.SeedLoadTestUserAsync(scope.ServiceProvider);
    }
}
