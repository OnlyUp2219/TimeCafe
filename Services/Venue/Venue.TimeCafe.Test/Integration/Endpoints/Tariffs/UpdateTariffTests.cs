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
            tariffId = tariff.TariffId,
            name = updatedName,
            description = tariff.Description,
            pricePerMinute = tariff.PricePerMinute,
            billingType = (int)TestData.NewTariffs.NewTariff1BillingType,
            themeId = theme.ThemeId,
            isActive = tariff.IsActive,
            lastModified = DateTimeOffset.UtcNow
        };

        var response = await Client.PutAsJsonAsync("/venue/tariffs", payload);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("message", out _).Should().BeTrue();
            json.TryGetProperty("tariff", out var tariffJson).Should().BeTrue();
            tariffJson.GetProperty("name").GetString().Should().Be(updatedName);
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
            tariffId = TestData.NonExistingIds.NonExistingTariffIdString,
            name = "Несуществующий",
            description = "Описание",
            pricePerMinute = 10m,
            billingType = 2,
            themeId = theme.ThemeId,
            isActive = true,
            lastModified = DateTimeOffset.UtcNow
        };

        var response = await Client.PutAsJsonAsync("/venue/tariffs", payload);
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
            tariffId = tariff.TariffId,
            name = TestData.NewTariffs.NewTariff1Name + " - updated",
            description = tariff.Description,
            pricePerMinute = tariff.PricePerMinute,
            billingType = (int)TestData.NewTariffs.NewTariff1BillingType,
            themeId = theme.ThemeId,
            isActive = tariff.IsActive,
        };

        var response = await Client.PutAsJsonAsync("/venue/tariffs", payload);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            var updatedTariff = json.GetProperty("tariff");
            updatedTariff.GetProperty("name").GetString().Should().Be(TestData.NewTariffs.NewTariff1Name + " - updated");
            updatedTariff.GetProperty("pricePerMinute").GetDecimal().Should().Be(tariff.PricePerMinute);
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
            tariffId = tariff.TariffId,
            name = invalidName,
            description = "Описание",
            pricePerMinute = 10m,
            billingType = 2,
            themeId = theme.ThemeId,
            isActive = true,
            lastModified = DateTimeOffset.UtcNow
        };

        var response = await Client.PutAsJsonAsync("/venue/tariffs", payload);
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

    [Theory]
    //[InlineData("")]
    [InlineData("not-a-guid")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public async Task Endpoint_UpdateTariff_Should_Return422_WhenTariffIdIsInvalid(string invalidId)
    {
        await ClearDatabaseAndCacheAsync();
        var theme = await SeedThemeAsync("Тема");
        var payload = new
        {
            tariffId = invalidId,
            name = TestData.DefaultValues.DefaultTariffName,
            description = TestData.DefaultValues.DefaultTariffName,
            pricePerMinute = 10m,
            billingType = (int)TestData.DefaultValues.DefaultBillingType,
            themeId = theme.ThemeId,
            isActive = true,
            lastModified = DateTimeOffset.UtcNow
        };

        var response = await Client.PutAsJsonAsync("/venue/tariffs", payload);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }
        catch (Exception)
        {
            Console.WriteLine($"Response: {jsonString} ({invalidId})");
            throw;
        }
    }
}
