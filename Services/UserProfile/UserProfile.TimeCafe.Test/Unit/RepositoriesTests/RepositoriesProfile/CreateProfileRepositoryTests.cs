namespace UserProfile.TimeCafe.Test.Unit.RepositoriesTests.RepositoriesProfile;

public class CreateProfileRepositoryTests : BaseRepositoryTest
{
    [Fact]
    public async Task Repository_CreateProfile_Should_AddProfile()
    {
        var userId = Guid.NewGuid();
        var profile = new Profile { UserId = userId, FirstName = "Alice" };

        var result = await Repository.CreateAsync(profile);

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

        var result = await Repository.CreateAsync(profile);

        result.Should().NotBeNull();
        result!.UserId.Should().Be(userId);
        result.FirstName.Should().Be("Bob");
        result.LastName.Should().Be("Smith");
        result.Gender.Should().Be(Gender.Male);
    }

    [Fact]
    public async Task Repository_CreateEmpty_Should_CreateEmptyProfile()
    {
        var userId = Guid.NewGuid();

        await Repository.CreateEmptyAsync(userId);

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

        await Repository.CreateEmptyAsync(firstProfile.UserId);

        var dbProfiles = await Context.Profiles.ToListAsync();
        dbProfiles.Should().HaveCount(TestProfiles.Count);
        dbProfiles.First(p => p.UserId == firstProfile.UserId).FirstName.Should().Be("John");
    }


}

