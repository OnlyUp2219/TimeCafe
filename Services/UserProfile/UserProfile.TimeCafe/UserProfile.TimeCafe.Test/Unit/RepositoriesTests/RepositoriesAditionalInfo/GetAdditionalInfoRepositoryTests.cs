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
        var result = await Repository.GetAdditionalInfosByUserIdAsync("U1", CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.First().InfoText.Should().Be("Second info"); // сортировка по CreatedAt DESC
        CacheMock.Verify(c => c.GetAsync(CacheKeys.AdditionalInfo_ByUserId("U1"), It.IsAny<CancellationToken>()), Times.Once());
        CacheMock.Verify(c => c.SetAsync(CacheKeys.AdditionalInfo_ByUserId("U1"), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task Repository_GetAdditionalInfosByUserId_Should_ReturnCached_WhenCacheExists()
    {
        // Arrange
        await SeedAsync();
        var cachedList = TestInfos.Where(i => i.UserId == "U1").ToList();
        SetupCache(CacheKeys.AdditionalInfo_ByUserId("U1"), cachedList);
        SetupCacheOperations();

        // Act
        var result = await Repository.GetAdditionalInfosByUserIdAsync("U1", CancellationToken.None);

        // Assert
        result.Should().HaveCount(2)
            .And.BeEquivalentTo(cachedList, o => o.Including(i => i.InfoId).Including(i => i.InfoText));
        CacheMock.Verify(c => c.GetAsync(CacheKeys.AdditionalInfo_ByUserId("U1"), It.IsAny<CancellationToken>()), Times.Once());
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
        var result = await Repository.GetAdditionalInfoByIdAsync(2, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.InfoId.Should().Be(2);
        result.InfoText.Should().Be("Second info");
        CacheMock.Verify(c => c.GetAsync(CacheKeys.AdditionalInfo_ById(2), It.IsAny<CancellationToken>()), Times.Once());
        CacheMock.Verify(c => c.SetAsync(CacheKeys.AdditionalInfo_ById(2), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task Repository_GetAdditionalInfoById_Should_ReturnCached_WhenCacheExists()
    {
        // Arrange
        await SeedAsync();
        var cached = TestInfos.First(i => i.InfoId == 1);
        SetupCache(CacheKeys.AdditionalInfo_ById(1), cached);
        SetupCacheOperations();

        // Act
        var result = await Repository.GetAdditionalInfoByIdAsync(1, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.InfoId.Should().Be(1);
        CacheMock.Verify(c => c.GetAsync(CacheKeys.AdditionalInfo_ById(1), It.IsAny<CancellationToken>()), Times.Once());
        CacheMock.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task Repository_GetAdditionalInfoById_Should_ReturnNull_WhenNotExists()
    {
        // Arrange
        SetupCacheNoData();
        SetupCacheOperations();

        // Act
        var result = await Repository.GetAdditionalInfoByIdAsync(999, CancellationToken.None);

        // Assert
        result.Should().BeNull();
        CacheMock.Verify(c => c.GetAsync(CacheKeys.AdditionalInfo_ById(999), It.IsAny<CancellationToken>()), Times.Once());
        CacheMock.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Never());
    }
}
