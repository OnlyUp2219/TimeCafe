namespace BuildingBlocks.Extensions;

public static class DatabaseExtensions
{
    public static IServiceCollection AddPostgresDatabase<TContext>(this IServiceCollection services, IConfiguration configuration)
        where TContext : DbContext
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<TContext>(options =>
        {
            // TODO : убрать в проде.
            options.UseNpgsql(connectionString).ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));

            options.UseNpgsql(connectionString);
        });

        return services;
    }

    public static async Task ApplyMigrationsAsync<TContext>(this WebApplication app)
        where TContext : DbContext
    {
        if (app.Configuration.GetValue<bool>("SkipMigrations"))
            return;

        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<TContext>>();

        var retries = 5;
        while (retries > 0)
        {
            try
            {
                await dbContext.Database.MigrateAsync();
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
}
