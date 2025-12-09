using static UserProfile.TimeCafe.Test.Integration.Helpers.TestData;

namespace UserProfile.TimeCafe.Test.Unit.RepositoriesTests.RepositoriesAditionalInfo;

public class CreateAdditionalInfoRepositoryTests : BaseAdditionalInfoRepositoryTest
{
    [Fact]
    public async Task Repository_CreateAdditionalInfo_Should_PersistAndInvalidateUserCache()
    {
        // Arrange
        await SeedAsync();
        var newInfo = new AdditionalInfo { UserId = TestInfos[0].UserId, InfoText = "New cached removal" };
        SetupCacheOperations();

        // Act
        var result = await Repository.CreateAdditionalInfoAsync(newInfo, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        var db = await Context.AdditionalInfos.FindAsync(result.InfoId);
        db.Should().NotBeNull();
        db!.InfoText.Should().Be("New cached removal");

        CacheMock.Verify(c => c.RemoveAsync(CacheKeys.AdditionalInfo_All, It.IsAny<CancellationToken>()), Times.Once());
        CacheMock.Verify(c => c.RemoveAsync(CacheKeys.AdditionalInfo_ByUserId(TestInfos[0].UserId.ToString()), It.IsAny<CancellationToken>()), Times.Once());
    }
}
