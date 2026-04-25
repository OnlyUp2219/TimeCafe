namespace UserProfile.TimeCafe.Test.Unit.RepositoriesTests.RepositoriesAditionalInfo;

public class CreateAdditionalInfoRepositoryTests : BaseAdditionalInfoRepositoryTest
{
    [Fact]
    public async Task Repository_CreateAdditionalInfo_Should_PersistAndInvalidateUserCache()
    {
        await SeedAsync();
        var newInfo = new AdditionalInfo { UserId = TestInfos[0].UserId, InfoText = "New cached removal" };

        var result = await Repository.CreateAdditionalInfoAsync(newInfo, CancellationToken.None);

        result.Should().NotBeNull();
        var db = await Context.AdditionalInfos.FindAsync(result.InfoId);
        db.Should().NotBeNull();
        db!.InfoText.Should().Be("New cached removal");
    }
}

