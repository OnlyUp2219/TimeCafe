namespace Venue.TimeCafe.Test.Integration.Endpoints.Tariffs;

public class CreateTariffTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_CreateTariff_Should_Return201_WhenValid()
    {
        await ClearDatabaseAndCacheAsync();
        var theme = await SeedThemeAsync(TestData.DefaultValues.DefaultThemeName);
        var payload = new
        {
            name = TestData.NewTariffs.NewTariff1Name,
            description = TestData.DefaultValues.DefaultTariffName,
            pricePerMinute = 10m,
            billingType = (int)TestData.NewTariffs.NewTariff1BillingType,
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
            tariff.GetProperty("name").GetString().Should().Be(TestData.NewTariffs.NewTariff1Name);
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
        var theme = await SeedThemeAsync(TestData.DefaultValues.DefaultThemeName);
        var payload = new
        {
            name = invalidName,
            description = TestData.DefaultValues.DefaultTariffName,
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
        var theme = await SeedThemeAsync(TestData.DefaultValues.DefaultThemeName);
        var longName = new string('A', 101);
        var payload = new
        {
            name = longName,
            description = TestData.DefaultValues.DefaultTariffName,
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
        var theme = await SeedThemeAsync(TestData.DefaultValues.DefaultThemeName);
        var payload = new
        {
            name = TestData.DefaultValues.DefaultTariffName,
            description = TestData.DefaultValues.DefaultTariffName,
            pricePerMinute = invalidPrice,
            billingType = (int)TestData.NewTariffs.NewTariff1BillingType,
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
        var theme = await SeedThemeAsync(TestData.DefaultValues.DefaultThemeName);
        var payload = new
        {
            name = TestData.NewTariffs.NewTariff2Name,
            description = TestData.DefaultValues.DefaultTariffName,
            pricePerMinute = TestData.NewTariffs.NewTariff2Price,
            billingType = (int)TestData.NewTariffs.NewTariff2BillingType,
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
            tariff.GetProperty("tariffId").GetString().Should().NotBeNullOrWhiteSpace();
            tariff.GetProperty("name").GetString().Should().Be(TestData.NewTariffs.NewTariff2Name);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_CreateTariff_Should_ReturnTariffWithId_WhenCreated] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_CreateTariff_Should_Return404_WhenThemeDoesNotExist()
    {
        await ClearDatabaseAndCacheAsync();
        var payload = new
        {
            name = TestData.NewTariffs.NewTariff2Name,
            description = TestData.DefaultValues.DefaultTariffName,
            pricePerMinute = TestData.NewTariffs.NewTariff2Price,
            billingType = (int)TestData.NewTariffs.NewTariff2BillingType,
            themeId = TestData.NonExistingIds.NonExistingThemeId,
            isActive = true
        };

        var response = await Client.PostAsJsonAsync("/tariffs", payload);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_CreateTariff_Should_Return404_WhenThemeDoesNotExist] Response: {jsonString}");
            throw;
        }
    }
}
