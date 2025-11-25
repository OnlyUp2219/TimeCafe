namespace UserProfile.TimeCafe.Test.Unit.RepositoriesTests.RepositoriesProfile;

public class PaginationRepositoryTests : BaseRepositoryTest
{
    [Fact]
    public async Task Repository_GetProfilesPage_Should_ReturnPagedProfiles_WhenNoCache()
    {
        // Arrange
        await SeedProfilesAsync();
        SetupCacheNoData();
        SetupCacheOperations();

        CacheMock.Setup(c => c.GetAsync(CacheKeys.ProfilePagesVersion(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        // Act
        var result = await Repository.GetProfilesPageAsync(1, 2, CancellationToken.None);

        // Assert
        result.Should().NotBeNull()
            .And.HaveCount(2)
            .And.Contain(p => p!.UserId == "1")
            .And.Contain(p => p!.UserId == "2");

        CacheMock.Verify(c => c.GetAsync(CacheKeys.Profile_Page(1, 1), It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task Repository_GetProfilesPage_Should_ReturnCachedProfiles_WhenCacheExists()
    {
        // Arrange
        var pageProfiles = TestProfiles.Take(2).ToList();
        SetupCache(CacheKeys.Profile_Page(1, 1), pageProfiles);
        SetupCacheOperations();

        CacheMock.Setup(c => c.GetAsync(CacheKeys.ProfilePagesVersion(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(System.Text.Encoding.UTF8.GetBytes("1"));

        // Act
        var result = await Repository.GetProfilesPageAsync(1, 2, CancellationToken.None);

        // Assert
        result.Should().NotBeNull()
            .And.HaveCount(2)
            .And.BeEquivalentTo(pageProfiles, options => options
                .Including(p => p!.UserId)
                .Including(p => p!.FirstName));

        CacheMock.Verify(c => c.GetAsync(CacheKeys.Profile_Page(1, 1), It.IsAny<CancellationToken>()), Times.Once());
        CacheMock.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task Repository_GetProfilesPage_Should_ReturnPartialPage_WhenLastPage()
    {
        // Arrange
        await SeedProfilesAsync();
        SetupCacheNoData();
        SetupCacheOperations();

        CacheMock.Setup(c => c.GetAsync(CacheKeys.ProfilePagesVersion(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        // Act
        var result = await Repository.GetProfilesPageAsync(2, 2, CancellationToken.None);

        // Assert
        result.Should().NotBeNull()
            .And.HaveCount(1) // только 1 профиль на второй странице
            .And.Contain(p => p!.UserId == "3");
    }

    [Fact]
    public async Task Repository_GetProfilesPage_Should_ReturnEmpty_WhenPageOutOfRange()
    {
        // Arrange
        await SeedProfilesAsync();
        SetupCacheNoData();
        SetupCacheOperations();

        CacheMock.Setup(c => c.GetAsync(CacheKeys.ProfilePagesVersion(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        // Act
        var result = await Repository.GetProfilesPageAsync(10, 2, CancellationToken.None);

        // Assert
        result.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public async Task Repository_GetProfilesPage_Should_InvalidateCache_WhenVersionChanges()
    {
        // Arrange
        await SeedProfilesAsync();

        // Первый запрос - кеш версии 1
        SetupCache(CacheKeys.Profile_Page(1, 1), TestProfiles.Take(2).ToList());
        SetupCacheOperations();
        CacheMock.Setup(c => c.GetAsync(CacheKeys.ProfilePagesVersion(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(System.Text.Encoding.UTF8.GetBytes("1"));

        var result1 = await Repository.GetProfilesPageAsync(1, 2, CancellationToken.None);
        result1.Should().HaveCount(2);

        // Изменяем версию на 2 - кеш должен быть инвалидирован
        CacheMock.Setup(c => c.GetAsync(CacheKeys.ProfilePagesVersion(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(System.Text.Encoding.UTF8.GetBytes("2"));
        CacheMock.Setup(c => c.GetAsync(CacheKeys.Profile_Page(1, 2), It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        // Act
        var result2 = await Repository.GetProfilesPageAsync(1, 2, CancellationToken.None);

        // Assert
        result2.Should().HaveCount(2);
        CacheMock.Verify(c => c.SetAsync(CacheKeys.Profile_Page(1, 2), It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Once());
    }
}
