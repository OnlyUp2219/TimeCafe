namespace UserProfile.TimeCafe.Test.Unit.RepositoriesTests.RepositoriesProfile;

public class UpdateProfileRepositoryTests : BaseRepositoryTest
{
    [Fact]
    public async Task Repository_UpdateProfile_Should_UpdateProfileAndInvalidateCache()
    {
        // Arrange
        await SeedProfilesAsync();
        var userId = TestProfiles[0].UserId;
        var updatedProfile = new Profile { UserId = userId, FirstName = "Jane" };
        SetupCacheOperations();
        CacheMock.Setup(c => c.GetAsync(CacheKeys.ProfilePagesVersion(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(System.Text.Encoding.UTF8.GetBytes("1"));

        // Act
        var result = await Repository.UpdateProfileAsync(updatedProfile, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();

        var dbProfile = await Context.Profiles.FindAsync(userId);
        dbProfile!.FirstName.Should().Be("Jane");

        CacheMock.Verify(c => c.RemoveAsync(CacheKeys.Profile_All, It.IsAny<CancellationToken>()), Times.Once());
        CacheMock.Verify(c => c.RemoveAsync(CacheKeys.Profile_ById(userId.ToString()), It.IsAny<CancellationToken>()), Times.Once());
        CacheMock.Verify(c => c.SetAsync(CacheKeys.ProfilePagesVersion(),
            It.Is<byte[]>(b => System.Text.Encoding.UTF8.GetString(b) == "2"),
            It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task Repository_UpdateProfile_Should_UpdateAllFields()
    {
        // Arrange
        await SeedProfilesAsync();
        var userId = TestProfiles[0].UserId;
        var updatedProfile = new Profile
        {
            UserId = userId,
            FirstName = "Updated",
            LastName = "User",
            Gender = Gender.Female,
            ProfileStatus = ProfileStatus.Completed,
            BirthDate = new DateOnly(1990, 1, 1)
        };
        SetupCacheOperations();
        CacheMock.Setup(c => c.GetAsync(CacheKeys.ProfilePagesVersion(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(System.Text.Encoding.UTF8.GetBytes("1"));

        // Act
        var result = await Repository.UpdateProfileAsync(updatedProfile, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.FirstName.Should().Be("Updated");
        result.LastName.Should().Be("User");
        result.Gender.Should().Be(Gender.Female);
        result.ProfileStatus.Should().Be(ProfileStatus.Completed);
        result.BirthDate.Should().Be(new DateOnly(1990, 1, 1));
    }

    [Fact]
    public async Task Repository_UpdateProfile_Should_ReturnNull_WhenProfileNotExists()
    {
        // Arrange
        var nonExistentProfile = new Profile { UserId = Guid.NewGuid(), FirstName = "Test" };
        SetupCacheOperations();

        // Act
        var result = await Repository.UpdateProfileAsync(nonExistentProfile, CancellationToken.None);

        // Assert
        result.Should().BeNull();
        CacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task Repository_UpdateProfile_Should_InvalidateSpecificCacheKeys()
    {
        // Arrange
        await SeedProfilesAsync();
        var userId = TestProfiles[1].UserId;
        var updatedProfile = new Profile { UserId = userId, FirstName = "UpdatedJane" };
        SetupCacheOperations();
        CacheMock.Setup(c => c.GetAsync(CacheKeys.ProfilePagesVersion(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(System.Text.Encoding.UTF8.GetBytes("5"));

        // Act
        await Repository.UpdateProfileAsync(updatedProfile, CancellationToken.None);

        // Assert
        CacheMock.Verify(c => c.RemoveAsync(CacheKeys.Profile_All, It.IsAny<CancellationToken>()), Times.Once());
        CacheMock.Verify(c => c.RemoveAsync(CacheKeys.Profile_ById(userId.ToString()), It.IsAny<CancellationToken>()), Times.Once());
        CacheMock.Verify(c => c.SetAsync(CacheKeys.ProfilePagesVersion(),
            It.Is<byte[]>(b => System.Text.Encoding.UTF8.GetString(b) == "6"),
            It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Once());
    }
}
