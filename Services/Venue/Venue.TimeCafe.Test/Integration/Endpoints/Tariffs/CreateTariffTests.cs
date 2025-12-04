namespace Venue.TimeCafe.Test.Integration.Endpoints.Tariffs;

public class CreateTariffTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_CreateTariff_Should_Return201_WhenValid()
    {
        await ClearDatabaseAndCacheAsync();
        var theme = await SeedThemeAsync("Тема для тарифа");
        var payload = new
        {
            name = "Новый тариф",
            description = "Описание тарифа",
            pricePerMinute = 10m,
            billingType = 2,
            themeId = theme.ThemeId,
            isActive = true
        };

        var response = await Client.PostAsJsonAsync("/tariffs", payload);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("message", out _).Should().BeTrue();
            json.TryGetProperty("tariff", out var tariff).Should().BeTrue();
            tariff.GetProperty("name").GetString().Should().Be("Новый тариф");
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_CreateTariff_Should_Return201_WhenValid] Response: {jsonString}");
            throw;
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Endpoint_CreateTariff_Should_Return422_WhenNameIsInvalid(string invalidName)
    {
        await ClearDatabaseAndCacheAsync();
        var theme = await SeedThemeAsync("Тема");
        var payload = new
        {
            name = invalidName,
            description = "Описание",
            pricePerMinute = 10m,
            billingType = 2,
            themeId = theme.ThemeId,
            isActive = true
        };

        var response = await Client.PostAsJsonAsync("/tariffs", payload);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_CreateTariff_Should_Return422_WhenNameIsInvalid] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_CreateTariff_Should_Return422_WhenNameExceedsMaxLength()
    {
        await ClearDatabaseAndCacheAsync();
        var theme = await SeedThemeAsync("Тема");
        var longName = new string('A', 101);
        var payload = new
        {
            name = longName,
            description = "Описание",
            pricePerMinute = 10m,
            billingType = 2,
            themeId = theme.ThemeId,
            isActive = true
        };

        var response = await Client.PostAsJsonAsync("/tariffs", payload);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_CreateTariff_Should_Return422_WhenNameExceedsMaxLength] Response: {jsonString}");
            throw;
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10.5)]
    public async Task Endpoint_CreateTariff_Should_Return422_WhenPriceIsInvalid(decimal invalidPrice)
    {
        await ClearDatabaseAndCacheAsync();
        var theme = await SeedThemeAsync("Тема");
        var payload = new
        {
            name = "Тариф",
            description = "Описание",
            pricePerMinute = invalidPrice,
            billingType = 2,
            themeId = theme.ThemeId,
            isActive = true
        };

        var response = await Client.PostAsJsonAsync("/tariffs", payload);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_CreateTariff_Should_Return422_WhenPriceIsInvalid] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_CreateTariff_Should_ReturnTariffWithId_WhenCreated()
    {
        await ClearDatabaseAndCacheAsync();
        var theme = await SeedThemeAsync("Тема");
        var payload = new
        {
            name = "Тариф с ID",
            description = "Описание",
            pricePerMinute = 15m,
            billingType = 2,
            themeId = theme.ThemeId,
            isActive = true
        };

        var response = await Client.PostAsJsonAsync("/tariffs", payload);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var json = JsonDocument.Parse(jsonString).RootElement;
            var tariff = json.GetProperty("tariff");
            tariff.GetProperty("tariffId").GetInt32().Should().BeGreaterThan(0);
            tariff.GetProperty("name").GetString().Should().Be("Тариф с ID");
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_CreateTariff_Should_ReturnTariffWithId_WhenCreated] Response: {jsonString}");
            throw;
        }
    }
}
