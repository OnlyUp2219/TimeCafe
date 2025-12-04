namespace Venue.TimeCafe.Test.Integration.Endpoints.Themes;

[Collection("ThemesSequential")]
public class CreateThemeTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_CreateTheme_Should_Return201_WhenValid()
    {
        // Arrange
        var dto = new { Name = "Новая тема", Emoji = "🎮", Colors = "{\"primary\":\"#FF0000\"}" };

        // Act
        var response = await Client.PostAsJsonAsync("/themes", dto);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("message", out var message).Should().BeTrue();
            message.GetString().Should().NotBeNullOrWhiteSpace();

            json.TryGetProperty("theme", out var theme).Should().BeTrue();
            theme.ValueKind.Should().Be(JsonValueKind.Object);
            theme.GetProperty("name").GetString().Should().Be("Новая тема");
            theme.GetProperty("emoji").GetString().Should().Be("🎮");
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_CreateTheme_Should_Return201_WhenValid] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_CreateTheme_Should_Return422_WhenNameIsEmpty()
    {
        // Arrange
        var dto = new { Name = "", Emoji = "🎨", Colors = (string?)null };

        // Act
        var response = await Client.PostAsJsonAsync("/themes", dto);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("code", out var code).Should().BeTrue();
            code.GetString().Should().Be("ValidationError");
            json.TryGetProperty("errors", out var errors).Should().BeTrue();
            errors.ValueKind.Should().Be(JsonValueKind.Array);
            errors.GetArrayLength().Should().BeGreaterThan(0);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_CreateTheme_Should_Return422_WhenNameIsEmpty] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_CreateTheme_Should_Return422_WhenNameExceedsMaxLength()
    {
        // Arrange
        var longName = new string('A', 101);
        var dto = new { Name = longName, Emoji = "🎨", Colors = (string?)null };

        // Act
        var response = await Client.PostAsJsonAsync("/themes", dto);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("code", out var code).Should().BeTrue();
            code.GetString().Should().Be("ValidationError");
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_CreateTheme_Should_Return422_WhenNameExceedsMaxLength] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_CreateTheme_Should_CreateWithNullEmoji_WhenEmojiNotProvided()
    {
        // Arrange
        var dto = new { Name = "Тема без эмодзи", Emoji = (string?)null, Colors = "{}" };

        // Act
        var response = await Client.PostAsJsonAsync("/themes", dto);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("theme", out var theme).Should().BeTrue();
            theme.GetProperty("name").GetString().Should().Be("Тема без эмодзи");
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_CreateTheme_Should_CreateWithNullEmoji_WhenEmojiNotProvided] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_CreateTheme_Should_CreateWithMaxLengthName()
    {
        // Arrange
        var maxLengthName = new string('А', 100);
        var dto = new { Name = maxLengthName, Emoji = "✓", Colors = "{}" };

        // Act
        var response = await Client.PostAsJsonAsync("/themes", dto);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("theme", out var theme).Should().BeTrue();
            theme.GetProperty("name").GetString().Should().HaveLength(100);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_CreateTheme_Should_CreateWithMaxLengthName] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_CreateTheme_Should_ReturnThemeWithId_WhenCreatedSuccessfully()
    {
        // Arrange
        var dto = new { Name = "Тема с проверкой ID", Emoji = "🆔", Colors = "{}" };

        // Act
        var response = await Client.PostAsJsonAsync("/themes", dto);
        var jsonString = await response.Content.ReadAsStringAsync();

        // Assert
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var json = JsonDocument.Parse(jsonString).RootElement;
            var theme = json.GetProperty("theme");
            theme.TryGetProperty("themeId", out var themeId).Should().BeTrue();
            themeId.GetInt32().Should().BeGreaterThan(0);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_CreateTheme_Should_ReturnThemeWithId_WhenCreatedSuccessfully] Response: {jsonString}");
            throw;
        }
    }
}
