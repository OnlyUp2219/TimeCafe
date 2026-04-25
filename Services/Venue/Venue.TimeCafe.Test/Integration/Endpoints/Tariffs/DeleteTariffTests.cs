namespace Venue.TimeCafe.Test.Integration.Endpoints.Tariffs;

public class DeleteTariffTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_DeleteTariff_Should_Return204_WhenTariffExists()
    {
        var tariff = await SeedTariffAsync(TestData.NewTariffs.NewTariff1Name, TestData.NewTariffs.NewTariff1Price);

        var response = await Client.DeleteAsync($"/venue/tariffs/{tariff.TariffId}");
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Endpoint_DeleteTariff_Should_Return404_WhenTariffNotFound()
    {
        var response = await Client.DeleteAsync($"/venue/tariffs/{TestData.NonExistingIds.NonExistingTariffId}");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            var json = JsonDocument.Parse(jsonString).RootElement;
            if (json.TryGetProperty("code", out var code))
                code.GetString().Should().Be("TariffNotFound");
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_DeleteTariff_Should_Return404_WhenTariffNotFound] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_DeleteTariff_Should_ActuallyRemoveTariffFromDatabase()
    {
        var tariff = await SeedTariffAsync(TestData.NewTariffs.NewTariff1Name, TestData.NewTariffs.NewTariff1Price);

        var deleteResponse = await Client.DeleteAsync($"/venue/tariffs/{tariff.TariffId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var secondDeleteResponse = await Client.DeleteAsync($"/venue/tariffs/{tariff.TariffId}");
        var jsonString = await secondDeleteResponse.Content.ReadAsStringAsync();
        try
        {
            secondDeleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
            var json = JsonDocument.Parse(jsonString).RootElement;
            if (json.TryGetProperty("code", out var code))
                code.GetString().Should().Be("TariffNotFound");
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_DeleteTariff_Should_ActuallyRemoveTariffFromDatabase] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_DeleteTariff_Should_Return422_WhenTariffIdIsEmpty()
    {
        var response = await Client.DeleteAsync($"/venue/tariffs/{Guid.Empty}");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("code", out var code).Should().BeTrue();
            code.GetString().Should().Be("ValidationError");
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_DeleteTariff_Should_Return422_WhenTariffIdIsEmpty] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_DeleteTariff_Should_Return405_WhenTariffIdIsEmpty()
    {
        var response = await Client.DeleteAsync("/venue/tariffs/");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_DeleteTariff_Should_Return405_WhenTariffIdIsEmpty] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_DeleteTariff_Should_NotAffectOtherTariffs_WhenOneIsDeleted()
    {
        var tariff1 = await SeedTariffAsync(TestData.NewTariffs.NewTariff1Name, TestData.NewTariffs.NewTariff1Price);
        var tariff2 = await SeedTariffAsync(TestData.NewTariffs.NewTariff2Name, TestData.NewTariffs.NewTariff2Price);

        await Client.DeleteAsync($"/venue/tariffs/{tariff1.TariffId}");

        var response = await Client.GetAsync($"/venue/tariffs/{tariff2.TariffId}");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.GetProperty("tariffId").GetGuid().Should().Be(tariff2.TariffId);
            json.GetProperty("name").GetString().Should().Be(TestData.NewTariffs.NewTariff2Name);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_DeleteTariff_Should_NotAffectOtherTariffs_WhenOneIsDeleted] Response: {jsonString}");
            throw;
        }
    }
}







