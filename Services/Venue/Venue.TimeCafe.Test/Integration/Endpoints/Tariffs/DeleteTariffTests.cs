namespace Venue.TimeCafe.Test.Integration.Endpoints.Tariffs;

public class DeleteTariffTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_DeleteTariff_Should_Return200_WhenTariffExists()
    {
        await ClearDatabaseAndCacheAsync();
        var tariff = await SeedTariffAsync("Тариф для удаления", 10m);

        var response = await Client.DeleteAsync($"/tariffs/{tariff.TariffId}");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("message", out _).Should().BeTrue();
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_DeleteTariff_Should_Return200_WhenTariffExists] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_DeleteTariff_Should_Return404_WhenTariffNotFound()
    {
        await ClearDatabaseAndCacheAsync();

        var response = await Client.DeleteAsync("/tariffs/9999");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_DeleteTariff_Should_Return404_WhenTariffNotFound] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_DeleteTariff_Should_ActuallyRemoveFromDatabase_WhenDeleted()
    {
        await ClearDatabaseAndCacheAsync();
        var tariff = await SeedTariffAsync("Удаляемый тариф", 10m);

        var deleteResponse = await Client.DeleteAsync($"/tariffs/{tariff.TariffId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getResponse = await Client.GetAsync($"/tariffs/{tariff.TariffId}");
        var jsonString = await getResponse.Content.ReadAsStringAsync();
        try
        {
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_DeleteTariff_Should_ActuallyRemoveFromDatabase_WhenDeleted] Response: {jsonString}");
            throw;
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task Endpoint_DeleteTariff_Should_Return422_WhenTariffIdIsInvalid(int invalidId)
    {
        await ClearDatabaseAndCacheAsync();

        var response = await Client.DeleteAsync($"/tariffs/{invalidId}");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_DeleteTariff_Should_Return422_WhenTariffIdIsInvalid({invalidId})] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_DeleteTariff_Should_NotAffectOtherTariffs_WhenOneIsDeleted()
    {
        await ClearDatabaseAndCacheAsync();
        var tariff1 = await SeedTariffAsync("Тариф 1", 10m);
        var tariff2 = await SeedTariffAsync("Тариф 2", 20m);

        await Client.DeleteAsync($"/tariffs/{tariff1.TariffId}");

        var response = await Client.GetAsync($"/tariffs/{tariff2.TariffId}");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.GetProperty("tariff").GetProperty("name").GetString().Should().Be("Тариф 2");
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_DeleteTariff_Should_NotAffectOtherTariffs_WhenOneIsDeleted] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_DeleteTariff_Should_ReturnSuccessMessage_WhenTariffDeleted()
    {
        await ClearDatabaseAndCacheAsync();
        var tariff = await SeedTariffAsync("Тариф с сообщением", 10m);

        var response = await Client.DeleteAsync($"/tariffs/{tariff.TariffId}");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("message", out var message).Should().BeTrue();
            message.GetString().Should().NotBeNullOrEmpty();
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_DeleteTariff_Should_ReturnSuccessMessage_WhenTariffDeleted] Response: {jsonString}");
            throw;
        }
    }
}
