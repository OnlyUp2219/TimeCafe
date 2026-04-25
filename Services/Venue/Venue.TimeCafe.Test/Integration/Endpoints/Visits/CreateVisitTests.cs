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

        var response = await Client.PostAsJsonAsync("/venue/visits", payload);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var json = JsonDocument.Parse(jsonString).RootElement;


            json.GetProperty("userId").GetString().Should().Be(TestData.DefaultValues.DefaultUserId.ToString());
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_CreateVisit_Should_Return201_WhenValid] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_CreateVisit_Should_Return422_WhenUserIdIsEmpty()
    {
        await ClearDatabaseAndCacheAsync();
        var tariff = await SeedTariffAsync(TestData.DefaultValues.DefaultTariffName, TestData.DefaultValues.DefaultTariffPrice);
        var payload = new
        {
            userId = Guid.Empty,
            tariffId = tariff.TariffId
        };

        var response = await Client.PostAsJsonAsync("/venue/visits", payload);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_CreateVisit_Should_Return422_WhenUserIdIsEmpty] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_CreateVisit_Should_Return422_WhenTariffIdIsEmpty()
    {
        await ClearDatabaseAndCacheAsync();
        var payload = new
        {
            userId = TestData.DefaultValues.DefaultUserId,
            tariffId = Guid.Empty
        };

        var response = await Client.PostAsJsonAsync("/venue/visits", payload);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_CreateVisit_Should_Return422_WhenTariffIdIsEmpty] Response: {jsonString}");
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

        var response = await Client.PostAsJsonAsync("/venue/visits", payload);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var json = JsonDocument.Parse(jsonString).RootElement;
            var visit = json;
            json.GetProperty("visitId").GetGuid().Should().NotBeEmpty();
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_CreateVisit_Should_ReturnVisitWithId_WhenCreated] Response: {jsonString}");
            throw;
        }
    }
}













