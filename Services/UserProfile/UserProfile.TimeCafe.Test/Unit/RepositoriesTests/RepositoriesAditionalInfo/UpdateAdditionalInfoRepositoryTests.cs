namespace UserProfile.TimeCafe.Test.Unit.RepositoriesTests.RepositoriesAditionalInfo;

public class UpdateAdditionalInfoRepositoryTests : BaseAdditionalInfoRepositoryTest
{
    [Fact]
    public async Task Repository_UpdateAdditionalInfo_Should_Update()
    {
        await SeedAsync();
        var infoId = TestInfos[1].InfoId;
        var userId = TestInfos[1].UserId;
        var upd = new AdditionalInfo { InfoId = infoId, UserId = userId, InfoText = "Updated second" };

        var result = await Repository.UpdateAsync(upd);
        await Context.SaveChangesAsync();

        result.Should().NotBeNull();
        var db = await Context.AdditionalInfos.FindAsync(infoId);
        db!.InfoText.Should().Be("Updated second");
    }

    [Fact]
    public async Task Repository_UpdateAdditionalInfo_Should_ReturnNull_WhenNotExists()
    {
        var upd = new AdditionalInfo { InfoId = Guid.NewGuid(), UserId = TestInfos[0].UserId, InfoText = "Ghost" };

        var result = await Repository.UpdateAsync(upd);

        result.Should().BeNull();
    }
}

