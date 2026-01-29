namespace UserProfile.TimeCafe.Test.Integration.Endpoints.ProfileEndpoints;

public class UpdateProfileTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_UpdateProfile_Should_Return200_WhenProfileExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        await SeedProfileAsync(userId, TestData.ExistingUsers.User2FirstName, TestData.ExistingUsers.User2LastName);
        var dto = new
        {
            userId = userId.ToString(),
            firstName = TestData.ExistingUsers.User2FirstName,
            lastName = TestData.UpdateData.UpdatedLastName2,
            middleName = TestData.UpdateData.UpdatedMiddleName,
            accessCardNumber = (string?)null,
            photoUrl = (string?)null,
            birthDate = (DateOnly?)null,
            gender = (byte)Gender.Female
        };

        // Act
        var response = await Client.PutAsJsonAsync("/profiles", dto);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("profile", out var profile).Should().BeTrue();
            profile.GetProperty("lastName").GetString()!.Should().Be(TestData.UpdateData.UpdatedLastName2);
            profile.GetProperty("middleName").GetString()!.Should().Be(TestData.UpdateData.UpdatedMiddleName);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_UpdateProfile_Should_Return200_WhenProfileExists] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_UpdateProfile_Should_Return404_WhenProfileNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dto = new
        {
            userId = userId.ToString(),
            firstName = "Test",
            lastName = "User",
            middleName = (string?)null,
            accessCardNumber = (string?)null,
            photoUrl = (string?)null,
            birthDate = (DateOnly?)null,
            gender = (byte)Gender.NotSpecified
        };

        // Act
        var response = await Client.PutAsJsonAsync("/profiles", dto);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("code", out var code).Should().BeTrue();
            code.GetString()!.Should().Be("ProfileNotFound");
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_UpdateProfile_Should_Return404_WhenProfileNotFound] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_UpdateProfile_Should_Return422_WhenValidationFails()
    {
        // Arrange
        var dto = new
        {
            userId = Guid.NewGuid().ToString(),
            firstName = "",
            lastName = "",
            middleName = (string?)null,
            accessCardNumber = (string?)null,
            photoUrl = (string?)null,
            birthDate = (DateOnly?)null,
            gender = (byte)Gender.NotSpecified
        };

        // Act
        var response = await Client.PutAsJsonAsync("/profiles", dto);
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
            Console.WriteLine($"[Endpoint_UpdateProfile_Should_Return422_WhenValidationFails] Response: {jsonString}");
            throw;
        }
    }
}
