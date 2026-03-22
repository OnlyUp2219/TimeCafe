using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;

namespace UserProfile.TimeCafe.Test.Unit.RepositoriesTests.RepositoriesProfile;

public abstract class BaseRepositoryTest : IDisposable
{
    protected readonly ApplicationDbContext Context;
    protected readonly HybridCache HybridCache;
    protected readonly UserRepositories Repository;
    protected readonly List<Profile> TestProfiles;
    private bool _disposed;

    protected BaseRepositoryTest()
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

        Repository = new UserRepositories(Context, HybridCache);

        TestProfiles =
        [
            new() { UserId = Guid.NewGuid(), FirstName = "John", CreatedAt = DateTimeOffset.UtcNow },
            new() { UserId = Guid.NewGuid(), FirstName = "Jane", CreatedAt = DateTimeOffset.UtcNow.AddDays(-1) },
            new() { UserId = Guid.NewGuid(), FirstName = "Bob", CreatedAt = DateTimeOffset.UtcNow.AddDays(-2) }
        ];
    }

    protected async Task SeedProfilesAsync()
    {
        Context.Profiles.AddRange(TestProfiles);
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
