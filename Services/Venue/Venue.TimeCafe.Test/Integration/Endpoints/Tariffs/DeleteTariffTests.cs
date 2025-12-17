namespace Venue.TimeCafe.Test.Integration.Endpoints.Tariffs;

public class DeleteTariffTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_DeleteTariff_Should_Return200_WhenTariffExists()
    {
        await ClearDatabaseAndCacheAsync();
        var name = TestData.NewTariffs.NewTariff1Name;
        var tariff = await SeedTariffAsync(name, TestData.NewTariffs.NewTariff1Price);

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

        var response = await Client.DeleteAsync($"/tariffs/{TestData.NonExistingIds.NonExistingTariffIdString}");
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
        var delName = TestData.NewTariffs.NewTariff2Name;
        var tariff = await SeedTariffAsync(delName, TestData.NewTariffs.NewTariff2Price);

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
    [InlineData("not-a-guid")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public async Task Endpoint_DeleteTariff_Should_Return422_WhenTariffIdIsInvalid(string invalidId)
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
        var tariff1 = await SeedTariffAsync(TestData.NewTariffs.NewTariff1Name, TestData.NewTariffs.NewTariff1Price);
        var tariff2 = await SeedTariffAsync(TestData.NewTariffs.NewTariff2Name, TestData.NewTariffs.NewTariff2Price);

        await Client.DeleteAsync($"/tariffs/{tariff1.TariffId}");

        var response = await Client.GetAsync($"/tariffs/{tariff2.TariffId}");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            var tariffJson = json;
            if (json.TryGetProperty("tariff", out var t)) tariffJson = t;
            string nameValue = null;
            if (tariffJson.TryGetProperty("name", out var n)) nameValue = n.GetString();
            if (tariffJson.TryGetProperty("tariffName", out var tn)) nameValue = tn.GetString();
            nameValue.Should().Be(TestData.NewTariffs.NewTariff2Name);
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
        var msgName = TestData.NewTariffs.NewTariff1Name;
        var tariff = await SeedTariffAsync(msgName, TestData.NewTariffs.NewTariff1Price);

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
