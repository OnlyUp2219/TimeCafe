namespace Venue.TimeCafe.Test.Integration.Endpoints.Visits;

public class CreateVisitTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_CreateVisit_Should_Return201_WhenValid()
    {
        await ClearDatabaseAndCacheAsync();
        var tariff = await SeedTariffAsync("Тариф", 10m);
        var payload = new
        {
            userId = "user123",
            tariffId = tariff.TariffId
        };

        var response = await Client.PostAsJsonAsync("/visits", payload);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("message", out _).Should().BeTrue();
            json.TryGetProperty("visit", out var visit).Should().BeTrue();
            visit.GetProperty("userId").GetString().Should().Be("user123");
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_CreateVisit_Should_Return201_WhenValid] Response: {jsonString}");
            throw;
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Endpoint_CreateVisit_Should_Return422_WhenUserIdIsInvalid(string? invalidUserId)
    {
        await ClearDatabaseAndCacheAsync();
        var tariff = await SeedTariffAsync("Тариф", 10m);
        var payload = new
        {
            userId = invalidUserId,
            tariffId = tariff.TariffId
        };

        var response = await Client.PostAsJsonAsync("/visits", payload);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_CreateVisit_Should_Return422_WhenUserIdIsInvalid] Response: {jsonString}");
            throw;
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Endpoint_CreateVisit_Should_Return422_WhenTariffIdIsInvalid(int invalidTariffId)
    {
        await ClearDatabaseAndCacheAsync();
        var payload = new
        {
            userId = "user123",
            tariffId = invalidTariffId
        };

        var response = await Client.PostAsJsonAsync("/visits", payload);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_CreateVisit_Should_Return422_WhenTariffIdIsInvalid] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_CreateVisit_Should_ReturnVisitWithId_WhenCreated()
    {
        await ClearDatabaseAndCacheAsync();
        var tariff = await SeedTariffAsync("Тариф", 10m);
        var payload = new
        {
            userId = "user123",
            tariffId = tariff.TariffId
        };

        var response = await Client.PostAsJsonAsync("/visits", payload);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var json = JsonDocument.Parse(jsonString).RootElement;
            var visit = json.GetProperty("visit");
            visit.GetProperty("visitId").GetInt32().Should().BeGreaterThan(0);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_CreateVisit_Should_ReturnVisitWithId_WhenCreated] Response: {jsonString}");
            throw;
        }
    }
}
