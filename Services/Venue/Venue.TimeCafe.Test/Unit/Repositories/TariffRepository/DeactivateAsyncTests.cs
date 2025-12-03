namespace Venue.TimeCafe.Test.Unit.Repositories.TariffRepository;

public class DeactivateAsyncTests : BaseCqrsTest
{
    [Fact]
    public async Task Repository_DeactivateAsync_Should_ReturnTrue_WhenTariffExists()
    {
        // Arrange
        var tariff = await SeedTariffAsync("Active", 100m);

        // Act
        var result = await TariffRepository.DeactivateAsync(tariff.TariffId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task Repository_DeactivateAsync_Should_SetIsActiveToFalse()
    {
        // Arrange
        var tariff = await SeedTariffAsync("To Deactivate", 100m);

        // Act
        await TariffRepository.DeactivateAsync(tariff.TariffId);

        // Assert
        var fromDb = await Context.Tariffs.FindAsync(tariff.TariffId);
        fromDb.Should().NotBeNull();
        fromDb!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Repository_DeactivateAsync_Should_ReturnFalse_WhenTariffNotExists()
    {
        // Arrange
        var nonExistentId = 99999;

        // Act
        var result = await TariffRepository.DeactivateAsync(nonExistentId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task Repository_DeactivateAsync_Should_UpdateLastModified()
    {
        // Arrange
        var tariff = await SeedTariffAsync("Test", 100m);
        var originalModified = tariff.LastModified;
        await Task.Delay(100);

        // Act
        await TariffRepository.DeactivateAsync(tariff.TariffId);

        // Assert
        var fromDb = await Context.Tariffs.FindAsync(tariff.TariffId);
        fromDb!.LastModified.Should().BeAfter(originalModified);
    }

    [Fact]
    public async Task Repository_DeactivateAsync_Should_InvalidateCache()
    {
        // Arrange
        var tariff = await SeedTariffAsync("Cache Test", 100m);

        // Act
        await TariffRepository.DeactivateAsync(tariff.TariffId);

        // Assert
        CacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }
}
