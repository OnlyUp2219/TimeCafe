using Venue.TimeCafe.Domain.Constants;

namespace Venue.TimeCafe.Test.Integration.Helpers;

public abstract class BaseEndpointTest(IntegrationApiFactory factory) : IClassFixture<IntegrationApiFactory>
{
    protected readonly HttpClient Client = factory.CreateClient();
    protected readonly IntegrationApiFactory Factory = factory;

    protected async Task ClearDatabaseAndCacheAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        context.Themes.RemoveRange(context.Themes);
        context.Tariffs.RemoveRange(context.Tariffs);
        context.Promotions.RemoveRange(context.Promotions);
        context.Visits.RemoveRange(context.Visits);
        await context.SaveChangesAsync();

        try
        {
            // Дополнительно очищаем кэш через IDistributedCache для надежности
            var distCache = scope.ServiceProvider.GetService<IDistributedCache>();
            if (distCache != null)
            {
                await distCache.RemoveAsync(CacheKeys.Theme_All);
            }

            var connectionMultiplexer = scope.ServiceProvider.GetService<StackExchange.Redis.IConnectionMultiplexer>();
            if (connectionMultiplexer != null)
            {
                var db = connectionMultiplexer.GetDatabase();
                var server = connectionMultiplexer.GetServer(connectionMultiplexer.GetEndPoints().First());

                var keys = server.Keys(pattern: "venue:*").ToArray();
                if (keys.Length > 0)
                {
                    await db.KeyDeleteAsync(keys);
                }
            }
        }
        catch
        {
            // Redis может быть не доступен в тестах, игнорируем
        }
    }

    protected async Task<Tariff> SeedTariffAsync(string name = "Test Tariff", decimal price = 100m, BillingType billingType = BillingType.PerMinute, bool isActive = true)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var tariff = new Tariff
        {
            Name = name,
            PricePerMinute = price,
            BillingType = billingType,
            IsActive = isActive,
            CreatedAt = DateTimeOffset.UtcNow
        };

        context.Tariffs.Add(tariff);
        await context.SaveChangesAsync();
        return tariff;
    }

    protected async Task<Promotion> SeedPromotionAsync(
        string name = "Test Promotion",
        int discountPercentage = 10,
        bool isActive = true,
        DateTimeOffset? validFrom = null,
        DateTimeOffset? validTo = null)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var promotion = new Promotion
        {
            Name = name,
            Description = "Test Description",
            DiscountPercent = discountPercentage,
            ValidFrom = validFrom ?? DateTimeOffset.UtcNow,
            ValidTo = validTo ?? DateTimeOffset.UtcNow.AddDays(30),
            IsActive = isActive,
            CreatedAt = DateTimeOffset.UtcNow
        };

        context.Promotions.Add(promotion);
        await context.SaveChangesAsync();
        return promotion;
    }

    protected async Task<Theme> SeedThemeAsync(string name = "Test Theme")
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var theme = new Theme
        {
            Name = name,
            Emoji = "🎨",
            Colors = "{\"primary\":\"#000000\",\"secondary\":\"#FFFFFF\"}"
        };

        context.Themes.Add(theme);
        await context.SaveChangesAsync();
        return theme;
    }

    protected async Task<Visit> SeedVisitAsync(Guid? userId = null, Guid? tariffId = null, bool isActive = true)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        Tariff tariff;
        if (tariffId.HasValue)
        {
            tariff = await context.Tariffs.FindAsync(tariffId.Value)
                ?? throw new InvalidOperationException($"Tariff with ID {tariffId} not found");
        }
        else
        {
            tariff = await SeedTariffAsync();
        }

        var visit = new Visit
        {
            UserId = userId ?? Guid.NewGuid(),
            TariffId = tariff.TariffId,
            EntryTime = DateTimeOffset.UtcNow,
            Status = isActive ? VisitStatus.Active : VisitStatus.Completed,
            ExitTime = isActive ? null : DateTimeOffset.UtcNow.AddMinutes(30),
            CalculatedCost = isActive ? null : 100m
        };

        context.Visits.Add(visit);
        await context.SaveChangesAsync();
        return visit;
    }
}
