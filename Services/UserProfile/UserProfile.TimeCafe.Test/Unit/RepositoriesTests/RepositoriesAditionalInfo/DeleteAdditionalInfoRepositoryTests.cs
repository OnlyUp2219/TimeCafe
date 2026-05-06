namespace UserProfile.TimeCafe.Test.Unit.RepositoriesTests.RepositoriesAditionalInfo;

public class DeleteAdditionalInfoRepositoryTests : BaseAdditionalInfoRepositoryTest
{
    [Fact]
    public async Task Repository_DeleteAdditionalInfo_Should_Remove()
    {
        await SeedAsync();
        var infoIdToDelete = TestInfos[0].InfoId;

        var ok = await Repository.DeleteAsync(infoIdToDelete);
        await Context.SaveChangesAsync();

        ok.Should().BeTrue();
        var db = await Context.AdditionalInfos.FindAsync(infoIdToDelete);
        db.Should().BeNull();
    }

    [Fact]
    public async Task Repository_DeleteAdditionalInfo_Should_ReturnFalse_WhenNotExists()
    {
        var nonexistentId = Guid.NewGuid();

        var ok = await Repository.DeleteAsync(nonexistentId);

        ok.Should().BeFalse();
    }
}

