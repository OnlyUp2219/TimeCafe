using static UserProfile.TimeCafe.Test.Integration.Helpers.TestData;

namespace UserProfile.TimeCafe.Test.Unit.RepositoriesTests.RepositoriesProfile;

public class DeleteProfileRepositoryTests : BaseRepositoryTest
{
    [Fact]
    public async Task Repository_DeleteProfile_Should_RemoveProfileAndInvalidateCache()
    {
        // Arrange
        await SeedProfilesAsync();
        SetupCacheOperations();
        CacheMock.Setup(c => c.GetAsync(CacheKeys.ProfilePagesVersion(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(System.Text.Encoding.UTF8.GetBytes("1"));

        // Act
        await Repository.DeleteProfileAsync(TestProfiles[0].UserId, CancellationToken.None);

        // Assert
        var dbProfile = await Context.Profiles.FindAsync(TestProfiles[0].UserId);
        dbProfile.Should().BeNull();

        CacheMock.Verify(c => c.RemoveAsync(CacheKeys.Profile_All, It.IsAny<CancellationToken>()), Times.Once());
        CacheMock.Verify(c => c.RemoveAsync(CacheKeys.Profile_ById(TestProfiles[0].UserId.ToString()), It.IsAny<CancellationToken>()), Times.Once());
        CacheMock.Verify(c => c.SetAsync(CacheKeys.ProfilePagesVersion(),
            It.Is<byte[]>(b => System.Text.Encoding.UTF8.GetString(b) == "2"),
            It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task Repository_DeleteProfile_Should_DoNothing_WhenProfileNotExists()
    {
        // Arrange
        SetupCacheOperations();

        // Act
        await Repository.DeleteProfileAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        CacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never());
        CacheMock.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task Repository_DeleteProfile_Should_ReduceTotalCount()
    {
        // Arrange
        await SeedProfilesAsync();
        SetupCacheOperations();
        CacheMock.Setup(c => c.GetAsync(CacheKeys.ProfilePagesVersion(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(System.Text.Encoding.UTF8.GetBytes("1"));

        var countBefore = await Repository.GetTotalPageAsync(CancellationToken.None);

        // Act
        await Repository.DeleteProfileAsync(TestProfiles[0].UserId, CancellationToken.None);

        // Assert
        var countAfter = await Repository.GetTotalPageAsync(CancellationToken.None);
        countAfter.Should().Be(countBefore - 1);
    }

    [Fact]
    public async Task Repository_DeleteProfile_Should_IncrementVersion()
    {
        // Arrange
        await SeedProfilesAsync();
        SetupCacheOperations();
        CacheMock.Setup(c => c.GetAsync(CacheKeys.ProfilePagesVersion(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(System.Text.Encoding.UTF8.GetBytes("10"));

        // Act
        await Repository.DeleteProfileAsync(TestProfiles[1].UserId, CancellationToken.None);

        // Assert
        CacheMock.Verify(c => c.SetAsync(CacheKeys.ProfilePagesVersion(),
            It.Is<byte[]>(b => System.Text.Encoding.UTF8.GetString(b) == "11"),
            It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task Repository_DeleteProfile_Should_RemoveCorrectProfile()
    {
        // Arrange
        await SeedProfilesAsync();
        SetupCacheOperations();
        CacheMock.Setup(c => c.GetAsync(CacheKeys.ProfilePagesVersion(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(System.Text.Encoding.UTF8.GetBytes("1"));

        // Act
        await Repository.DeleteProfileAsync(TestProfiles[1].UserId, CancellationToken.None);

        // Assert
        var profile1 = await Context.Profiles.FindAsync(TestProfiles[0].UserId);
        var profile2 = await Context.Profiles.FindAsync(TestProfiles[1].UserId);
        var profile3 = await Context.Profiles.FindAsync(TestProfiles[2].UserId);

        profile1.Should().NotBeNull();
        profile2.Should().BeNull();
        profile3.Should().NotBeNull();
    }
}
