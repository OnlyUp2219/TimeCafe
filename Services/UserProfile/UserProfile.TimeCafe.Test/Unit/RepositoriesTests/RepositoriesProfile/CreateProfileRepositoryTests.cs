using static UserProfile.TimeCafe.Test.Integration.Helpers.TestData;

namespace UserProfile.TimeCafe.Test.Unit.RepositoriesTests.RepositoriesProfile;

public class CreateProfileRepositoryTests : BaseRepositoryTest
{
    [Fact]
    public async Task Repository_CreateProfile_Should_AddProfileAndInvalidateCache()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var profile = new Profile { UserId = userId, FirstName = "Alice" };
        SetupCacheOperations();
        CacheMock.Setup(c => c.GetAsync(CacheKeys.ProfilePagesVersion(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(System.Text.Encoding.UTF8.GetBytes("1"));

        // Act
        var result = await Repository.CreateProfileAsync(profile, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();

        var dbProfile = await Context.Profiles.FindAsync(userId);
        dbProfile.Should().NotBeNull()
            .And.BeEquivalentTo(profile, options => options
                .Including(p => p.UserId)
                .Including(p => p.FirstName));

        CacheMock.Verify(c => c.RemoveAsync(CacheKeys.Profile_All, It.IsAny<CancellationToken>()), Times.Once());
        CacheMock.Verify(c => c.SetAsync(CacheKeys.ProfilePagesVersion(),
            It.Is<byte[]>(b => System.Text.Encoding.UTF8.GetString(b) == "2"),
            It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task Repository_CreateProfile_Should_ReturnCreatedProfile()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var profile = new Profile
        {
            UserId = userId,
            FirstName = "Bob",
            LastName = "Smith",
            Gender = Gender.Male,
            ProfileStatus = ProfileStatus.Pending
        };
        SetupCacheOperations();
        CacheMock.Setup(c => c.GetAsync(CacheKeys.ProfilePagesVersion(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(System.Text.Encoding.UTF8.GetBytes("1"));

        // Act
        var result = await Repository.CreateProfileAsync(profile, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.UserId.Should().Be(userId);
        result.FirstName.Should().Be("Bob");
        result.LastName.Should().Be("Smith");
        result.Gender.Should().Be(Gender.Male);
    }

    [Fact]
    public async Task Repository_CreateEmpty_Should_CreateEmptyProfileAndInvalidateCache()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupCacheOperations();
        CacheMock.Setup(c => c.GetAsync(CacheKeys.ProfilePagesVersion(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(System.Text.Encoding.UTF8.GetBytes("1"));

        // Act
        await Repository.CreateEmptyAsync(userId, CancellationToken.None);

        // Assert
        var dbProfile = await Context.Profiles.FindAsync(userId);
        dbProfile.Should().NotBeNull();
        dbProfile!.UserId.Should().Be(userId);
        dbProfile.FirstName.Should().BeEmpty();
        dbProfile.ProfileStatus.Should().Be(ProfileStatus.Pending);

        CacheMock.Verify(c => c.RemoveAsync(CacheKeys.Profile_All, It.IsAny<CancellationToken>()), Times.Once());
        CacheMock.Verify(c => c.SetAsync(CacheKeys.ProfilePagesVersion(),
            System.Text.Encoding.UTF8.GetBytes("2"),
            It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task Repository_CreateEmpty_Should_DoNothing_WhenProfileExists()
    {
        // Arrange
        await SeedProfilesAsync();
        var firstProfile = TestProfiles[0];

        // Act
        await Repository.CreateEmptyAsync(firstProfile.UserId, CancellationToken.None);

        // Assert
        var dbProfiles = await Context.Profiles.ToListAsync();
        dbProfiles.Should().HaveCount(TestProfiles.Count);
        dbProfiles.First(p => p.UserId == firstProfile.UserId).FirstName.Should().Be("John");

        CacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never());
        CacheMock.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task Repository_CreateProfile_Should_IncrementVersionFromZero()
    {
        // Arrange
        var profile = new Profile { UserId = Guid.NewGuid(), FirstName = "Test" };
        SetupCacheOperations();
        CacheMock.Setup(c => c.GetAsync(CacheKeys.ProfilePagesVersion(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null); // Версия отсутствует

        // Act
        await Repository.CreateProfileAsync(profile, CancellationToken.None);

        // Assert
        CacheMock.Verify(c => c.SetAsync(CacheKeys.ProfilePagesVersion(),
            It.Is<byte[]>(b => System.Text.Encoding.UTF8.GetString(b) == "2"),
            It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Once());
    }
}
