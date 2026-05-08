namespace UserProfile.TimeCafe.Test.Unit.ProfilesCqrs.Events;

public class ProfileChangedEventHandlerTests : BaseCqrsTest
{
    [Fact]
    public async Task Handler_Should_InvalidateProfileCache()
    {
        var userId = Guid.NewGuid();
        var profile = await SeedProfileAsync(userId, "John", "Doe");
        var handler = new ProfileChangedEventHandler(HybridCache);

        await Uow.Profiles.GetByIdAsync(userId);

        var dbProfile = await Context.Profiles.FindAsync(userId);
        dbProfile!.FirstName = "Updated";
        await Context.SaveChangesAsync();

        await handler.Handle(new ProfileChangedEvent(userId), CancellationToken.None);

        var result = await Uow.Profiles.GetByIdAsync(userId);
        result.Should().NotBeNull();
        result!.FirstName.Should().Be("Updated");
    }
}
