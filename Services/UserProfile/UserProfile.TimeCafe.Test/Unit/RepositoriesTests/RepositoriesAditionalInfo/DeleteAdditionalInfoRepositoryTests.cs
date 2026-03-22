namespace UserProfile.TimeCafe.Test.Unit.RepositoriesTests.RepositoriesAditionalInfo;

public class DeleteAdditionalInfoRepositoryTests : BaseAdditionalInfoRepositoryTest
{
    [Fact]
    public async Task Repository_DeleteAdditionalInfo_Should_RemoveAndInvalidateCaches()
    {
        await SeedAsync();
        var infoIdToDelete = TestInfos[0].InfoId;

        var ok = await Repository.DeleteAdditionalInfoAsync(infoIdToDelete, CancellationToken.None);

        ok.Should().BeTrue();
        var db = await Context.AdditionalInfos.FindAsync(infoIdToDelete);
        db.Should().BeNull();
    }

    [Fact]
    public async Task Repository_DeleteAdditionalInfo_Should_ReturnFalse_WhenNotExists()
    {
        var nonexistentId = Guid.NewGuid();

        var ok = await Repository.DeleteAdditionalInfoAsync(nonexistentId, CancellationToken.None);

        ok.Should().BeFalse();
    }
}
