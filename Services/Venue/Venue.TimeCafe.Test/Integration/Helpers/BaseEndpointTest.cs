namespace Venue.TimeCafe.Test.Integration.Helpers;

public abstract class BaseEndpointTest(IntegrationApiFactory factory) : IClassFixture<IntegrationApiFactory>
{
    protected readonly HttpClient Client = factory.CreateClient();
    protected readonly IntegrationApiFactory Factory = factory;

    protected async Task<Tariff> SeedTariffAsync(string name = "Test Tariff", decimal price = 100m, BillingType billingType = BillingType.PerMinute)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var tariff = new Tariff
        {
            Name = name,
            PricePerMinute = price,
            BillingType = billingType,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Tariffs.Add(tariff);
        await context.SaveChangesAsync();
        return tariff;
    }

    protected async Task<Promotion> SeedPromotionAsync(string name = "Test Promotion", int discountPercentage = 10)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var promotion = new Promotion
        {
            Name = name,
            Description = "Test Description",
            DiscountPercent = discountPercentage,
            ValidFrom = DateTime.UtcNow,
            ValidTo = DateTime.UtcNow.AddDays(30),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
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

    protected async Task<Visit> SeedVisitAsync(string userId = "user123", int? tariffId = null)
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
            UserId = userId,
            TariffId = tariff.TariffId,
            EntryTime = DateTime.UtcNow,
            Status = VisitStatus.Active
        };

        context.Visits.Add(visit);
        await context.SaveChangesAsync();
        return visit;
    }
}
