namespace BuildingBlocks.Extensions;

public static class DatabaseExtensions
{
    public static IServiceCollection AddPostgresDatabase<TContext>(this IServiceCollection services, IConfiguration configuration)
    where TContext : DbContext
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<TContext>((sp, options) =>
        {
            // TODO : убрать в проде.
            options.UseNpgsql(connectionString).ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));

            var auditInterceptor = sp.GetRequiredService<AuditSaveChangesInterceptor>();
            options.AddInterceptors(auditInterceptor);

        });

        return services;
    }

    public static IServiceCollection AddAuditDatabase<TContext>(this IServiceCollection services)
    where TContext : DbContext
    {
        services.AddScoped<AuditSaveChangesInterceptor>();

        Audit.EntityFramework.Configuration.Setup()
            .ForContext<TContext>(config => config
                .IncludeEntityObjects()
            )
            .UseOptOut()
            .IgnoreAny(type => type == typeof(InboxState)
                        || type == typeof(OutboxState)
                        || type == typeof(OutboxMessage)
                        || type.Name == "ApplicationUser"
                        || type.Name == "RefreshToken");

        Audit.Core.Configuration.AddCustomAction(ActionType.OnScopeCreated, auditEvent =>
        {
            var commandName = AuditCommandContext.CurrentCommandName ?? "UnknownCommand";
            auditEvent.SetCustomField("CommandName", commandName);
        });

        Audit.Core.Configuration.CreationPolicy = EventCreationPolicy.InsertOnEnd;

        return services;
    }

    public static void ConfigurePostgresConventions(this ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder
            .Properties<DateTime>()
            .HaveConversion<UtcDateTimeConverter>();

        configurationBuilder
            .Properties<DateTimeOffset>()
            .HaveConversion<UtcDateTimeOffsetConverter>();
    }

    public static async Task ApplyMigrationsAsync<TContext>(this WebApplication app)
    where TContext : DbContext
    {
        if (app.Configuration.GetValue<bool>("SkipMigrations"))
            return;

        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();
        await dbContext.Database.MigrateAsync();
    }

    private sealed class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
    {
        public UtcDateTimeConverter() : base(v => v.Kind == DateTimeKind.Utc ? v : v.ToUniversalTime(), v => DateTime.SpecifyKind(v, DateTimeKind.Utc)) { }
    }

    private sealed class UtcDateTimeOffsetConverter : ValueConverter<DateTimeOffset, DateTimeOffset>
    {
        public UtcDateTimeOffsetConverter() : base(v => v.ToUniversalTime(), v => v) { }
    }
}
