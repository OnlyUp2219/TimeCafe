namespace UserProfile.TimeCafe.Test.Unit.RepositoriesTests.RepositoriesAditionalInfo;

public class UpdateAdditionalInfoRepositoryTests : BaseAdditionalInfoRepositoryTest
{
    [Fact]
    public async Task Repository_UpdateAdditionalInfo_Should_UpdateAndInvalidateCaches()
    {
        // Arrange
        await SeedAsync();
        var upd = new AdditionalInfo { InfoId = 2, UserId = "U1", InfoText = "Updated second" };
        SetupCacheOperations();

        // Act
        var result = await Repository.UpdateAdditionalInfoAsync(upd, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        var db = await Context.AdditionalInfos.FindAsync(2);
        db!.InfoText.Should().Be("Updated second");

        CacheMock.Verify(c => c.RemoveAsync(CacheKeys.AdditionalInfo_All, It.IsAny<CancellationToken>()), Times.Once());
        CacheMock.Verify(c => c.RemoveAsync(CacheKeys.AdditionalInfo_ById(2), It.IsAny<CancellationToken>()), Times.Once());
        CacheMock.Verify(c => c.RemoveAsync(CacheKeys.AdditionalInfo_ByUserId("U1"), It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task Repository_UpdateAdditionalInfo_Should_ReturnNull_WhenNotExists()
    {
        // Arrange
        var upd = new AdditionalInfo { InfoId = 999, UserId = "U1", InfoText = "Ghost" };
        SetupCacheOperations();

        // Act
        var result = await Repository.UpdateAdditionalInfoAsync(upd, CancellationToken.None);

        // Assert
        result.Should().BeNull();
        CacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never());
    }
}
