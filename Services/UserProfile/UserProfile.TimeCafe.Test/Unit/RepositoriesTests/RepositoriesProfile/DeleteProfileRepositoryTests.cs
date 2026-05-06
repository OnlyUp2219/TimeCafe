namespace UserProfile.TimeCafe.Test.Unit.RepositoriesTests.RepositoriesProfile;

public class DeleteProfileRepositoryTests : BaseRepositoryTest
{
    [Fact]
    public async Task Repository_DeleteProfile_Should_RemoveProfile()
    {
        await SeedProfilesAsync();

        await Repository.DeleteAsync(TestProfiles[0].UserId);
        await Context.SaveChangesAsync();

        var dbProfile = await Context.Profiles.FindAsync(TestProfiles[0].UserId);
        dbProfile.Should().BeNull();
    }

    [Fact]
    public async Task Repository_DeleteProfile_Should_DoNothing_WhenProfileNotExists()
    {
        await Repository.DeleteAsync(Guid.NewGuid());
        await Context.SaveChangesAsync();

        var count = await Context.Profiles.CountAsync();
        count.Should().Be(0);
    }

    [Fact]
    public async Task Repository_DeleteProfile_Should_ReduceTotalCount()
    {
        await SeedProfilesAsync();
        var countBefore = await Repository.GetTotalPageAsync(CancellationToken.None);

        await Repository.DeleteAsync(TestProfiles[0].UserId);
        await Context.SaveChangesAsync();

        var countAfter = await Repository.GetTotalPageAsync(CancellationToken.None);
        countAfter.Should().Be(countBefore - 1);
    }



    [Fact]
    public async Task Repository_DeleteProfile_Should_RemoveCorrectProfile()
    {
        await SeedProfilesAsync();

        await Repository.DeleteAsync(TestProfiles[1].UserId);
        await Context.SaveChangesAsync();

        var profile1 = await Context.Profiles.FindAsync(TestProfiles[0].UserId);
        var profile2 = await Context.Profiles.FindAsync(TestProfiles[1].UserId);
        var profile3 = await Context.Profiles.FindAsync(TestProfiles[2].UserId);

        profile1.Should().NotBeNull();
        profile2.Should().BeNull();
        profile3.Should().NotBeNull();
    }
}

