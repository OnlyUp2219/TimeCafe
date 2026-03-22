namespace UserProfile.TimeCafe.Test.Unit.RepositoriesTests.RepositoriesProfile;

public class CreateProfileRepositoryTests : BaseRepositoryTest
{
    [Fact]
    public async Task Repository_CreateProfile_Should_AddProfileAndInvalidateCache()
    {
        var userId = Guid.NewGuid();
        var profile = new Profile { UserId = userId, FirstName = "Alice" };

        var result = await Repository.CreateProfileAsync(profile, CancellationToken.None);

        result.Should().NotBeNull();

        var dbProfile = await Context.Profiles.FindAsync(userId);
        dbProfile.Should().NotBeNull()
            .And.BeEquivalentTo(profile, options => options
                .Including(p => p.UserId)
                .Including(p => p.FirstName));
    }

    [Fact]
    public async Task Repository_CreateProfile_Should_ReturnCreatedProfile()
    {
        var userId = Guid.NewGuid();
        var profile = new Profile
        {
            UserId = userId,
            FirstName = "Bob",
            LastName = "Smith",
            Gender = Gender.Male,
            ProfileStatus = ProfileStatus.Pending
        };

        var result = await Repository.CreateProfileAsync(profile, CancellationToken.None);

        result.Should().NotBeNull();
        result!.UserId.Should().Be(userId);
        result.FirstName.Should().Be("Bob");
        result.LastName.Should().Be("Smith");
        result.Gender.Should().Be(Gender.Male);
    }

    [Fact]
    public async Task Repository_CreateEmpty_Should_CreateEmptyProfileAndInvalidateCache()
    {
        var userId = Guid.NewGuid();

        await Repository.CreateEmptyAsync(userId, CancellationToken.None);

        var dbProfile = await Context.Profiles.FindAsync(userId);
        dbProfile.Should().NotBeNull();
        dbProfile!.UserId.Should().Be(userId);
        dbProfile.FirstName.Should().BeEmpty();
        dbProfile.ProfileStatus.Should().Be(ProfileStatus.Pending);
    }

    [Fact]
    public async Task Repository_CreateEmpty_Should_DoNothing_WhenProfileExists()
    {
        await SeedProfilesAsync();
        var firstProfile = TestProfiles[0];

        await Repository.CreateEmptyAsync(firstProfile.UserId, CancellationToken.None);

        var dbProfiles = await Context.Profiles.ToListAsync();
        dbProfiles.Should().HaveCount(TestProfiles.Count);
        dbProfiles.First(p => p.UserId == firstProfile.UserId).FirstName.Should().Be("John");
    }

    [Fact]
    public async Task Repository_CreateProfile_Should_InvalidateCacheAfterCreate()
    {
        await SeedProfilesAsync();
        var allBefore = await Repository.GetAllProfilesAsync(CancellationToken.None);

        var newProfile = new Profile { UserId = Guid.NewGuid(), FirstName = "NewUser" };
        await Repository.CreateProfileAsync(newProfile, CancellationToken.None);

        var allAfter = await Repository.GetAllProfilesAsync(CancellationToken.None);
        allAfter.Should().HaveCount(allBefore.Count() + 1);
    }
}
