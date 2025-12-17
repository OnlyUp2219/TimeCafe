namespace Venue.TimeCafe.Test.Integration.Endpoints.Tariffs;

public class GetAllTariffsTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_GetAllTariffs_Should_Return200_WhenTariffsExist()
    {
        await ClearDatabaseAndCacheAsync();
        await SeedTariffAsync(TestData.NewTariffs.NewTariff1Name, TestData.NewTariffs.NewTariff1Price);
        await SeedTariffAsync(TestData.NewTariffs.NewTariff2Name, TestData.NewTariffs.NewTariff2Price);
        await SeedTariffAsync(TestData.ExistingTariffs.Tariff3Name, TestData.ExistingTariffs.Tariff3PricePerMinute);

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
        var tariff = await SeedTariffAsync(TestData.NewTariffs.NewTariff2Name, TestData.NewTariffs.NewTariff2Price);

        var response = await Client.GetAsync("/tariffs");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            var tariffs = json.GetProperty("tariffs");
            var foundTariff = tariffs.EnumerateArray().FirstOrDefault(t => t.GetProperty("tariffName").GetString() == TestData.NewTariffs.NewTariff2Name);

            foundTariff.ValueKind.Should().NotBe(JsonValueKind.Undefined);
            foundTariff.GetProperty("tariffId").GetString().Should().Be(tariff.TariffId.ToString());
            foundTariff.GetProperty("tariffName").GetString().Should().Be(TestData.NewTariffs.NewTariff2Name);
            foundTariff.GetProperty("tariffPricePerMinute").GetDecimal().Should().Be(TestData.NewTariffs.NewTariff2Price);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetAllTariffs_Should_ReturnAllProperties_WhenTariffsExist] Response: {jsonString}");
            throw;
        }
    }
}
