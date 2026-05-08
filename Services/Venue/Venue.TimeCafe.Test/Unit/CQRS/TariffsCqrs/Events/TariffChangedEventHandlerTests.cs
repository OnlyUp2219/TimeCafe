namespace Venue.TimeCafe.Test.Unit.CQRS.TariffsCqrs.Events;

public class TariffChangedEventHandlerTests : BaseCqrsTest
{
    [Fact]
    public async Task Handler_Should_InvalidateTariffsCache()
    {
        // Arrange
        var tariff = await SeedTariffAsync("Tariff 1", 100m);
        var handler = new TariffChangedEventHandler(HybridCache);

        // Fill cache
        await TariffRepository.GetWithThemeByIdAsync(tariff.TariffId);

        // Modify DB directly
        var dbTariff = await Context.Tariffs.FindAsync(tariff.TariffId);
        dbTariff!.Name = "Updated Name";
        await Context.SaveChangesAsync();

        // Act
        await handler.Handle(new TariffChangedEvent(tariff.TariffId));

        // Assert
        var result = await TariffRepository.GetWithThemeByIdAsync(tariff.TariffId);
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Name"); // Should be NEW name because cache was invalidated
    }
}
