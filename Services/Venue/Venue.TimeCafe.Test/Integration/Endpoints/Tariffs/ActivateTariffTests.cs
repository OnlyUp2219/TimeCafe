namespace Venue.TimeCafe.Test.Integration.Endpoints.Tariffs;

public class ActivateTariffTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_ActivateTariff_Should_Return204_WhenTariffExists()
    {
        var tariff = await SeedTariffAsync(TestData.NewTariffs.NewTariff1Name, TestData.NewTariffs.NewTariff1Price, isActive: false);

        var response = await Client.PostAsync($"/venue/tariffs/{tariff.TariffId}/activate", null);
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Endpoint_ActivateTariff_Should_Return404_WhenTariffNotFound()
    {
        var response = await Client.PostAsync($"/venue/tariffs/{TestData.NonExistingIds.NonExistingTariffId}/activate", null);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Endpoint_ActivateTariff_Should_Return422_WhenTariffIdIsEmpty()
    {
        var response = await Client.PostAsync($"/venue/tariffs/{Guid.Empty}/activate", null);
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Endpoint_ActivateTariff_Should_ActuallyActivateTariff_WhenCalled()
    {
        var tariff = await SeedTariffAsync(TestData.NewTariffs.NewTariff2Name, TestData.NewTariffs.NewTariff2Price, isActive: false);

        var activateResponse = await Client.PostAsync($"/venue/tariffs/{tariff.TariffId}/activate", null);
        activateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var response = await Client.GetAsync($"/venue/tariffs/{tariff.TariffId}");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.GetProperty("isActive").GetBoolean().Should().BeTrue();
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_ActivateTariff_Should_ActuallyActivateTariff_WhenCalled] Response: {jsonString}");
            throw;
        }
    }
}





