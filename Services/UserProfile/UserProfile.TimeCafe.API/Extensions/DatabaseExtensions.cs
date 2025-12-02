namespace UserProfile.TimeCafe.API.Extensions;

public static class DatabaseExtensions
{
    public static IServiceCollection AddUserProfileDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<ApplicationDbContext>(op => op.UseNpgsql(connectionString));

        return services;
    }

    public static async Task ApplyMigrationsAsync(this WebApplication app)
    {
        if (app.Configuration.GetValue<bool>("SkipMigrations"))
            return;

        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();
    }
}
