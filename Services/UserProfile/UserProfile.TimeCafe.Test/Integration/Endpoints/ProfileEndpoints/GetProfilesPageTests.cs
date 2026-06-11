namespace UserProfile.TimeCafe.Test.Integration.Endpoints.ProfileEndpoints;

public class GetProfilesPageTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_GetProfilesPage_Should_Return200_WithPaginatedData()
    {
        // Arrange
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();
        var userId3 = Guid.NewGuid();
        await SeedProfileAsync(userId1, TestData.ExistingUsers.User1FirstName, TestData.ExistingUsers.User1LastName);
        await SeedProfileAsync(userId2, TestData.ExistingUsers.User2FirstName, TestData.ExistingUsers.User2LastName);
        await SeedProfileAsync(userId3, TestData.ExistingUsers.User3FirstName, TestData.ExistingUsers.User3LastName);

        // Act
        var response = await Client.GetAsync($"/userprofile/profiles/page?page={TestData.PaginationData.FirstPage}&pageSize={TestData.PaginationData.DefaultPageSize}");
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            var items = json.GetProperty("items");
            items.ValueKind.Should().Be(JsonValueKind.Array);
            var itemsList = items.EnumerateArray().ToList();
            itemsList.Should().HaveCount(TestData.PaginationData.DefaultPageSize);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetProfilesPage_Should_Return200_WithPaginatedData] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_GetProfilesPage_Should_Return200_WhenInvalidPageNumber()
    {
        // Act
        var response = await Client.GetAsync($"/userprofile/profiles/page?page={TestData.PaginationData.InvalidPage}&pageSize={TestData.PaginationData.LargePageSize}");
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            var items = json.GetProperty("items");
            items.ValueKind.Should().Be(JsonValueKind.Array);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetProfilesPage_Should_Return200_WhenInvalidPageNumber] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_GetProfilesPage_Should_Return200_SecondPage()
    {
        // Arrange
        for (int i = 0; i < 5; i++)
        {
            await SeedProfileAsync(Guid.NewGuid(), $"FirstName{i}", $"LastName{i}");
        }

        // Act
        var response = await Client.GetAsync($"/userprofile/profiles/page?page={TestData.PaginationData.SecondPage}&pageSize={TestData.PaginationData.DefaultPageSize}");
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            var items = json.GetProperty("items");
            items.ValueKind.Should().Be(JsonValueKind.Array);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetProfilesPage_Should_Return200_SecondPage] Response: {jsonString}");
            throw;
        }
    }
}








