using Microsoft.Extensions.Caching.Hybrid;
using Venue.TimeCafe.Domain.Constants;

namespace Venue.TimeCafe.Test.Integration.Helpers;

public abstract class BaseEndpointTest(IntegrationApiFactory factory) : IClassFixture<IntegrationApiFactory>, IAsyncLifetime
{
    protected readonly IntegrationApiFactory Factory = factory;
    private HttpClient? _client;
    protected HttpClient Client => _client ??= Factory.CreateClient();

    public Task InitializeAsync()
    {
        if (!string.IsNullOrWhiteSpace(IntegrationApiFactory.InfrastructureUnavailableReason))
        {
            throw new InvalidOperationException(IntegrationApiFactory.InfrastructureUnavailableReason);
        }

        return Task.CompletedTask;
    }

    public Task DisposeAsync() => Task.CompletedTask;

    protected async Task ClearDatabaseAndCacheAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        context.Themes.RemoveRange(context.Themes);
        context.Tariffs.RemoveRange(context.Tariffs);
        context.Promotions.RemoveRange(context.Promotions);
        context.Visits.RemoveRange(context.Visits);
        await context.SaveChangesAsync();

        var hybridCache = scope.ServiceProvider.GetRequiredService<HybridCache>();
        await hybridCache.RemoveByTagAsync(CacheTags.Tariffs);
        await hybridCache.RemoveByTagAsync(CacheTags.Themes);
        await hybridCache.RemoveByTagAsync(CacheTags.Promotions);
        await hybridCache.RemoveByTagAsync(CacheTags.Visits);

        await hybridCache.RemoveAsync(CacheKeys.Tariff_All);
        await hybridCache.RemoveAsync(CacheKeys.Tariff_Active);
        await hybridCache.RemoveAsync(CacheKeys.Theme_All);
        await hybridCache.RemoveAsync(CacheKeys.Visit_Active);
        await hybridCache.RemoveAsync(CacheKeys.Promotion_All);
        await hybridCache.RemoveAsync(CacheKeys.Promotion_Active);
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

        var hybridCache = scope.ServiceProvider.GetRequiredService<HybridCache>();
        await hybridCache.RemoveByTagAsync(CacheTags.Themes);

        return theme;
    }

    protected async Task<Visit> SeedVisitAsync(Guid? userId = null, Guid? tariffId = null, bool isActive = true)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var tariff = tariffId.HasValue
            ? await context.Tariffs.FindAsync(tariffId.Value)
                ?? throw new InvalidOperationException($"Tariff with ID {tariffId} not found")
            : await SeedTariffAsync();

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
