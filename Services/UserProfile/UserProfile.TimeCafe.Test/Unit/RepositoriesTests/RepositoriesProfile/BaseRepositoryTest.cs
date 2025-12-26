namespace UserProfile.TimeCafe.Test.Unit.RepositoriesTests.RepositoriesProfile;

public abstract class BaseRepositoryTest : IDisposable
{
    protected readonly ApplicationDbContext Context;
    protected readonly Mock<IDistributedCache> CacheMock;
    protected readonly Mock<ILogger<UserRepositories>> LoggerMock;
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

        CacheMock = new Mock<IDistributedCache>();
        LoggerMock = new Mock<ILogger<UserRepositories>>();
        Repository = new UserRepositories(Context, CacheMock.Object, LoggerMock.Object);

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

    protected void SetupCache<T>(string key, T value)
    {
        var json = JsonSerializer.Serialize(value, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var bytes = System.Text.Encoding.UTF8.GetBytes(json);
        CacheMock.Setup(c => c.GetAsync(key, It.IsAny<CancellationToken>()))
            .ReturnsAsync(bytes);
    }

    protected void SetupCacheNoData()
    {
        CacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);
    }

    protected void SetupCacheOperations()
    {
        CacheMock.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        CacheMock.Setup(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
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
