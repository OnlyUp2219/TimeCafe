namespace UserProfile.TimeCafe.Test.Unit.AdditionalInfosCqrs.Events;

public class AdditionalInfoChangedEventHandlerTests : BaseCqrsTest
{
    [Fact]
    public async Task Handler_Should_InvalidateAdditionalInfoCache()
    {
        var userId = Guid.NewGuid();
        var info = new AdditionalInfo { InfoId = Guid.NewGuid(), UserId = userId, InfoText = "Initial" };
        Context.AdditionalInfos.Add(info);
        await Context.SaveChangesAsync();

        var handler = new AdditionalInfoChangedEventHandler(HybridCache);

        await Uow.AdditionalInfos.GetByUserIdAsync(userId);

        var dbInfo = await Context.AdditionalInfos.FindAsync(info.InfoId);
        dbInfo!.InfoText = "Updated";
        await Context.SaveChangesAsync();

        await handler.Handle(new AdditionalInfoChangedEvent(userId), CancellationToken.None);

        var result = await Uow.AdditionalInfos.GetByUserIdAsync(userId);
        result.Should().NotBeNull();
        result.First().InfoText.Should().Be("Updated");
    }
}
