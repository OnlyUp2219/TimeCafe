namespace Venue.TimeCafe.Test.Integration.Endpoints.Tariffs;

public class UpdateTariffTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_UpdateTariff_Should_Return200_WhenTariffExists()
    {
        await ClearDatabaseAndCacheAsync();
        var theme = await SeedThemeAsync("Тема");
        var tariff = await SeedTariffAsync("Старое название", 10m);
        var payload = new
        {
            tariffId = tariff.TariffId,
            name = "Новое название",
            description = tariff.Description,
            pricePerMinute = tariff.PricePerMinute,
            billingType = 2,
            themeId = theme.ThemeId,
            isActive = tariff.IsActive,
            lastModified = DateTime.UtcNow
        };

        var response = await Client.PutAsJsonAsync("/tariffs", payload);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("message", out _).Should().BeTrue();
            json.TryGetProperty("tariff", out var tariffJson).Should().BeTrue();
            tariffJson.GetProperty("name").GetString().Should().Be("Новое название");
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_UpdateTariff_Should_Return200_WhenTariffExists] Response: {jsonString}");
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
            tariffId = 9999,
            name = "Несуществующий",
            description = "Описание",
            pricePerMinute = 10m,
            billingType = 2,
            themeId = theme.ThemeId,
            isActive = true,
            lastModified = DateTime.UtcNow
        };

        var response = await Client.PutAsJsonAsync("/tariffs", payload);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_UpdateTariff_Should_Return404_WhenTariffNotFound] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_UpdateTariff_Should_UpdateOnlyChangedFields_WhenPartialUpdateProvided()
    {
        await ClearDatabaseAndCacheAsync();
        var theme = await SeedThemeAsync("Тема");
        var tariff = await SeedTariffAsync("Название до обновления", 10m);
        var payload = new
        {
            tariffId = tariff.TariffId,
            name = "Название после обновления",
            description = tariff.Description,
            pricePerMinute = tariff.PricePerMinute,
            billingType = 2,
            themeId = theme.ThemeId,
            isActive = tariff.IsActive,
            lastModified = DateTime.UtcNow
        };

        var response = await Client.PutAsJsonAsync("/tariffs", payload);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            var updatedTariff = json.GetProperty("tariff");
            updatedTariff.GetProperty("name").GetString().Should().Be("Название после обновления");
            updatedTariff.GetProperty("pricePerMinute").GetDecimal().Should().Be(10m);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_UpdateTariff_Should_UpdateOnlyChangedFields_WhenPartialUpdateProvided] Response: {jsonString}");
            throw;
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Endpoint_UpdateTariff_Should_Return422_WhenNameIsInvalid(string invalidName)
    {
        await ClearDatabaseAndCacheAsync();
        var theme = await SeedThemeAsync("Тема");
        var tariff = await SeedTariffAsync("Тариф", 10m);
        var payload = new
        {
            tariffId = tariff.TariffId,
            name = invalidName,
            description = "Описание",
            pricePerMinute = 10m,
            billingType = 2,
            themeId = theme.ThemeId,
            isActive = true,
            lastModified = DateTime.UtcNow
        };

        var response = await Client.PutAsJsonAsync("/tariffs", payload);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_UpdateTariff_Should_Return422_WhenNameIsInvalid] Response: {jsonString}");
            throw;
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task Endpoint_UpdateTariff_Should_Return422_WhenTariffIdIsInvalid(int invalidId)
    {
        await ClearDatabaseAndCacheAsync();
        var theme = await SeedThemeAsync("Тема");
        var payload = new
        {
            tariffId = invalidId,
            name = "Тариф",
            description = "Описание",
            pricePerMinute = 10m,
            billingType = 2,
            themeId = theme.ThemeId,
            isActive = true,
            lastModified = DateTime.UtcNow
        };

        var response = await Client.PutAsJsonAsync("/tariffs", payload);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_UpdateTariff_Should_Return422_WhenTariffIdIsInvalid({invalidId})] Response: {jsonString}");
            throw;
        }
    }
}
