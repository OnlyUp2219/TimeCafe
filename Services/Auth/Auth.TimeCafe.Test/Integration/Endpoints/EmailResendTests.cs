namespace Auth.TimeCafe.Test.Integration.Endpoints;

public class EmailResendTests : BaseEndpointTest
{
    private const string EndpointMock = "/auth/email/resend-mock";

    public EmailResendTests(IntegrationApiFactory factory) : base(factory)
    {
        SeedUserAsync("unconfirmed@example.com", "P@ssw0rd!", false).GetAwaiter().GetResult();
        SeedUserAsync("confirmed@example.com", "P@ssw0rd!", true).GetAwaiter().GetResult();
    }

    [Fact]
    public async Task Endpoint_Resend_Should_ReturnUserNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var dto = new { Email = "notexists@example.com" };

        // Act
        using var client = CreateClientWithDisabledRateLimiter();
        var response = await client.PostAsJsonAsync(EndpointMock, dto);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be((HttpStatusCode)401);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("code", out var code).Should().BeTrue();
            code.GetString()!.Should().Be("UserNotFound");
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_Resend_Should_ReturnUserNotFound] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_Resend_Should_ReturnEmailAlreadyConfirmed_WhenEmailConfirmed()
    {
        // Arrange
        var dto = new { Email = "confirmed@example.com" };

        // Act
        using var client = CreateClientWithDisabledRateLimiter();
        var response = await client.PostAsJsonAsync(EndpointMock, dto);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("code", out var code).Should().BeTrue();
            code.GetString()!.Should().Be("EmailAlreadyConfirmed");
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_Resend_Should_ReturnEmailAlreadyConfirmed] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_ResendMock_Should_ReturnCallbackUrl_WhenUserUnconfirmed()
    {
        // Arrange
        var dto = new { Email = "unconfirmed@example.com" };

        // Act
        using var client = CreateClientWithDisabledRateLimiter();
        var response = await client.PostAsJsonAsync(EndpointMock, dto);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("callbackUrl", out var url).Should().BeTrue();
            url.GetString()!.Should().Contain("confirm-email?userId=");
            url.GetString()!.Should().Contain("&token=");
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_ResendMock_Should_ReturnCallbackUrl] Response: {jsonString}");
            throw;
        }
    }

    [Theory]
    [InlineData("", "Email не может быть пустым")]
    [InlineData("invalid-email", "Неверный формат Email")]
    public async Task Endpoint_Resend_Should_ReturnValidationError_WhenInvalidCommand(string email, string expectedMessagePart)
    {
        // Arrange
        var dto = new { Email = email };

        // Act
        using var client = CreateClientWithDisabledRateLimiter();
        var response = await client.PostAsJsonAsync(EndpointMock, dto);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("code", out var code).Should().BeTrue();
            code.GetString()!.Should().Be("ValidationError");

            json.TryGetProperty("errors", out var errors).Should().BeTrue();
            errors.EnumerateArray()
                  .Any(e => e.TryGetProperty("message", out var msg) && msg.GetString()!.Contains(expectedMessagePart))
                  .Should().BeTrue();
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_Resend_Should_ReturnValidationError] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_Resend_Should_WorkWithRateLimiterOverrides()
    {
        // Arrange
        var dto = new { Email = "unconfirmed@example.com" };

        // Act
        using var client = CreateClientWithDisabledRateLimiter();
        var response1 = await client.PostAsJsonAsync(EndpointMock, dto);
        var response2 = await client.PostAsJsonAsync(EndpointMock, dto);

        // Assert
        try
        {
            response1.StatusCode.Should().Be(HttpStatusCode.OK);
            response2.StatusCode.Should().Be(HttpStatusCode.OK);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_Resend_Should_WorkWithRateLimiterOverrides] Response1: {await response1.Content.ReadAsStringAsync()}");
            Console.WriteLine($"Response2: {await response2.Content.ReadAsStringAsync()}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_ResendMock_Should_GenerateDifferentToken_EachCall()
    {
        // Arrange
        var dto = new { Email = "unconfirmed@example.com" };

        // Act
        using var client = CreateClientWithDisabledRateLimiter();
        var resp1 = await client.PostAsJsonAsync(EndpointMock, dto);
        var resp2 = await client.PostAsJsonAsync(EndpointMock, dto);

        var url1 = JsonDocument.Parse(await resp1.Content.ReadAsStringAsync()).RootElement.GetProperty("callbackUrl").GetString();
        var url2 = JsonDocument.Parse(await resp2.Content.ReadAsStringAsync()).RootElement.GetProperty("callbackUrl").GetString();

        // Assert
        try
        {
            url1.Should().NotBe(url2);
        }
        catch (Exception)
        {
            Console.WriteLine($"URL1: {url1}");
            Console.WriteLine($"URL2: {url2}");
            throw;
        }
    }
}