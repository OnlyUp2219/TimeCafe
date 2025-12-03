namespace Venue.TimeCafe.Test.Helpers;

public abstract class BaseCqrsTest : IDisposable
{
    protected readonly ApplicationDbContext Context;
    protected readonly Mock<IDistributedCache> CacheMock;
    protected readonly Mock<ILogger<TariffRepository>> TariffLoggerMock;
    protected readonly Mock<ILogger<PromotionRepository>> PromotionLoggerMock;
    protected readonly Mock<ILogger<ThemeRepository>> ThemeLoggerMock;
    protected readonly Mock<ILogger<VisitRepository>> VisitLoggerMock;
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

        CacheMock = new Mock<IDistributedCache>();
        TariffLoggerMock = new Mock<ILogger<TariffRepository>>();
        PromotionLoggerMock = new Mock<ILogger<PromotionRepository>>();
        ThemeLoggerMock = new Mock<ILogger<ThemeRepository>>();
        VisitLoggerMock = new Mock<ILogger<VisitRepository>>();

        SetupDefaultCacheMocks();

        TariffRepository = new TariffRepository(Context, CacheMock.Object, TariffLoggerMock.Object);
        PromotionRepository = new PromotionRepository(Context, CacheMock.Object, PromotionLoggerMock.Object);
        ThemeRepository = new ThemeRepository(Context, CacheMock.Object, ThemeLoggerMock.Object);
        VisitRepository = new VisitRepository(Context, CacheMock.Object, VisitLoggerMock.Object);
    }

    private void SetupDefaultCacheMocks()
    {
        CacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        CacheMock.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        CacheMock.Setup(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    protected async Task<Tariff> SeedTariffAsync(string name = "Test Tariff", decimal price = 100m)
    {
        var tariff = new Tariff
        {
            Name = name,
            PricePerMinute = price,
            BillingType = BillingType.PerMinute,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
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
            ValidFrom = DateTime.UtcNow,
            ValidTo = DateTime.UtcNow.AddDays(30),
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow
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

    protected async Task<Visit> SeedVisitAsync(string userId = "user123", int? tariffId = null)
    {
        var tariff = tariffId.HasValue
            ? await Context.Tariffs.FindAsync(tariffId.Value)
            : await SeedTariffAsync();

        var visit = new Visit
        {
            UserId = userId,
            TariffId = tariff!.TariffId,
            EntryTime = DateTime.UtcNow,
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
        if (_disposed) return;

        if (disposing)
        {
            Context?.Dispose();
        }

        _disposed = true;
    }
}
