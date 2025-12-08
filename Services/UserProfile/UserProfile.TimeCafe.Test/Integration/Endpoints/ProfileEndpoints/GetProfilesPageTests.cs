using FluentAssertions;
using System.Net;
using System.Text.Json;
using UserProfile.TimeCafe.Application.CQRS.Profiles.Queries;
using UserProfile.TimeCafe.Test.Integration.Helpers;

namespace UserProfile.TimeCafe.Test.Integration.Endpoints;

public class GetProfilesPageTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_GetProfilesPage_Should_Return200_WithPaginatedData()
    {
        // Arrange
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();
        var userId3 = Guid.NewGuid();
        await SeedProfileAsync(userId1, "Иван", "Петров");
        await SeedProfileAsync(userId2, "Мария", "Сидорова");
        await SeedProfileAsync(userId3, "Петр", "Иванов");

        // Act
        var response = await Client.GetAsync("/profiles/page?pageNumber=1&pageSize=2");
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("profiles", out var profiles).Should().BeTrue();
            profiles.GetArrayLength().Should().Be(2);
            json.TryGetProperty("pageNumber", out var pageNum).Should().BeTrue();
            pageNum.GetInt32().Should().Be(1);
            json.TryGetProperty("pageSize", out var pageSize).Should().BeTrue();
            pageSize.GetInt32().Should().Be(2);
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
        var response = await Client.GetAsync("/profiles/page?pageNumber=0&pageSize=10");
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
        var response = await Client.GetAsync("/profiles/page?pageNumber=2&pageSize=2");
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("pageNumber", out var pageNum).Should().BeTrue();
            pageNum.GetInt32().Should().Be(2);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetProfilesPage_Should_Return200_SecondPage] Response: {jsonString}");
            throw;
        }
    }
}
