namespace UserProfile.TimeCafe.Test.Unit.RepositoriesTests.RepositoriesAditionalInfo;

public class GetAdditionalInfoRepositoryTests : BaseAdditionalInfoRepositoryTest
{
    [Fact]
    public async Task Repository_GetAdditionalInfosByUserId_Should_ReturnAllInfosOrdered_WhenNoCache()
    {
        await SeedAsync();

        var result = await Repository.GetAdditionalInfosByUserIdAsync(TestInfos[0].UserId, CancellationToken.None);

        result.Should().HaveCount(2);
        result.First().InfoText.Should().Be("Second info");
    }

    [Fact]
    public async Task Repository_GetAdditionalInfosByUserId_Should_ReturnCached_WhenCacheExists()
    {
        await SeedAsync();

        var result1 = await Repository.GetAdditionalInfosByUserIdAsync(TestInfos[0].UserId, CancellationToken.None);
        var result2 = await Repository.GetAdditionalInfosByUserIdAsync(TestInfos[0].UserId, CancellationToken.None);

        result2.Should().HaveCount(2)
            .And.BeEquivalentTo(result1, o => o.Including(i => i.InfoId).Including(i => i.InfoText));
    }

    [Fact]
    public async Task Repository_GetAdditionalInfoById_Should_ReturnInfo_WhenExistsAndNoCache()
    {
        await SeedAsync();

        var result = await Repository.GetAdditionalInfoByIdAsync(TestInfos[1].InfoId, CancellationToken.None);

        result.Should().NotBeNull();
        result!.InfoId.Should().Be(TestInfos[1].InfoId);
        result.InfoText.Should().Be("Second info");
    }

    [Fact]
    public async Task Repository_GetAdditionalInfoById_Should_ReturnCached_WhenCacheExists()
    {
        await SeedAsync();

        var result1 = await Repository.GetAdditionalInfoByIdAsync(TestInfos[0].InfoId, CancellationToken.None);
        var result2 = await Repository.GetAdditionalInfoByIdAsync(TestInfos[0].InfoId, CancellationToken.None);

        result2.Should().NotBeNull();
        result2!.InfoId.Should().Be(result1!.InfoId);
    }

    [Fact]
    public async Task Repository_GetAdditionalInfoById_Should_ReturnNull_WhenNotExists()
    {
        var nonexistentId = Guid.NewGuid();

        var result = await Repository.GetAdditionalInfoByIdAsync(nonexistentId, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Repository_GetAdditionalInfosByUserId_Should_ReturnEmpty_WhenUserHasNoInfos()
    {
        await SeedAsync();

        var result = await Repository.GetAdditionalInfosByUserIdAsync(Guid.NewGuid(), CancellationToken.None);

        result.Should().BeEmpty();
    }
}
