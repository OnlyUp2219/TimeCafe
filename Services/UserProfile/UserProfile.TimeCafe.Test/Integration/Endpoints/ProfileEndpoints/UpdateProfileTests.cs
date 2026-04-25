namespace UserProfile.TimeCafe.Test.Integration.Endpoints.ProfileEndpoints;

public class UpdateProfileTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_UpdateProfile_Should_Return204_WhenProfileExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        await SeedProfileAsync(userId, TestData.ExistingUsers.User2FirstName, TestData.ExistingUsers.User2LastName);
        var dto = new
        {
            firstName = TestData.ExistingUsers.User2FirstName,
            lastName = TestData.UpdateData.UpdatedLastName2,
            middleName = TestData.UpdateData.UpdatedMiddleName,
            photoUrl = (string?)null,
            birthDate = (DateOnly?)null,
            gender = (byte)Gender.Female
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/userprofile/profiles/{userId}", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify update
        var getResponse = await Client.GetAsync($"/userprofile/profiles/{userId}");
        var json = JsonDocument.Parse(await getResponse.Content.ReadAsStringAsync()).RootElement;
        json.GetProperty("lastName").GetString()!.Should().Be(TestData.UpdateData.UpdatedLastName2);
        json.GetProperty("middleName").GetString()!.Should().Be(TestData.UpdateData.UpdatedMiddleName);
    }

    [Fact]
    public async Task Endpoint_UpdateProfile_Should_Return404_WhenProfileNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dto = new
        {
            firstName = "Test",
            lastName = "User",
            middleName = (string?)null,
            photoUrl = (string?)null,
            birthDate = (DateOnly?)null,
            gender = (byte)Gender.NotSpecified
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/userprofile/profiles/{userId}", dto);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        if (!string.IsNullOrWhiteSpace(jsonString))
        {
            var json = JsonDocument.Parse(jsonString).RootElement;
            if (json.TryGetProperty("code", out var code))
                code.GetString()!.Should().Be("ProfileNotFound");
        }
    }

    [Fact]
    public async Task Endpoint_UpdateProfile_Should_Return422_WhenValidationFails()
    {
        // Arrange
        var dto = new
        {
            firstName = "",
            lastName = "",
            middleName = (string?)null,
            photoUrl = (string?)null,
            birthDate = (DateOnly?)null,
            gender = (byte)Gender.NotSpecified
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/userprofile/profiles/{Guid.NewGuid()}", dto);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        var json = JsonDocument.Parse(jsonString).RootElement;
        json.TryGetProperty("code", out var code).Should().BeTrue();
        code.GetString()!.Should().Be("ValidationError");
    }
}


