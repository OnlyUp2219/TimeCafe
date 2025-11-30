namespace UserProfile.TimeCafe.Test.Unit.RepositoriesTests.RepositoriesAditionalInfo;

public class DeleteAdditionalInfoRepositoryTests : BaseAdditionalInfoRepositoryTest
{
    [Fact]
    public async Task Repository_DeleteAdditionalInfo_Should_RemoveAndInvalidateCaches()
    {
        // Arrange
        await SeedAsync();
        SetupCacheOperations();

        // Act
        var ok = await Repository.DeleteAdditionalInfoAsync(1, CancellationToken.None);

        // Assert
        ok.Should().BeTrue();
        var db = await Context.AdditionalInfos.FindAsync(1);
        db.Should().BeNull();
        CacheMock.Verify(c => c.RemoveAsync(CacheKeys.AdditionalInfo_All, It.IsAny<CancellationToken>()), Times.Once());
        CacheMock.Verify(c => c.RemoveAsync(CacheKeys.AdditionalInfo_ById(1), It.IsAny<CancellationToken>()), Times.Once());
        CacheMock.Verify(c => c.RemoveAsync(CacheKeys.AdditionalInfo_ByUserId("U1"), It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task Repository_DeleteAdditionalInfo_Should_ReturnFalse_WhenNotExists()
    {
        // Arrange
        SetupCacheOperations();

        // Act
        var ok = await Repository.DeleteAdditionalInfoAsync(999, CancellationToken.None);

        // Assert
        ok.Should().BeFalse();
        CacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never());
    }
}
