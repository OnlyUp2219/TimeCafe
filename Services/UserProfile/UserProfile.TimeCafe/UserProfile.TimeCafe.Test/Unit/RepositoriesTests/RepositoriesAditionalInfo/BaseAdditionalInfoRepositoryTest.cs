namespace UserProfile.TimeCafe.Test.Unit.RepositoriesTests.RepositoriesAditionalInfo;

public abstract class BaseAdditionalInfoRepositoryTest : IDisposable
{
    protected readonly ApplicationDbContext Context;
    protected readonly Mock<IDistributedCache> CacheMock;
    protected readonly Mock<ILogger<AdditionalInfoRepository>> CacheLoggerMock;
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

        CacheMock = new Mock<IDistributedCache>();
        CacheLoggerMock = new Mock<ILogger<AdditionalInfoRepository>>();
        Repository = new AdditionalInfoRepository(Context, CacheMock.Object, CacheLoggerMock.Object);

        TestInfos = new List<AdditionalInfo>
        {
            new() { InfoId = 1, UserId = "U1", InfoText = "First info", CreatedAt = DateTime.UtcNow.AddMinutes(-10) },
            new() { InfoId = 2, UserId = "U1", InfoText = "Second info", CreatedAt = DateTime.UtcNow.AddMinutes(-5) },
            new() { InfoId = 3, UserId = "U2", InfoText = "Another user info", CreatedAt = DateTime.UtcNow.AddMinutes(-2) }
        };
    }

    protected async Task SeedAsync()
    {
        Context.AdditionalInfos.AddRange(TestInfos);
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
