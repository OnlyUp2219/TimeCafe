namespace Venue.TimeCafe.Test.Unit.Repositories.TariffRepository;

public class GetPagedAsyncTests : BaseCqrsTest
{
    [Fact]
    public async Task Repository_GetPagedAsync_Should_ReturnPagedResults()
    {
        // Arrange
        for (int i = 1; i <= 10; i++)
        {
            await SeedTariffAsync($"Tariff {i}", i * 100m);
        }

        // Act
        var result = await TariffRepository.GetPagedAsync(1, 5);

        // Assert
        result.Should().HaveCount(5);
    }

    [Fact]
    public async Task Repository_GetPagedAsync_Should_ReturnCorrectPage()
    {
        // Arrange
        for (int i = 1; i <= 10; i++)
        {
            await SeedTariffAsync($"Tariff {i:D2}", i * 100m);
            await Task.Delay(10);
        }

        // Act
        var page1 = (await TariffRepository.GetPagedAsync(1, 3)).ToList();
        var page2 = (await TariffRepository.GetPagedAsync(2, 3)).ToList();

        // Assert
        page1.Should().HaveCount(3);
        page2.Should().HaveCount(3);
        page1[0].TariffId.Should().NotBe(page2[0].TariffId);
    }

    [Fact]
    public async Task Repository_GetPagedAsync_Should_OrderByCreatedAtDescending()
    {
        // Arrange
        var tariff1 = await SeedTariffAsync(TestData.ExistingTariffs.Tariff1Name, TestData.ExistingTariffs.Tariff1PricePerMinute);
        await Task.Delay(10);
        var tariff2 = await SeedTariffAsync(TestData.ExistingTariffs.Tariff2Name, TestData.ExistingTariffs.Tariff2PricePerMinute);
        await Task.Delay(10);
        var tariff3 = await SeedTariffAsync(TestData.ExistingTariffs.Tariff3Name, TestData.ExistingTariffs.Tariff3PricePerMinute);

        // Act
        var result = (await TariffRepository.GetPagedAsync(1, 10)).ToList();

        // Assert
        result[0].Name.Should().Be(TestData.ExistingTariffs.Tariff3Name);
        result[1].Name.Should().Be(TestData.ExistingTariffs.Tariff2Name);
        result[2].Name.Should().Be(TestData.ExistingTariffs.Tariff1Name);
    }

    [Fact]
    public async Task Repository_GetPagedAsync_Should_ReturnEmptyList_WhenPageExceedsTotalPages()
    {
        // Arrange
        await SeedTariffAsync(TestData.DefaultValues.DefaultTariffName, TestData.DefaultValues.DefaultTariffPrice);

        // Act
        var result = await TariffRepository.GetPagedAsync(10, 5);

        // Assert
        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData(1, 10)]
    [InlineData(2, 5)]
    [InlineData(3, 3)]
    public async Task Repository_GetPagedAsync_Should_HandleDifferentPageSizes(int pageNumber, int pageSize)
    {
        // Arrange
        for (int i = 1; i <= 20; i++)
        {
            await SeedTariffAsync($"Tariff {i}", i * 100m);
        }

        // Act
        var result = await TariffRepository.GetPagedAsync(pageNumber, pageSize);

        // Assert
        result.Should().NotBeNull();
        result.Count().Should().BeLessThanOrEqualTo(pageSize);
    }

    [Fact]
    public async Task Repository_GetPagedAsync_Should_IncludeThemes()
    {
        // Arrange
        var theme = await SeedThemeAsync(TestData.ExistingThemes.Theme1Name);
        var tariff = new Tariff
        {
            Name = TestData.ExistingTariffs.Tariff1Name,
            PricePerMinute = TestData.ExistingTariffs.Tariff1PricePerMinute,
            BillingType = BillingType.PerMinute,
            ThemeId = theme.ThemeId,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };
        Context.Tariffs.Add(tariff);
        await Context.SaveChangesAsync();

        // Act
        var result = await TariffRepository.GetPagedAsync(1, 10);

        // Assert
        var tariffWithTheme = result.First();
        tariffWithTheme.ThemeId.Should().Be(theme.ThemeId);
    }
}
