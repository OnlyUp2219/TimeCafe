using Microsoft.Extensions.Caching.Hybrid;

namespace Venue.TimeCafe.Test.Helpers;

public abstract class BaseCqrsTest : IDisposable
{
    protected readonly ApplicationDbContext Context;
    protected readonly HybridCache HybridCache;
    protected readonly ITariffRepository TariffRepository;
    protected readonly IPromotionRepository PromotionRepository;
    protected readonly IThemeRepository ThemeRepository;
    protected readonly IVisitRepository VisitRepository;
    private bool _disposed;

    protected BaseCqrsTest()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        Context = new ApplicationDbContext(options);
        Context.Database.EnsureCreated();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDistributedMemoryCache();
#pragma warning disable EXTEXP0018
        services.AddHybridCache();
#pragma warning restore EXTEXP0018
        HybridCache = services.BuildServiceProvider().GetRequiredService<HybridCache>();

        TariffRepository = new TariffRepository(Context, HybridCache);
        PromotionRepository = new PromotionRepository(Context, HybridCache);
        ThemeRepository = new ThemeRepository(Context, HybridCache);
        VisitRepository = new VisitRepository(Context, HybridCache);
    }

    protected async Task<Tariff> SeedTariffAsync(string name = "Test Tariff", decimal price = 100m)
    {
        var tariff = new Tariff
        {
            Name = name,
            PricePerMinute = price,
            BillingType = BillingType.PerMinute,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        Context.Tariffs.Add(tariff);
        await Context.SaveChangesAsync();
        return tariff;
    }

    protected async Task<Promotion> SeedPromotionAsync(string name = "Test Promotion", decimal discount = 10m, bool isActive = true)
    {
        var promotion = new Promotion
        {
            Name = name,
            Description = "Test Description",
            DiscountPercent = discount,
            ValidFrom = DateTimeOffset.UtcNow,
            ValidTo = DateTimeOffset.UtcNow.AddDays(30),
            IsActive = isActive,
            CreatedAt = DateTimeOffset.UtcNow
        };

        Context.Promotions.Add(promotion);
        await Context.SaveChangesAsync();
        return promotion;
    }

    protected async Task<Theme> SeedThemeAsync(string name = "Test Theme", string? emoji = null, string? colors = null)
    {
        var theme = new Theme
        {
            Name = name,
            Emoji = emoji ?? "🎨",
            Colors = colors ?? "{\"primary\":\"#000000\",\"secondary\":\"#FFFFFF\"}"
        };

        Context.Themes.Add(theme);
        await Context.SaveChangesAsync();
        return theme;
    }

    protected async Task<Visit> SeedVisitAsync(Guid? userId = null, Guid? tariffId = null)
    {
        var tariff = tariffId.HasValue
            ? await Context.Tariffs.FindAsync(tariffId.Value)
            : await SeedTariffAsync();

        var visit = new Visit
        {
            UserId = userId ?? Guid.Parse("12345678-1234-1234-1234-123456789012"),
            TariffId = tariff!.TariffId,
            EntryTime = DateTimeOffset.UtcNow,
            Status = VisitStatus.Active
        };

        Context.Visits.Add(visit);
        await Context.SaveChangesAsync();
        return visit;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            Context?.Dispose();
        }

        _disposed = true;
    }
}

