namespace Venue.TimeCafe.Test.Integration.Endpoints.Tariffs;

public class UpdateTariffTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_UpdateTariff_Should_Return200_WhenTariffExists()
    {
        await ClearDatabaseAndCacheAsync();
        var theme = await SeedThemeAsync(TestData.DefaultValues.DefaultThemeName);
        var tariff = await SeedTariffAsync(TestData.NewTariffs.NewTariff1Name, TestData.NewTariffs.NewTariff1Price);
        var updatedName = TestData.NewTariffs.NewTariff1Name + " - updated";
        var payload = new
        {
            name = updatedName,
            description = tariff.Description,
            pricePerMinute = tariff.PricePerMinute,
            billingType = (int)TestData.NewTariffs.NewTariff1BillingType,
            themeId = theme.ThemeId,
            isActive = tariff.IsActive,
            lastModified = DateTimeOffset.UtcNow
        };

        var response = await Client.PutAsJsonAsync($"/venue/tariffs/{tariff.TariffId}", payload);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;


            json.GetProperty("name").GetString().Should().Be(updatedName);
        }
        catch (Exception)
        {
            Console.WriteLine($"Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_UpdateTariff_Should_Return404_WhenTariffNotFound()
    {
        await ClearDatabaseAndCacheAsync();
        var theme = await SeedThemeAsync("Тема");
        var payload = new
        {
            name = "Несуществующий",
            description = "Описание",
            pricePerMinute = 10m,
            billingType = 2,
            themeId = theme.ThemeId,
            isActive = true,
            lastModified = DateTimeOffset.UtcNow
        };

        var response = await Client.PutAsJsonAsync($"/venue/tariffs/{TestData.NonExistingIds.NonExistingTariffIdString}", payload);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
        catch (Exception)
        {
            Console.WriteLine($"Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_UpdateTariff_Should_UpdateOnlyChangedFields_WhenPartialUpdateProvided()
    {
        await ClearDatabaseAndCacheAsync();
        var theme = await SeedThemeAsync("Тема");
        var tariff = await SeedTariffAsync(TestData.NewTariffs.NewTariff1Name, TestData.NewTariffs.NewTariff1Price);
        var payload = new
        {
            name = TestData.NewTariffs.NewTariff1Name + " - updated",
            description = tariff.Description,
            pricePerMinute = tariff.PricePerMinute,
            billingType = (int)TestData.NewTariffs.NewTariff1BillingType,
            themeId = theme.ThemeId,
            isActive = tariff.IsActive,
        };

        var response = await Client.PutAsJsonAsync($"/venue/tariffs/{tariff.TariffId}", payload);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;

            json.GetProperty("name").GetString().Should().Be(TestData.NewTariffs.NewTariff1Name + " - updated");
            json.GetProperty("pricePerMinute").GetDecimal().Should().Be(tariff.PricePerMinute);
        }
        catch (Exception)
        {
            Console.WriteLine($"Response: {jsonString}");
            throw;
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Endpoint_UpdateTariff_Should_Return422_WhenNameIsInvalid(string? invalidName)
    {
        await ClearDatabaseAndCacheAsync();
        var theme = await SeedThemeAsync("Тема");
        var tariff = await SeedTariffAsync(TestData.NewTariffs.NewTariff1Name, TestData.NewTariffs.NewTariff1Price);
        var payload = new
        {
            name = invalidName,
            description = "Описание",
            pricePerMinute = 10m,
            billingType = 2,
            themeId = theme.ThemeId,
            isActive = true,
            lastModified = DateTimeOffset.UtcNow
        };

        var response = await Client.PutAsJsonAsync($"/venue/tariffs/{tariff.TariffId}", payload);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }
        catch (Exception)
        {
            Console.WriteLine($"Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_UpdateTariff_Should_Return422_WhenTariffIdIsEmpty()
    {
        await ClearDatabaseAndCacheAsync();
        var theme = await SeedThemeAsync("Тема");
        var payload = new
        {
            name = TestData.DefaultValues.DefaultTariffName,
            description = TestData.DefaultValues.DefaultTariffName,
            pricePerMinute = 10m,
            billingType = (int)TestData.DefaultValues.DefaultBillingType,
            themeId = theme.ThemeId,
            isActive = true,
            lastModified = DateTimeOffset.UtcNow
        };

        var response = await Client.PutAsJsonAsync($"/venue/tariffs/{Guid.Empty}", payload);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }
        catch (Exception)
        {
            Console.WriteLine($"Response: {jsonString}");
            throw;
        }
    }
}













