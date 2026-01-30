namespace Venue.TimeCafe.Test.Integration.Endpoints.Tariffs;

public class GetTariffsByBillingTypeTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_GetTariffsByBillingType_Should_Return200_WhenTariffsExist()
    {
        await ClearDatabaseAndCacheAsync();
        await SeedTariffAsync(TestData.NewTariffs.NewTariff1Name, TestData.NewTariffs.NewTariff1Price, BillingType.PerMinute);
        await SeedTariffAsync(TestData.NewTariffs.NewTariff2Name, TestData.NewTariffs.NewTariff2Price, BillingType.Hourly);
        await SeedTariffAsync(TestData.ExistingTariffs.Tariff3Name, TestData.ExistingTariffs.Tariff3PricePerMinute, BillingType.PerMinute);

        var response = await Client.GetAsync("/venue/tariffs/billing-type/2");
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
        await SeedTariffAsync(TestData.NewTariffs.NewTariff1Name, TestData.NewTariffs.NewTariff1Price, BillingType.PerMinute);

        var response = await Client.GetAsync("/venue/tariffs/billing-type/1");
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
        await SeedTariffAsync(TestData.NewTariffs.NewTariff1Name, TestData.NewTariffs.NewTariff1Price, BillingType.PerMinute);
        await SeedTariffAsync(TestData.NewTariffs.NewTariff2Name, TestData.NewTariffs.NewTariff2Price, BillingType.Hourly);

        var response = await Client.GetAsync("/venue/tariffs/billing-type/2");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("tariffs", out var tariffs).Should().BeTrue();
            foreach (var tariff in tariffs.EnumerateArray())
            {
                int billing = -1;
                if (tariff.TryGetProperty("billingType", out var b)) billing = b.GetInt32();
                if (tariff.TryGetProperty("tariffBillingType", out var tb)) billing = tb.GetInt32();
                billing.Should().Be(2);
            }
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetTariffsByBillingType_Should_OnlyReturnMatchingBillingType_WhenCalled] Response: {jsonString}");
            throw;
        }
    }
}
