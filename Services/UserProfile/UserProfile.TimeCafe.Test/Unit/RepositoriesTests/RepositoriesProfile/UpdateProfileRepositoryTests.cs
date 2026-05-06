namespace UserProfile.TimeCafe.Test.Unit.RepositoriesTests.RepositoriesProfile;

public class UpdateProfileRepositoryTests : BaseRepositoryTest
{
    [Fact]
    public async Task Repository_UpdateProfile_Should_UpdateProfile()
    {
        await SeedProfilesAsync();
        var userId = TestProfiles[0].UserId;
        var updatedProfile = new Profile { UserId = userId, FirstName = "Jane" };

        var result = await Repository.UpdateAsync(updatedProfile);
        await Context.SaveChangesAsync();

        result.Should().NotBeNull();

        var dbProfile = await Context.Profiles.FindAsync(userId);
        dbProfile!.FirstName.Should().Be("Jane");
    }

    [Fact]
    public async Task Repository_UpdateProfile_Should_UpdateAllFields()
    {
        await SeedProfilesAsync();
        var userId = TestProfiles[0].UserId;
        var updatedProfile = new Profile
        {
            UserId = userId,
            FirstName = "Updated",
            LastName = "User",
            Gender = Gender.Female,
            ProfileStatus = ProfileStatus.Completed,
            BirthDate = new DateOnly(1990, 1, 1)
        };

        var result = await Repository.UpdateAsync(updatedProfile);
        await Context.SaveChangesAsync();

        result.Should().NotBeNull();
        result!.FirstName.Should().Be("Updated");
        result.LastName.Should().Be("User");
        result.Gender.Should().Be(Gender.Female);
        result.ProfileStatus.Should().Be(ProfileStatus.Completed);
        result.BirthDate.Should().Be(new DateOnly(1990, 1, 1));
    }

    [Fact]
    public async Task Repository_UpdateProfile_Should_ReturnNull_WhenProfileNotExists()
    {
        var nonExistentProfile = new Profile { UserId = Guid.NewGuid(), FirstName = "Test" };

        var result = await Repository.UpdateAsync(nonExistentProfile);

        result.Should().BeNull();
    }
}

