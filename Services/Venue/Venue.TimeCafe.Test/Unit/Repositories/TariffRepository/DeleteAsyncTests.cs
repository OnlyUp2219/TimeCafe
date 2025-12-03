namespace Venue.TimeCafe.Test.Unit.Repositories.TariffRepository;

public class DeleteAsyncTests : BaseCqrsTest
{
    [Fact]
    public async Task Repository_DeleteAsync_Should_ReturnTrue_WhenTariffExists()
    {
        // Arrange
        var tariff = await SeedTariffAsync("To Delete", 100m);

        // Act
        var result = await TariffRepository.DeleteAsync(tariff.TariffId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task Repository_DeleteAsync_Should_ReturnFalse_WhenTariffNotExists()
    {
        // Arrange
        var nonExistentId = 99999;

        // Act
        var result = await TariffRepository.DeleteAsync(nonExistentId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task Repository_DeleteAsync_Should_RemoveFromDatabase()
    {
        // Arrange
        var tariff = await SeedTariffAsync("To Remove", 100m);
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
        var tariff = await SeedTariffAsync("Cache Test", 100m);

        // Act
        await TariffRepository.DeleteAsync(tariff.TariffId);

        // Assert
        CacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task Repository_DeleteAsync_Should_HandleAlreadyDeleted()
    {
        // Arrange
        var tariff = await SeedTariffAsync("To Delete Twice", 100m);
        var tariffId = tariff.TariffId;

        // Act
        var firstDelete = await TariffRepository.DeleteAsync(tariffId);
        var secondDelete = await TariffRepository.DeleteAsync(tariffId);

        // Assert
        firstDelete.Should().BeTrue();
        secondDelete.Should().BeFalse();
    }
}
