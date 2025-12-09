using static UserProfile.TimeCafe.Test.Integration.Helpers.TestData;

namespace UserProfile.TimeCafe.Test.Unit.RepositoriesTests.RepositoriesAditionalInfo;

public class DeleteAdditionalInfoRepositoryTests : BaseAdditionalInfoRepositoryTest
{
    [Fact]
    public async Task Repository_DeleteAdditionalInfo_Should_RemoveAndInvalidateCaches()
    {
        // Arrange
        await SeedAsync();
        var infoIdToDelete = TestInfos[0].InfoId;
        var userIdToDelete = TestInfos[0].UserId;
        SetupCacheOperations();

        // Act
        var ok = await Repository.DeleteAdditionalInfoAsync(infoIdToDelete, CancellationToken.None);

        // Assert
        ok.Should().BeTrue();
        var db = await Context.AdditionalInfos.FindAsync(infoIdToDelete);
        db.Should().BeNull();
        CacheMock.Verify(c => c.RemoveAsync(CacheKeys.AdditionalInfo_All, It.IsAny<CancellationToken>()), Times.Once());
        CacheMock.Verify(c => c.RemoveAsync(CacheKeys.AdditionalInfo_ById(infoIdToDelete.ToString()), It.IsAny<CancellationToken>()), Times.Once());
        CacheMock.Verify(c => c.RemoveAsync(CacheKeys.AdditionalInfo_ByUserId(userIdToDelete.ToString()), It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task Repository_DeleteAdditionalInfo_Should_ReturnFalse_WhenNotExists()
    {
        // Arrange
        SetupCacheOperations();
        var nonexistentId = Guid.NewGuid();

        // Act
        var ok = await Repository.DeleteAdditionalInfoAsync(nonexistentId, CancellationToken.None);

        // Assert
        ok.Should().BeFalse();
        CacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never());
    }
}
