namespace Venue.TimeCafe.Test.Integration.Endpoints.Tariffs;

public class GetActiveTariffsTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_GetActiveTariffs_Should_Return200_WhenActiveTariffsExist()
    {
        await ClearDatabaseAndCacheAsync();
        await SeedTariffAsync("Активный 1", 10m, isActive: true);
        await SeedTariffAsync("Неактивный", 20m, isActive: false);
        await SeedTariffAsync("Активный 2", 30m, isActive: true);

        var response = await Client.GetAsync("/tariffs/active");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("tariffs", out var tariffs).Should().BeTrue();
            tariffs.ValueKind.Should().Be(JsonValueKind.Array);
            var activeTariffs = tariffs.EnumerateArray().Where(t => t.GetProperty("isActive").GetBoolean()).ToList();
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
        await SeedTariffAsync("Неактивный 1", 10m, isActive: false);
        await SeedTariffAsync("Неактивный 2", 20m, isActive: false);

        var response = await Client.GetAsync("/tariffs/active");
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
        await SeedTariffAsync("Активный", 10m, isActive: true);
        await SeedTariffAsync("Неактивный", 20m, isActive: false);

        var response = await Client.GetAsync("/tariffs/active");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            var tariffs = json.GetProperty("tariffs");
            foreach (var tariff in tariffs.EnumerateArray())
            {
                tariff.GetProperty("isActive").GetBoolean().Should().BeTrue();
            }
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetActiveTariffs_Should_OnlyReturnActiveTariffs_WhenBothExist] Response: {jsonString}");
            throw;
        }
    }
}
