namespace Venue.TimeCafe.Test.Integration.Endpoints.Tariffs;

public class GetAllTariffsTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_GetAllTariffs_Should_Return200_WhenTariffsExist()
    {
        await ClearDatabaseAndCacheAsync();
        await SeedTariffAsync("Тариф 1", 10m);
        await SeedTariffAsync("Тариф 2", 20m);
        await SeedTariffAsync("Тариф 3", 30m);

        var response = await Client.GetAsync("/tariffs");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("tariffs", out var tariffs).Should().BeTrue();
            tariffs.ValueKind.Should().Be(JsonValueKind.Array);
            tariffs.GetArrayLength().Should().BeGreaterThanOrEqualTo(3);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetAllTariffs_Should_Return200_WhenTariffsExist] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_GetAllTariffs_Should_Return200_WhenNoTariffsExist()
    {
        await ClearDatabaseAndCacheAsync();
        var response = await Client.GetAsync("/tariffs");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("tariffs", out var tariffs).Should().BeTrue();
            tariffs.ValueKind.Should().Be(JsonValueKind.Array);
            tariffs.GetArrayLength().Should().Be(0);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetAllTariffs_Should_Return200_WhenNoTariffsExist] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_GetAllTariffs_Should_ReturnAllProperties_WhenTariffsExist()
    {
        await ClearDatabaseAndCacheAsync();
        var tariff = await SeedTariffAsync("Специальный тариф", 50m);

        var response = await Client.GetAsync("/tariffs");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            var tariffs = json.GetProperty("tariffs");
            var foundTariff = tariffs.EnumerateArray().FirstOrDefault(t => t.GetProperty("name").GetString() == "Специальный тариф");

            foundTariff.ValueKind.Should().NotBe(JsonValueKind.Undefined);
            foundTariff.GetProperty("tariffId").GetInt32().Should().Be(tariff.TariffId);
            foundTariff.GetProperty("name").GetString().Should().Be("Специальный тариф");
            foundTariff.GetProperty("pricePerMinute").GetDecimal().Should().Be(50m);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetAllTariffs_Should_ReturnAllProperties_WhenTariffsExist] Response: {jsonString}");
            throw;
        }
    }
}
