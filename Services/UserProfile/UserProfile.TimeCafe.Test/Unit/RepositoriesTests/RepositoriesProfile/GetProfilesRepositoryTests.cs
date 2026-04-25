namespace UserProfile.TimeCafe.Test.Unit.RepositoriesTests.RepositoriesProfile;

public class GetProfilesRepositoryTests : BaseRepositoryTest
{
    [Fact]
    public async Task Repository_GetAllProfiles_Should_ReturnAllProfiles_WhenNoCache()
    {
        await SeedProfilesAsync();

        var result = await Repository.GetAllProfilesAsync(CancellationToken.None);

        result.Should().HaveCount(TestProfiles.Count)
            .And.BeEquivalentTo(TestProfiles, options => options
                .Including(p => p.UserId)
                .Including(p => p.FirstName));
    }

    [Fact]
    public async Task Repository_GetAllProfiles_Should_ReturnCachedProfiles_WhenCacheExists()
    {
        await SeedProfilesAsync();

        var result1 = await Repository.GetAllProfilesAsync(CancellationToken.None);
        var result2 = await Repository.GetAllProfilesAsync(CancellationToken.None);

        result2.Should().HaveCount(TestProfiles.Count)
            .And.BeEquivalentTo(result1, options => options
                .Including(p => p.UserId)
                .Including(p => p.FirstName));
    }

    [Fact]
    public async Task Repository_GetProfileById_Should_ReturnProfile_WhenExistsAndNoCache()
    {
        await SeedProfilesAsync();

        var result = await Repository.GetProfileByIdAsync(TestProfiles[0].UserId, CancellationToken.None);

        result.Should().NotBeNull()
            .And.BeEquivalentTo(TestProfiles[0], options => options
                .Including(p => p.UserId)
                .Including(p => p.FirstName));
    }

    [Fact]
    public async Task Repository_GetProfileById_Should_ReturnCachedProfile_WhenCacheExists()
    {
        await SeedProfilesAsync();

        var result1 = await Repository.GetProfileByIdAsync(TestProfiles[0].UserId, CancellationToken.None);
        var result2 = await Repository.GetProfileByIdAsync(TestProfiles[0].UserId, CancellationToken.None);

        result2.Should().NotBeNull()
            .And.BeEquivalentTo(result1, options => options
                .Including(p => p.UserId)
                .Including(p => p.FirstName));
    }

    [Fact]
    public async Task Repository_GetProfileById_Should_ReturnNull_WhenNotExists()
    {
        var nonexistentId = Guid.NewGuid();

        var result = await Repository.GetProfileByIdAsync(nonexistentId, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Repository_GetTotalPage_Should_ReturnCorrectCount()
    {
        await SeedProfilesAsync();

        var result = await Repository.GetTotalPageAsync(CancellationToken.None);

        result.Should().Be(TestProfiles.Count);
    }

    [Fact]
    public async Task Repository_GetTotalPage_Should_ReturnZero_WhenNoProfiles()
    {
        var result = await Repository.GetTotalPageAsync(CancellationToken.None);

        result.Should().Be(0);
    }
}

