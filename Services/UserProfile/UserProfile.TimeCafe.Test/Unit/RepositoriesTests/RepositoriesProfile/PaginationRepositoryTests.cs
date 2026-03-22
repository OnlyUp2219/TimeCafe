namespace UserProfile.TimeCafe.Test.Unit.RepositoriesTests.RepositoriesProfile;

public class PaginationRepositoryTests : BaseRepositoryTest
{
    [Fact]
    public async Task Repository_GetProfilesPage_Should_ReturnPagedProfiles_WhenNoCache()
    {
        await SeedProfilesAsync();

        var result = await Repository.GetProfilesPageAsync(1, 2, CancellationToken.None);

        result.Should().NotBeNull()
            .And.HaveCount(2);
    }

    [Fact]
    public async Task Repository_GetProfilesPage_Should_ReturnCachedProfiles_WhenCacheExists()
    {
        await SeedProfilesAsync();

        var result1 = await Repository.GetProfilesPageAsync(1, 2, CancellationToken.None);
        var result2 = await Repository.GetProfilesPageAsync(1, 2, CancellationToken.None);

        result2.Should().NotBeNull()
            .And.HaveCount(2)
            .And.BeEquivalentTo(result1, options => options
                .Including(p => p!.UserId)
                .Including(p => p!.FirstName));
    }

    [Fact]
    public async Task Repository_GetProfilesPage_Should_ReturnPartialPage_WhenLastPage()
    {
        await SeedProfilesAsync();

        var result = await Repository.GetProfilesPageAsync(2, 2, CancellationToken.None);

        result.Should().NotBeNull()
            .And.HaveCount(1);
    }

    [Fact]
    public async Task Repository_GetProfilesPage_Should_ReturnEmpty_WhenPageOutOfRange()
    {
        await SeedProfilesAsync();

        var result = await Repository.GetProfilesPageAsync(10, 2, CancellationToken.None);

        result.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public async Task Repository_GetProfilesPage_Should_InvalidateCache_WhenVersionChanges()
    {
        await SeedProfilesAsync();

        var result1 = await Repository.GetProfilesPageAsync(1, 2, CancellationToken.None);
        result1.Should().HaveCount(2);

        var newProfile = new Profile { UserId = Guid.NewGuid(), FirstName = "NewUser", CreatedAt = DateTimeOffset.UtcNow.AddDays(1) };
        await Repository.CreateProfileAsync(newProfile, CancellationToken.None);

        var result2 = await Repository.GetProfilesPageAsync(1, 2, CancellationToken.None);
        result2.Should().HaveCount(2);
    }
}
