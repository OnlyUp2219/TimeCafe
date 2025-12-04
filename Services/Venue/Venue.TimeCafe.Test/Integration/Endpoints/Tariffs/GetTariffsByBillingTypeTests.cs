namespace Venue.TimeCafe.Test.Integration.Endpoints.Tariffs;

public class GetTariffsByBillingTypeTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_GetTariffsByBillingType_Should_Return200_WhenTariffsExist()
    {
        await ClearDatabaseAndCacheAsync();
        await SeedTariffAsync("Поминутный 1", 10m, BillingType.PerMinute);
        await SeedTariffAsync("Почасовой", 60m, BillingType.Hourly);
        await SeedTariffAsync("Поминутный 2", 15m, BillingType.PerMinute);

        var response = await Client.GetAsync("/tariffs/billing-type/2");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("tariffs", out var tariffs).Should().BeTrue();
            tariffs.ValueKind.Should().Be(JsonValueKind.Array);
            tariffs.GetArrayLength().Should().BeGreaterThanOrEqualTo(2);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetTariffsByBillingType_Should_Return200_WhenTariffsExist] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_GetTariffsByBillingType_Should_Return200_WhenNoTariffsExist()
    {
        await ClearDatabaseAndCacheAsync();
        await SeedTariffAsync("Поминутный", 10m, BillingType.PerMinute);

        var response = await Client.GetAsync("/tariffs/billing-type/1");
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
            Console.WriteLine($"[Endpoint_GetTariffsByBillingType_Should_Return200_WhenNoTariffsExist] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_GetTariffsByBillingType_Should_OnlyReturnMatchingBillingType_WhenCalled()
    {
        await ClearDatabaseAndCacheAsync();
        await SeedTariffAsync("Поминутный", 10m, BillingType.PerMinute);
        await SeedTariffAsync("Почасовой", 60m, BillingType.Hourly);

        var response = await Client.GetAsync("/tariffs/billing-type/2");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            var tariffs = json.GetProperty("tariffs");
            foreach (var tariff in tariffs.EnumerateArray())
            {
                tariff.GetProperty("billingType").GetInt32().Should().Be(2);
            }
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetTariffsByBillingType_Should_OnlyReturnMatchingBillingType_WhenCalled] Response: {jsonString}");
            throw;
        }
    }
}
