using static UserProfile.TimeCafe.Test.Integration.Helpers.TestData;

namespace UserProfile.TimeCafe.Test.Unit.RepositoriesTests.RepositoriesProfile;

public class GetProfilesRepositoryTests : BaseRepositoryTest
{
    [Fact]
    public async Task Repository_GetAllProfiles_Should_ReturnAllProfiles_WhenNoCache()
    {
        // Arrange
        await SeedProfilesAsync();
        SetupCacheNoData();
        SetupCacheOperations();

        // Act
        var result = await Repository.GetAllProfilesAsync(CancellationToken.None);

        // Assert
        result.Should().HaveCount(TestProfiles.Count)
            .And.BeEquivalentTo(TestProfiles, options => options
                .Including(p => p.UserId)
                .Including(p => p.FirstName));

        CacheMock.Verify(c => c.GetAsync(CacheKeys.Profile_All, It.IsAny<CancellationToken>()), Times.Once());
        CacheMock.Verify(c => c.SetAsync(CacheKeys.Profile_All, It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task Repository_GetAllProfiles_Should_ReturnCachedProfiles_WhenCacheExists()
    {
        // Arrange
        SetupCache(CacheKeys.Profile_All, TestProfiles);
        SetupCacheOperations();

        // Act
        var result = await Repository.GetAllProfilesAsync(CancellationToken.None);

        // Assert
        result.Should().HaveCount(TestProfiles.Count)
            .And.BeEquivalentTo(TestProfiles, options => options
                .Including(p => p.UserId)
                .Including(p => p.FirstName));

        CacheMock.Verify(c => c.GetAsync(CacheKeys.Profile_All, It.IsAny<CancellationToken>()), Times.Once());
        CacheMock.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task Repository_GetProfileById_Should_ReturnProfile_WhenExistsAndNoCache()
    {
        // Arrange
        await SeedProfilesAsync();
        SetupCacheNoData();
        SetupCacheOperations();

        // Act
        var result = await Repository.GetProfileByIdAsync(TestProfiles[0].UserId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull()
            .And.BeEquivalentTo(TestProfiles[0], options => options
                .Including(p => p.UserId)
                .Including(p => p.FirstName));

        CacheMock.Verify(c => c.GetAsync(CacheKeys.Profile_ById(TestProfiles[0].UserId.ToString()), It.IsAny<CancellationToken>()), Times.Once());
        CacheMock.Verify(c => c.SetAsync(CacheKeys.Profile_ById(TestProfiles[0].UserId.ToString()), It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task Repository_GetProfileById_Should_ReturnCachedProfile_WhenCacheExists()
    {
        // Arrange
        var profile = TestProfiles[0];
        SetupCache(CacheKeys.Profile_ById(profile.UserId.ToString()), profile);
        SetupCacheOperations();

        // Act
        var result = await Repository.GetProfileByIdAsync(profile.UserId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull()
            .And.BeEquivalentTo(profile, options => options
                .Including(p => p.UserId)
                .Including(p => p.FirstName));

        CacheMock.Verify(c => c.GetAsync(CacheKeys.Profile_ById(profile.UserId.ToString()), It.IsAny<CancellationToken>()), Times.Once());
        CacheMock.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task Repository_GetProfileById_Should_ReturnNull_WhenNotExists()
    {
        // Arrange
        SetupCacheNoData();
        SetupCacheOperations();
        var nonexistentId = Guid.NewGuid();

        // Act
        var result = await Repository.GetProfileByIdAsync(nonexistentId, CancellationToken.None);

        // Assert
        result.Should().BeNull();
        CacheMock.Verify(c => c.GetAsync(CacheKeys.Profile_ById(nonexistentId.ToString()), It.IsAny<CancellationToken>()), Times.Once());
        CacheMock.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task Repository_GetTotalPage_Should_ReturnCorrectCount()
    {
        // Arrange
        await SeedProfilesAsync();

        // Act
        var result = await Repository.GetTotalPageAsync(CancellationToken.None);

        // Assert
        result.Should().Be(TestProfiles.Count);
    }

    [Fact]
    public async Task Repository_GetTotalPage_Should_ReturnZero_WhenNoProfiles()
    {
        // Arrange - контекст пустой

        // Act
        var result = await Repository.GetTotalPageAsync(CancellationToken.None);

        // Assert
        result.Should().Be(0);
    }
}
