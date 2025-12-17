namespace Venue.TimeCafe.Test.Integration.Endpoints.Tariffs;

public class GetTariffByIdTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_GetTariffById_Should_Return200_WhenTariffExists()
    {
        await ClearDatabaseAndCacheAsync();
        var name = TestData.NewTariffs.NewTariff1Name;
        var price = TestData.NewTariffs.NewTariff1Price;
        var tariff = await SeedTariffAsync(name, 15m);

        var response = await Client.GetAsync($"/tariffs/{tariff.TariffId}");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("tariff", out var tariffJson).Should().BeTrue();
            tariffJson.GetProperty("tariffId").GetString().Should().Be(tariff.TariffId.ToString());
            tariffJson.GetProperty("tariffName").GetString().Should().Be(name);
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

        var nonExistingId = TestData.NonExistingIds.NonExistingTariffIdString;
        var response = await Client.GetAsync($"/tariffs/{nonExistingId}");
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
    //[InlineData("")]
    [InlineData("not-a-guid")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public async Task Endpoint_GetTariffById_Should_Return422_WhenTariffIdIsInvalid(string invalidId)
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
        var detailName = TestData.NewTariffs.NewTariff2Name;
        var tariff = await SeedTariffAsync(detailName, 25m);

        var response = await Client.GetAsync($"/tariffs/{tariff.TariffId}");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            var tariffJson = json.GetProperty("tariff");
            tariffJson.GetProperty("tariffId").GetString().Should().Be(tariff.TariffId.ToString());
            tariffJson.GetProperty("tariffName").GetString().Should().Be(detailName);
            tariffJson.GetProperty("tariffPricePerMinute").GetDecimal().Should().Be(25m);
            tariffJson.TryGetProperty("themeId", out _).Should().BeTrue();
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetTariffById_Should_ReturnAllProperties_WhenTariffExists] Response: {jsonString}");
            throw;
        }
    }
}
