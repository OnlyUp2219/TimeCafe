namespace Venue.TimeCafe.Test.Integration.Endpoints.Tariffs;

public class GetActiveTariffsTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_GetActiveTariffs_Should_Return200_WhenActiveTariffsExist()
    {
        await ClearDatabaseAndCacheAsync();
        await SeedTariffAsync(TestData.NewTariffs.NewTariff1Name, TestData.NewTariffs.NewTariff1Price, isActive: true);
        await SeedTariffAsync(TestData.NewTariffs.NewTariff2Name, TestData.NewTariffs.NewTariff2Price, isActive: false);
        await SeedTariffAsync(TestData.ExistingTariffs.Tariff3Name, TestData.ExistingTariffs.Tariff3PricePerMinute, isActive: true);

        var response = await Client.GetAsync("/venue/tariffs/active");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("tariffs", out var tariffs).Should().BeTrue();
            tariffs.ValueKind.Should().Be(JsonValueKind.Array);
            var activeTariffs = tariffs.EnumerateArray().Where(t => (t.TryGetProperty("isActive", out var a) && a.GetBoolean()) || (t.TryGetProperty("tariffIsActive", out var ai) && ai.GetBoolean())).ToList();
            activeTariffs.Should().HaveCountGreaterThanOrEqualTo(2);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetActiveTariffs_Should_Return200_WhenActiveTariffsExist] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_GetActiveTariffs_Should_Return200_WhenNoActiveTariffsExist()
    {
        await ClearDatabaseAndCacheAsync();
        await SeedTariffAsync(TestData.NewTariffs.NewTariff1Name, TestData.NewTariffs.NewTariff1Price, isActive: false);
        await SeedTariffAsync(TestData.NewTariffs.NewTariff2Name, TestData.NewTariffs.NewTariff2Price, isActive: false);

        var response = await Client.GetAsync("/venue/tariffs/active");
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
            Console.WriteLine($"[Endpoint_GetActiveTariffs_Should_Return200_WhenNoActiveTariffsExist] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_GetActiveTariffs_Should_OnlyReturnActiveTariffs_WhenBothExist()
    {
        await ClearDatabaseAndCacheAsync();
        await SeedTariffAsync(TestData.NewTariffs.NewTariff1Name, TestData.NewTariffs.NewTariff1Price, isActive: true);
        await SeedTariffAsync(TestData.NewTariffs.NewTariff2Name, TestData.NewTariffs.NewTariff2Price, isActive: false);

        var response = await Client.GetAsync("/venue/tariffs/active");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("tariffs", out var tariffs).Should().BeTrue();
            foreach (var tariff in tariffs.EnumerateArray())
            {
                var isActive = (tariff.TryGetProperty("isActive", out var a) && a.GetBoolean()) || (tariff.TryGetProperty("tariffIsActive", out var ai) && ai.GetBoolean());
                isActive.Should().BeTrue();
            }
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetActiveTariffs_Should_OnlyReturnActiveTariffs_WhenBothExist] Response: {jsonString}");
            throw;
        }
    }
}
