namespace Venue.TimeCafe.Test.Integration.Endpoints.Tariffs;

public class GetTariffByIdTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_GetTariffById_Should_Return200_WhenTariffExists()
    {
        await ClearDatabaseAndCacheAsync();
        var tariff = await SeedTariffAsync("Существующий тариф", 15m);

        var response = await Client.GetAsync($"/tariffs/{tariff.TariffId}");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("tariff", out var tariffJson).Should().BeTrue();
            tariffJson.GetProperty("tariffId").GetInt32().Should().Be(tariff.TariffId);
            tariffJson.GetProperty("name").GetString().Should().Be("Существующий тариф");
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetTariffById_Should_Return200_WhenTariffExists] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_GetTariffById_Should_Return404_WhenTariffNotFound()
    {
        await ClearDatabaseAndCacheAsync();

        var response = await Client.GetAsync("/tariffs/9999");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetTariffById_Should_Return404_WhenTariffNotFound] Response: {jsonString}");
            throw;
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-999)]
    public async Task Endpoint_GetTariffById_Should_Return422_WhenTariffIdIsInvalid(int invalidId)
    {
        await ClearDatabaseAndCacheAsync();

        var response = await Client.GetAsync($"/tariffs/{invalidId}");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetTariffById_Should_Return422_WhenTariffIdIsInvalid({invalidId})] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_GetTariffById_Should_ReturnAllProperties_WhenTariffExists()
    {
        await ClearDatabaseAndCacheAsync();
        var tariff = await SeedTariffAsync("Детальный тариф", 25m);

        var response = await Client.GetAsync($"/tariffs/{tariff.TariffId}");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            var tariffJson = json.GetProperty("tariff");
            tariffJson.GetProperty("tariffId").GetInt32().Should().Be(tariff.TariffId);
            tariffJson.GetProperty("name").GetString().Should().Be("Детальный тариф");
            tariffJson.GetProperty("pricePerMinute").GetDecimal().Should().Be(25m);
            tariffJson.TryGetProperty("themeId", out _).Should().BeTrue();
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetTariffById_Should_ReturnAllProperties_WhenTariffExists] Response: {jsonString}");
            throw;
        }
    }
}
