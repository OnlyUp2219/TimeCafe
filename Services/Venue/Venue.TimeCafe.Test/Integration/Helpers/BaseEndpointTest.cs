using Microsoft.Extensions.Caching.Hybrid;
using Venue.TimeCafe.Domain.Constants;

namespace Venue.TimeCafe.Test.Integration.Helpers;

public abstract class BaseEndpointTest(IntegrationApiFactory factory) : IClassFixture<IntegrationApiFactory>, IAsyncLifetime
{
    protected readonly IntegrationApiFactory Factory = factory;

    protected HttpClient Client => field ??= Factory.CreateClient();

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
        context.Resources.RemoveRange(context.Resources);
        context.ResourceGroups.RemoveRange(context.ResourceGroups);
        await context.SaveChangesAsync();

        var hybridCache = scope.ServiceProvider.GetRequiredService<HybridCache>();
        await hybridCache.RemoveByTagAsync(CacheTags.Tariffs);
        await hybridCache.RemoveByTagAsync(CacheTags.Themes);
        await hybridCache.RemoveByTagAsync(CacheTags.Promotions);
        await hybridCache.RemoveByTagAsync(CacheTags.Visits);
        await hybridCache.RemoveByTagAsync(CacheTags.Resources);
        await hybridCache.RemoveByTagAsync(CacheTags.ResourceGroups);

        await hybridCache.RemoveAsync(CacheKeys.Tariff_All);
        await hybridCache.RemoveAsync(CacheKeys.Tariff_Active);
        await hybridCache.RemoveAsync(CacheKeys.Theme_All);
        await hybridCache.RemoveAsync(CacheKeys.Visit_Active);
        await hybridCache.RemoveAsync(CacheKeys.Promotion_All);
        await hybridCache.RemoveAsync(CacheKeys.Promotion_Active);
        await hybridCache.RemoveAsync(CacheKeys.Resource_All);
        await hybridCache.RemoveAsync(CacheKeys.ResourceGroup_All);
        await hybridCache.RemoveAsync(CacheKeys.Visit_Pending(1, 20));
        await hybridCache.RemoveAsync(CacheKeys.Visit_Pending(1, 2));
        await hybridCache.RemoveAsync(CacheKeys.Visit_Pending(100, 20));

        var connectionMultiplexer = scope.ServiceProvider.GetService<StackExchange.Redis.IConnectionMultiplexer>();
        if (connectionMultiplexer != null)
        {
            var endpoints = connectionMultiplexer.GetEndPoints();
            foreach (var endpoint in endpoints)
            {
                var server = connectionMultiplexer.GetServer(endpoint);
                await server.FlushDatabaseAsync();
            }
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
            Description = "Полное описание тестового тарифа с подробностями",
            Summary = "Краткая информация о тарифе",
            Features = new List<string> { "Бесплатный кофе", "Настольные игры", "Wi-Fi" },
            AudienceTags = new List<string> { "Тест", "Гости" },
            MinSessionMinutes = 15,
            RoundingRule = "FiveMinutes",
            MaxGuests = 4,
            CancellationPolicy = "Гибкие условия отмены",
            IsRecommended = true,
            SortOrder = 1,
            CreatedAt = DateTimeOffset.UtcNow,
            LastModified = DateTimeOffset.UtcNow
        };

        context.Tariffs.Add(tariff);
        await context.SaveChangesAsync();
        return tariff;
    }

    protected async Task<Promotion> SeedPromotionAsync(
        string name = "Test Promotion",
        int DiscountPercentage = 10,
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
            DiscountPercent = DiscountPercentage,
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

    protected async Task<Visit> SeedVisitAsync(Guid? userId = null, Guid? tariffId = null, bool isActive = true, VisitStatus? status = null, Guid? resourceId = null)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var tariff = tariffId.HasValue
            ? await context.Tariffs.FindAsync(tariffId.Value)
                ?? throw new InvalidOperationException($"Tariff with ID {tariffId} not found")
            : await SeedTariffAsync();

        var visitStatus = status ?? (isActive ? VisitStatus.Active : VisitStatus.Completed);
        var visit = new Visit
        {
            UserId = userId,
            TariffId = tariff.TariffId,
            ResourceId = resourceId,
            EntryTime = DateTimeOffset.UtcNow,
            Status = visitStatus,
            ExitTime = visitStatus == VisitStatus.Active || visitStatus == VisitStatus.Pending ? null : DateTimeOffset.UtcNow.AddMinutes(30),
            CalculatedCost = visitStatus == VisitStatus.Active || visitStatus == VisitStatus.Pending ? null : 100m
        };

        context.Visits.Add(visit);
        await context.SaveChangesAsync();
        return visit;
    }

    protected async Task<ResourceGroup> SeedResourceGroupAsync(string name = "Test Group", int capacity = 10, string? description = null, bool isActive = true)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var group = ResourceGroup.Create(null, name, description, capacity, isActive);

        context.ResourceGroups.Add(group);
        await context.SaveChangesAsync();
        return group;
    }

    protected async Task<Resource> SeedResourceAsync(Guid resourceGroupId, string name = "Test Resource", int capacity = 4, bool isActive = true)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var resource = Resource.Create(null, resourceGroupId, name, capacity, isActive);

        context.Resources.Add(resource);
        await context.SaveChangesAsync();
        return resource;
    }
}








