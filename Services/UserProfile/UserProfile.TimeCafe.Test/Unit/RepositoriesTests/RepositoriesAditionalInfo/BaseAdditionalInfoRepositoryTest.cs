using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;

namespace UserProfile.TimeCafe.Test.Unit.RepositoriesTests.RepositoriesAditionalInfo;

public abstract class BaseAdditionalInfoRepositoryTest : IDisposable
{
    protected readonly ApplicationDbContext Context;
    protected readonly HybridCache HybridCache;
    protected readonly AdditionalInfoRepository Repository;
    protected readonly List<AdditionalInfo> TestInfos;
    private bool _disposed;

    protected BaseAdditionalInfoRepositoryTest()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"AddInfoDb_{Guid.NewGuid()}")
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

        Repository = new AdditionalInfoRepository(Context, HybridCache);

        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();

        TestInfos =
        [
            new() { InfoId = Guid.NewGuid(), UserId = userId1, InfoText = "First info", CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-10) },
            new() { InfoId = Guid.NewGuid(), UserId = userId1, InfoText = "Second info", CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-5) },
            new() { InfoId = Guid.NewGuid(), UserId = userId2, InfoText = "Another user info", CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-2) }
        ];
    }

    protected async Task SeedAsync()
    {
        Context.AdditionalInfos.AddRange(TestInfos);
        await Context.SaveChangesAsync();
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
