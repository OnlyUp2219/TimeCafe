namespace UserProfile.TimeCafe.Test.Unit.RepositoriesTests.RepositoriesAditionalInfo;

public class GetAdditionalInfoRepositoryTests : BaseAdditionalInfoRepositoryTest
{
    [Fact]
    public async Task Repository_GetAdditionalInfosByUserId_Should_ReturnAllInfosOrdered_WhenNoCache()
    {
        await SeedAsync();

        var result = await Repository.GetByUserIdAsync(TestInfos[0].UserId);

        result.Should().HaveCount(2);
        result.First().InfoText.Should().Be("Second info");
    }

    [Fact]
    public async Task Repository_GetAdditionalInfosByUserId_Should_ReturnCached_WhenCacheExists()
    {
        await SeedAsync();

        var result1 = await Repository.GetByUserIdAsync(TestInfos[0].UserId);
        var result2 = await Repository.GetByUserIdAsync(TestInfos[0].UserId);

        result2.Should().HaveCount(2)
            .And.BeEquivalentTo(result1, o => o.Including(i => i.InfoId).Including(i => i.InfoText));
    }

    [Fact]
    public async Task Repository_GetAdditionalInfoById_Should_ReturnInfo_WhenExistsAndNoCache()
    {
        await SeedAsync();

        var result = await Repository.GetByIdAsync(TestInfos[1].InfoId);

        result.Should().NotBeNull();
        result!.InfoId.Should().Be(TestInfos[1].InfoId);
        result.InfoText.Should().Be("Second info");
    }

    [Fact]
    public async Task Repository_GetAdditionalInfoById_Should_ReturnCached_WhenCacheExists()
    {
        await SeedAsync();

        var result1 = await Repository.GetByIdAsync(TestInfos[0].InfoId);
        var result2 = await Repository.GetByIdAsync(TestInfos[0].InfoId);

        result2.Should().NotBeNull();
        result2!.InfoId.Should().Be(result1!.InfoId);
    }

    [Fact]
    public async Task Repository_GetAdditionalInfoById_Should_ReturnNull_WhenNotExists()
    {
        var nonexistentId = Guid.NewGuid();

        var result = await Repository.GetByIdAsync(nonexistentId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Repository_GetAdditionalInfosByUserId_Should_ReturnEmpty_WhenUserHasNoInfos()
    {
        await SeedAsync();

        var result = await Repository.GetByUserIdAsync(Guid.NewGuid());

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Repository_GetPagedByUserIdAsync_Should_ReturnPagedInfos()
    {
        await SeedAsync();
        var userId = TestInfos[0].UserId;

        var (items, totalCount) = await Repository.GetPagedByUserIdAsync(userId, 1, 1);

        totalCount.Should().Be(2);
        items.Should().HaveCount(1);
        items.First().InfoText.Should().Be("Second info");
    }

    [Fact]
    public async Task Repository_GetPagedByUserIdAsync_Should_ReturnSecondPage()
    {
        await SeedAsync();
        var userId = TestInfos[0].UserId;

        var (items, totalCount) = await Repository.GetPagedByUserIdAsync(userId, 2, 1);

        totalCount.Should().Be(2);
        items.Should().HaveCount(1);
        items.First().InfoText.Should().Be("First info");
    }

    [Fact]
    public async Task Repository_GetPagedByUserIdAsync_Should_ReturnEmpty_WhenPageOutOfBounds()
    {
        await SeedAsync();
        var userId = TestInfos[0].UserId;

        var (items, totalCount) = await Repository.GetPagedByUserIdAsync(userId, 3, 1);

        totalCount.Should().Be(2);
        items.Should().BeEmpty();
    }
}

