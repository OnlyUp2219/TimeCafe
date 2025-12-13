namespace Venue.TimeCafe.Test.Unit.Repositories.TariffRepository;

public class DeleteAsyncTests : BaseCqrsTest
{
    [Fact]
    public async Task Repository_DeleteAsync_Should_ReturnTrue_WhenTariffExists()
    {
        // Arrange
        var tariff = await SeedTariffAsync(TestData.ExistingTariffs.Tariff1Name, TestData.ExistingTariffs.Tariff1PricePerMinute);

        // Act
        var result = await TariffRepository.DeleteAsync(tariff.TariffId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task Repository_DeleteAsync_Should_ReturnFalse_WhenTariffNotExists()
    {
        // Arrange
        var nonExistentId = TestData.NonExistingIds.NonExistingTariffId;

        // Act
        var result = await TariffRepository.DeleteAsync(nonExistentId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task Repository_DeleteAsync_Should_RemoveFromDatabase()
    {
        // Arrange
        var tariff = await SeedTariffAsync(TestData.ExistingTariffs.Tariff2Name, TestData.ExistingTariffs.Tariff2PricePerMinute);
        var tariffId = tariff.TariffId;

        // Act
        await TariffRepository.DeleteAsync(tariffId);

        // Assert
        var fromDb = await Context.Tariffs.FindAsync(tariffId);
        fromDb.Should().BeNull();
    }

    [Fact]
    public async Task Repository_DeleteAsync_Should_InvalidateCache()
    {
        // Arrange
        var tariff = await SeedTariffAsync(TestData.DefaultValues.DefaultTariffName, TestData.DefaultValues.DefaultTariffPrice);

        // Act
        await TariffRepository.DeleteAsync(tariff.TariffId);

        // Assert
        CacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task Repository_DeleteAsync_Should_HandleAlreadyDeleted()
    {
        // Arrange
        var tariff = await SeedTariffAsync(TestData.ExistingTariffs.Tariff3Name, TestData.ExistingTariffs.Tariff3PricePerMinute);
        var tariffId = tariff.TariffId;

        // Act
        var firstDelete = await TariffRepository.DeleteAsync(tariffId);
        var secondDelete = await TariffRepository.DeleteAsync(tariffId);

        // Assert
        firstDelete.Should().BeTrue();
        secondDelete.Should().BeFalse();
    }
}
