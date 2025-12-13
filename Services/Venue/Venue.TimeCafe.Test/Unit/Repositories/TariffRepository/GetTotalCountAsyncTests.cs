namespace Venue.TimeCafe.Test.Unit.Repositories.TariffRepository;

public class GetTotalCountAsyncTests : BaseCqrsTest
{
    [Fact]
    public async Task Repository_GetTotalCountAsync_Should_ReturnCorrectCount()
    {
        // Arrange
        await SeedTariffAsync(TestData.ExistingTariffs.Tariff1Name, TestData.ExistingTariffs.Tariff1PricePerMinute);
        await SeedTariffAsync(TestData.ExistingTariffs.Tariff2Name, TestData.ExistingTariffs.Tariff2PricePerMinute);
        await SeedTariffAsync(TestData.ExistingTariffs.Tariff3Name, TestData.ExistingTariffs.Tariff3PricePerMinute);

        // Act
        var result = await TariffRepository.GetTotalCountAsync();

        // Assert
        result.Should().Be(3);
    }

    [Fact]
    public async Task Repository_GetTotalCountAsync_Should_ReturnZero_WhenNoTariffs()
    {
        // Act
        var result = await TariffRepository.GetTotalCountAsync();

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async Task Repository_GetTotalCountAsync_Should_CountBothActiveAndInactive()
    {
        // Arrange
        await SeedTariffAsync(TestData.ExistingTariffs.Tariff1Name, TestData.ExistingTariffs.Tariff1PricePerMinute);

        var inactiveTariff = new Tariff
        {
            Name = TestData.ExistingTariffs.Tariff2Name,
            PricePerMinute = TestData.ExistingTariffs.Tariff2PricePerMinute,
            BillingType = BillingType.PerMinute,
            IsActive = false,
            CreatedAt = DateTimeOffset.UtcNow
        };
        Context.Tariffs.Add(inactiveTariff);
        await Context.SaveChangesAsync();

        // Act
        var result = await TariffRepository.GetTotalCountAsync();

        // Assert
        result.Should().Be(2);
    }

    [Fact]
    public async Task Repository_GetTotalCountAsync_Should_UpdateAfterCreate()
    {
        // Arrange
        await SeedTariffAsync(TestData.DefaultValues.DefaultTariffName, TestData.DefaultValues.DefaultTariffPrice);
        var countBefore = await TariffRepository.GetTotalCountAsync();

        // Act
        await SeedTariffAsync(TestData.NewTariffs.NewTariff1Name, TestData.NewTariffs.NewTariff1Price);
        var countAfter = await TariffRepository.GetTotalCountAsync();

        // Assert
        countBefore.Should().Be(1);
        countAfter.Should().Be(2);
    }

    [Fact]
    public async Task Repository_GetTotalCountAsync_Should_UpdateAfterDelete()
    {
        // Arrange
        var tariff1 = await SeedTariffAsync(TestData.ExistingTariffs.Tariff3Name, TestData.ExistingTariffs.Tariff3PricePerMinute);
        var tariff2 = await SeedTariffAsync(TestData.NewTariffs.NewTariff2Name, TestData.NewTariffs.NewTariff2Price);
        var countBefore = await TariffRepository.GetTotalCountAsync();

        // Act
        await TariffRepository.DeleteAsync(tariff2.TariffId);
        var countAfter = await TariffRepository.GetTotalCountAsync();

        // Assert
        countBefore.Should().Be(2);
        countAfter.Should().Be(1);
    }
}
