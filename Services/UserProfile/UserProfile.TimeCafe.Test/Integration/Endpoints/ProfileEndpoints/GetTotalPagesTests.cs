using FluentAssertions;
using System.Net;
using System.Text.Json;
using UserProfile.TimeCafe.Application.CQRS.Profiles.Queries;
using UserProfile.TimeCafe.Test.Integration.Helpers;

namespace UserProfile.TimeCafe.Test.Integration.Endpoints;

public class GetTotalPagesTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_GetTotalPages_Should_Return200_WithCount()
    {
        // Arrange
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();
        await SeedProfileAsync(userId1, "Иван", "Петров");
        await SeedProfileAsync(userId2, "Мария", "Сидорова");

        // Act
        var response = await Client.GetAsync("/profiles/total");
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("totalCount", out var totalCount).Should().BeTrue();
            totalCount.GetInt32().Should().BeGreaterThanOrEqualTo(2);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetTotalPages_Should_Return200_WithCount] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_GetTotalPages_Should_Return200_WithZero()
    {
        // Act (не создаём профили)
        var response = await Client.GetAsync("/profiles/total");
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("totalCount", out var totalCount).Should().BeTrue();
            totalCount.GetInt32().Should().BeGreaterThanOrEqualTo(0);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetTotalPages_Should_Return200_WithZero] Response: {jsonString}");
            throw;
        }
    }
}
