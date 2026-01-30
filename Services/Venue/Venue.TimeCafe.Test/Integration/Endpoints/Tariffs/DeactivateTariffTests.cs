namespace Venue.TimeCafe.Test.Integration.Endpoints.Tariffs;

public class DeactivateTariffTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_DeactivateTariff_Should_Return200_WhenTariffExists()
    {
        await ClearDatabaseAndCacheAsync();
        var name = TestData.NewTariffs.NewTariff1Name;
        var tariff = await SeedTariffAsync(name, TestData.NewTariffs.NewTariff1Price, isActive: true);

        var response = await Client.PostAsync($"/venue/tariffs/{tariff.TariffId}/deactivate", null);
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

        var response = await Client.PostAsync($"/venue/tariffs/{TestData.NonExistingIds.NonExistingTariffIdString}/deactivate", null);
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
    [InlineData("not-a-guid")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public async Task Endpoint_DeactivateTariff_Should_Return422_WhenTariffIdIsInvalid(string invalidId)
    {
        await ClearDatabaseAndCacheAsync();

        var response = await Client.PostAsync($"/venue/tariffs/{invalidId}/deactivate", null);
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
        var name2 = TestData.NewTariffs.NewTariff2Name;
        var tariff = await SeedTariffAsync(name2, TestData.NewTariffs.NewTariff2Price, isActive: true);

        await Client.PostAsync($"/venue/tariffs/{tariff.TariffId}/deactivate", null);

        var response = await Client.GetAsync($"/venue/tariffs/{tariff.TariffId}");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            var tariffJson = json;
            if (json.TryGetProperty("tariff", out var t)) tariffJson = t;
            bool isActive = true;
            if (tariffJson.TryGetProperty("isActive", out var a)) isActive = a.GetBoolean();
            else if (tariffJson.TryGetProperty("tariffIsActive", out var ai)) isActive = ai.GetBoolean();
            isActive.Should().BeFalse();
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_DeactivateTariff_Should_ActuallyDeactivateTariff_WhenCalled] Response: {jsonString}");
            throw;
        }
    }
}
