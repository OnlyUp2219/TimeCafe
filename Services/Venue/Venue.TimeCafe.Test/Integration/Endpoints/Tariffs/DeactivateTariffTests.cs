namespace Venue.TimeCafe.Test.Integration.Endpoints.Tariffs;

public class DeactivateTariffTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_DeactivateTariff_Should_Return200_WhenTariffExists()
    {
        await ClearDatabaseAndCacheAsync();
        var tariff = await SeedTariffAsync("Активный тариф", 10m, isActive: true);

        var response = await Client.PostAsync($"/tariffs/{tariff.TariffId}/deactivate", null);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("message", out _).Should().BeTrue();
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_DeactivateTariff_Should_Return200_WhenTariffExists] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_DeactivateTariff_Should_Return404_WhenTariffNotFound()
    {
        await ClearDatabaseAndCacheAsync();

        var response = await Client.PostAsync("/tariffs/9999/deactivate", null);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_DeactivateTariff_Should_Return404_WhenTariffNotFound] Response: {jsonString}");
            throw;
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task Endpoint_DeactivateTariff_Should_Return422_WhenTariffIdIsInvalid(int invalidId)
    {
        await ClearDatabaseAndCacheAsync();

        var response = await Client.PostAsync($"/tariffs/{invalidId}/deactivate", null);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_DeactivateTariff_Should_Return422_WhenTariffIdIsInvalid({invalidId})] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_DeactivateTariff_Should_ActuallyDeactivateTariff_WhenCalled()
    {
        await ClearDatabaseAndCacheAsync();
        var tariff = await SeedTariffAsync("Тариф для деактивации", 10m, isActive: true);

        await Client.PostAsync($"/tariffs/{tariff.TariffId}/deactivate", null);

        var response = await Client.GetAsync($"/tariffs/{tariff.TariffId}");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.GetProperty("tariff").GetProperty("isActive").GetBoolean().Should().BeFalse();
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_DeactivateTariff_Should_ActuallyDeactivateTariff_WhenCalled] Response: {jsonString}");
            throw;
        }
    }
}
