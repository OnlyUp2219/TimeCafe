namespace Venue.TimeCafe.Test.Integration.Endpoints.Tariffs;

public class GetTariffsPageTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_GetTariffsPage_Should_Return200_WhenTariffsExist()
    {
        await ClearDatabaseAndCacheAsync();
        await SeedTariffAsync("Тариф 1", 10m);
        await SeedTariffAsync("Тариф 2", 20m);
        await SeedTariffAsync("Тариф 3", 30m);

        var response = await Client.GetAsync("/tariffs/page?pageNumber=1&pageSize=10");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("tariffs", out var tariffs).Should().BeTrue();
            json.TryGetProperty("totalCount", out var totalCount).Should().BeTrue();
            tariffs.ValueKind.Should().Be(JsonValueKind.Array);
            totalCount.GetInt32().Should().BeGreaterThanOrEqualTo(3);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetTariffsPage_Should_Return200_WhenTariffsExist] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_GetTariffsPage_Should_Return200_WhenNoTariffsExist()
    {
        await ClearDatabaseAndCacheAsync();

        var response = await Client.GetAsync("/tariffs/page?pageNumber=1&pageSize=10");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("tariffs", out var tariffs).Should().BeTrue();
            json.TryGetProperty("totalCount", out var totalCount).Should().BeTrue();
            tariffs.ValueKind.Should().Be(JsonValueKind.Array);
            tariffs.GetArrayLength().Should().Be(0);
            totalCount.GetInt32().Should().Be(0);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetTariffsPage_Should_Return200_WhenNoTariffsExist] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_GetTariffsPage_Should_RespectPageSize_WhenCalled()
    {
        await ClearDatabaseAndCacheAsync();
        for (int i = 1; i <= 5; i++)
        {
            await SeedTariffAsync($"Тариф {i}", i * 10m);
        }

        var response = await Client.GetAsync("/tariffs/page?pageNumber=1&pageSize=2");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            var tariffs = json.GetProperty("tariffs");
            tariffs.GetArrayLength().Should().BeLessThanOrEqualTo(2);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetTariffsPage_Should_RespectPageSize_WhenCalled] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_GetTariffsPage_Should_ReturnCorrectTotalCount_WhenCalled()
    {
        await ClearDatabaseAndCacheAsync();
        await SeedTariffAsync("Тариф 1", 10m);
        await SeedTariffAsync("Тариф 2", 20m);
        await SeedTariffAsync("Тариф 3", 30m);

        var response = await Client.GetAsync("/tariffs/page?pageNumber=1&pageSize=2");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            var totalCount = json.GetProperty("totalCount").GetInt32();
            totalCount.Should().BeGreaterThanOrEqualTo(3);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetTariffsPage_Should_ReturnCorrectTotalCount_WhenCalled] Response: {jsonString}");
            throw;
        }
    }
}
