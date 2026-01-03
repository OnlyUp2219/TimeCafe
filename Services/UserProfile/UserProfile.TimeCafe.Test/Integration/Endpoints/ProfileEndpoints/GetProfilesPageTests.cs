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
        var response = await Client.GetAsync($"/profiles/page?pageNumber={TestData.PaginationData.FirstPage}&pageSize={TestData.PaginationData.DefaultPageSize}");
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("profiles", out var profiles).Should().BeTrue();
            profiles.GetArrayLength().Should().Be(TestData.PaginationData.DefaultPageSize);
            json.TryGetProperty("pageNumber", out var pageNum).Should().BeTrue();
            pageNum.GetInt32().Should().Be(TestData.PaginationData.FirstPage);
            json.TryGetProperty("pageSize", out var pageSize).Should().BeTrue();
            pageSize.GetInt32().Should().Be(TestData.PaginationData.DefaultPageSize);
            json.TryGetProperty("totalCount", out var totalCount).Should().BeTrue();
            totalCount.GetInt32().Should().BeGreaterThanOrEqualTo(3);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetProfilesPage_Should_Return200_WithPaginatedData] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_GetProfilesPage_Should_Return422_WhenInvalidPageNumber()
    {
        // Act
        var response = await Client.GetAsync($"/profiles/page?pageNumber={TestData.PaginationData.InvalidPage}&pageSize={TestData.PaginationData.LargePageSize}");
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("code", out var code).Should().BeTrue();
            code.GetString()!.Should().Be("ValidationError");
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetProfilesPage_Should_Return422_WhenInvalidPageNumber] Response: {jsonString}");
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
        var response = await Client.GetAsync($"/profiles/page?pageNumber={TestData.PaginationData.SecondPage}&pageSize={TestData.PaginationData.DefaultPageSize}");
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("pageNumber", out var pageNum).Should().BeTrue();
            pageNum.GetInt32().Should().Be(TestData.PaginationData.SecondPage);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetProfilesPage_Should_Return200_SecondPage] Response: {jsonString}");
            throw;
        }
    }
}
