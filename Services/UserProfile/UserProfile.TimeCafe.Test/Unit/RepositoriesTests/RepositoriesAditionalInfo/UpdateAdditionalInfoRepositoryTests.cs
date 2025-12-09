using static UserProfile.TimeCafe.Test.Integration.Helpers.TestData;

namespace UserProfile.TimeCafe.Test.Unit.RepositoriesTests.RepositoriesAditionalInfo;

public class UpdateAdditionalInfoRepositoryTests : BaseAdditionalInfoRepositoryTest
{
    [Fact]
    public async Task Repository_UpdateAdditionalInfo_Should_UpdateAndInvalidateCaches()
    {
        // Arrange
        await SeedAsync();
        var infoId = TestInfos[1].InfoId;
        var userId = TestInfos[1].UserId;
        var upd = new AdditionalInfo { InfoId = infoId, UserId = userId, InfoText = "Updated second" };
        SetupCacheOperations();

        // Act
        var result = await Repository.UpdateAdditionalInfoAsync(upd, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        var db = await Context.AdditionalInfos.FindAsync(infoId);
        db!.InfoText.Should().Be("Updated second");

        CacheMock.Verify(c => c.RemoveAsync(CacheKeys.AdditionalInfo_All, It.IsAny<CancellationToken>()), Times.Once());
        CacheMock.Verify(c => c.RemoveAsync(CacheKeys.AdditionalInfo_ById(infoId.ToString()), It.IsAny<CancellationToken>()), Times.Once());
        CacheMock.Verify(c => c.RemoveAsync(CacheKeys.AdditionalInfo_ByUserId(userId.ToString()), It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task Repository_UpdateAdditionalInfo_Should_ReturnNull_WhenNotExists()
    {
        // Arrange
        var upd = new AdditionalInfo { InfoId = Guid.NewGuid(), UserId = TestInfos[0].UserId, InfoText = "Ghost" };
        SetupCacheOperations();

        // Act
        var result = await Repository.UpdateAdditionalInfoAsync(upd, CancellationToken.None);

        // Assert
        result.Should().BeNull();
        CacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never());
    }
}
