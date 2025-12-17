namespace Venue.TimeCafe.Test.Integration.Endpoints.Visits;

public class CreateVisitTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_CreateVisit_Should_Return201_WhenValid()
    {
        await ClearDatabaseAndCacheAsync();
        var tariff = await SeedTariffAsync(TestData.DefaultValues.DefaultTariffName, TestData.DefaultValues.DefaultTariffPrice);
        var payload = new
        {
            userId = TestData.DefaultValues.DefaultUserId.ToString(),
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
            visit.GetProperty("userId").GetString().Should().Be(TestData.DefaultValues.DefaultUserId.ToString());
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
        var tariff = await SeedTariffAsync(TestData.DefaultValues.DefaultTariffName, TestData.DefaultValues.DefaultTariffPrice);
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
    [InlineData("")]
    [InlineData("not-a-guid")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public async Task Endpoint_CreateVisit_Should_Return422_WhenTariffIdIsInvalid(string invalidTariffId)
    {
        await ClearDatabaseAndCacheAsync();
        var payload = new
        {
            userId = TestData.DefaultValues.DefaultUserId.ToString(),
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
        var tariff = await SeedTariffAsync(TestData.DefaultValues.DefaultTariffName, TestData.DefaultValues.DefaultTariffPrice);
        var payload = new
        {
            userId = TestData.DefaultValues.DefaultUserId.ToString(),
            tariffId = tariff.TariffId
        };

        var response = await Client.PostAsJsonAsync("/visits", payload);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var json = JsonDocument.Parse(jsonString).RootElement;
            var visit = json.GetProperty("visit");
            visit.GetProperty("visitId").GetGuid().Should().NotBeEmpty();
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_CreateVisit_Should_ReturnVisitWithId_WhenCreated] Response: {jsonString}");
            throw;
        }
    }
}
