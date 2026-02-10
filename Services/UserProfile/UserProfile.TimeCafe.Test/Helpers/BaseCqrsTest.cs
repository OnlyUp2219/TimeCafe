namespace UserProfile.TimeCafe.Test.Helpers;

public abstract class BaseCqrsTest : IDisposable
{
    protected readonly ApplicationDbContext Context;
    protected readonly Mock<IDistributedCache> CacheMock;
    protected readonly Mock<ILogger<UserRepositories>> LoggerMock;
    protected readonly IUserRepositories Repository;
    private bool _disposed;

    protected BaseCqrsTest()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        Context = new ApplicationDbContext(options);
        Context.Database.EnsureCreated();

        CacheMock = new Mock<IDistributedCache>();
        LoggerMock = new Mock<ILogger<UserRepositories>>();
        Repository = new UserRepositories(Context, CacheMock.Object, LoggerMock.Object);

        SetupDefaultCacheMocks();
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

    protected async Task<Profile> SeedProfileAsync(Guid userId, string firstName = "Test", string lastName = "User")
    {
        var profile = new Profile
        {
            UserId = userId,
            FirstName = firstName,
            LastName = lastName,
            Gender = Gender.NotSpecified,
            ProfileStatus = ProfileStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow
        };

        Context.Profiles.Add(profile);
        await Context.SaveChangesAsync();
        return profile;
    }

    protected async Task<Profile> SeedProfileAsync(string userIdStr, string firstName = "Test", string lastName = "User")
    {
        return await SeedProfileAsync(Guid.Parse(userIdStr), firstName, lastName);
    }

    protected async Task ClearProfilesAsync()
    {
        Context.Profiles.RemoveRange(Context.Profiles);
        await Context.SaveChangesAsync();
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
