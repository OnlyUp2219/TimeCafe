using static UserProfile.TimeCafe.Test.Integration.Helpers.TestData;

namespace UserProfile.TimeCafe.Test.Unit.RepositoriesTests.RepositoriesAditionalInfo;

public class GetAdditionalInfoRepositoryTests : BaseAdditionalInfoRepositoryTest
{
    [Fact]
    public async Task Repository_GetAdditionalInfosByUserId_Should_ReturnAllInfosOrdered_WhenNoCache()
    {
        // Arrange
        await SeedAsync();
        SetupCacheNoData();
        SetupCacheOperations();

        // Act
        var result = await Repository.GetAdditionalInfosByUserIdAsync(TestInfos[0].UserId, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.First().InfoText.Should().Be("Second info"); // сортировка по CreatedAt DESC
        CacheMock.Verify(c => c.GetAsync(CacheKeys.AdditionalInfo_ByUserId(TestInfos[0].UserId), It.IsAny<CancellationToken>()), Times.Once());
        CacheMock.Verify(c => c.SetAsync(CacheKeys.AdditionalInfo_ByUserId(TestInfos[0].UserId), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task Repository_GetAdditionalInfosByUserId_Should_ReturnCached_WhenCacheExists()
    {
        // Arrange
        await SeedAsync();
        var cachedList = TestInfos.Where(i => i.UserId == TestInfos[0].UserId).ToList();
        SetupCache(CacheKeys.AdditionalInfo_ByUserId(TestInfos[0].UserId), cachedList);
        SetupCacheOperations();

        // Act
        var result = await Repository.GetAdditionalInfosByUserIdAsync(TestInfos[0].UserId, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2)
            .And.BeEquivalentTo(cachedList, o => o.Including(i => i.InfoId).Including(i => i.InfoText));
        CacheMock.Verify(c => c.GetAsync(CacheKeys.AdditionalInfo_ByUserId(TestInfos[0].UserId), It.IsAny<CancellationToken>()), Times.Once());
        CacheMock.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task Repository_GetAdditionalInfoById_Should_ReturnInfo_WhenExistsAndNoCache()
    {
        // Arrange
        await SeedAsync();
        SetupCacheNoData();
        SetupCacheOperations();

        // Act
        var result = await Repository.GetAdditionalInfoByIdAsync(TestInfos[1].InfoId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.InfoId.Should().Be(TestInfos[1].InfoId);
        result.InfoText.Should().Be("Second info");
        CacheMock.Verify(c => c.GetAsync(CacheKeys.AdditionalInfo_ById(TestInfos[1].InfoId.ToString()), It.IsAny<CancellationToken>()), Times.Once());
        CacheMock.Verify(c => c.SetAsync(CacheKeys.AdditionalInfo_ById(TestInfos[1].InfoId.ToString()), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task Repository_GetAdditionalInfoById_Should_ReturnCached_WhenCacheExists()
    {
        // Arrange
        await SeedAsync();
        var cached = TestInfos.First(i => i.InfoId == TestInfos[0].InfoId);
        SetupCache(CacheKeys.AdditionalInfo_ById(cached.InfoId.ToString()), cached);
        SetupCacheOperations();

        // Act
        var result = await Repository.GetAdditionalInfoByIdAsync(cached.InfoId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.InfoId.Should().Be(cached.InfoId);
        CacheMock.Verify(c => c.GetAsync(CacheKeys.AdditionalInfo_ById(cached.InfoId.ToString()), It.IsAny<CancellationToken>()), Times.Once());
        CacheMock.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task Repository_GetAdditionalInfoById_Should_ReturnNull_WhenNotExists()
    {
        // Arrange
        SetupCacheNoData();
        SetupCacheOperations();
        var nonexistentId = Guid.NewGuid();

        // Act
        var result = await Repository.GetAdditionalInfoByIdAsync(nonexistentId, CancellationToken.None);

        // Assert
        result.Should().BeNull();
        CacheMock.Verify(c => c.GetAsync(CacheKeys.AdditionalInfo_ById(nonexistentId.ToString()), It.IsAny<CancellationToken>()), Times.Once());
        CacheMock.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Never());
    }
}
