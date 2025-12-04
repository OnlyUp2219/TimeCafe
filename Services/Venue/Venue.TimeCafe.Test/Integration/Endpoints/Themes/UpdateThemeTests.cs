namespace Venue.TimeCafe.Test.Integration.Endpoints.Themes;

public class UpdateThemeTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_UpdateTheme_Should_Return200_WhenThemeExists()
    {
        var theme = await SeedThemeAsync("Оригинальная тема");
        var dto = new
        {
            ThemeId = theme.ThemeId,
            Name = "Обновленная тема",
            Emoji = "🎭",
            Colors = "{\"primary\":\"#00FF00\"}"
        };

        var response = await Client.PutAsJsonAsync("/themes", dto);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("message", out var message).Should().BeTrue();
            message.GetString().Should().NotBeNullOrWhiteSpace();

            json.TryGetProperty("theme", out var updatedTheme).Should().BeTrue();
            updatedTheme.GetProperty("themeId").GetInt32().Should().Be(theme.ThemeId);
            updatedTheme.GetProperty("name").GetString().Should().Be("Обновленная тема");
            updatedTheme.GetProperty("emoji").GetString().Should().Be("🎭");
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_UpdateTheme_Should_Return200_WhenThemeExists] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_UpdateTheme_Should_Return404_WhenThemeNotFound()
    {
        var dto = new
        {
            ThemeId = 99999,
            Name = "Несуществующая тема",
            Emoji = "🚫",
            Colors = "{}"
        };

        var response = await Client.PutAsJsonAsync("/themes", dto);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("code", out var code).Should().BeTrue();
            code.GetString().Should().Be("ThemeNotFound");
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_UpdateTheme_Should_Return404_WhenThemeNotFound] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_UpdateTheme_Should_UpdateOnlyChangedFields()
    {
        var theme = await SeedThemeAsync("Исходная тема");
        var dto = new
        {
            ThemeId = theme.ThemeId,
            Name = "Новое имя",
            Emoji = "🎨",
            Colors = "{\"primary\":\"#FF0000\"}"
        };

        var response = await Client.PutAsJsonAsync("/themes", dto);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            var updatedTheme = json.GetProperty("theme");
            updatedTheme.GetProperty("name").GetString().Should().Be("Новое имя");
            updatedTheme.GetProperty("emoji").GetString().Should().Be("🎨");
            updatedTheme.GetProperty("colors").GetString().Should().Be("{\"primary\":\"#FF0000\"}");
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_UpdateTheme_Should_UpdateOnlyChangedFields] Response: {jsonString}");
            throw;
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Endpoint_UpdateTheme_Should_Return422_WhenNameIsInvalid(string invalidName)
    {
        var theme = await SeedThemeAsync("Исходная тема");
        var dto = new
        {
            ThemeId = theme.ThemeId,
            Name = invalidName,
            Emoji = "🎨",
            Colors = (string?)null
        };

        var response = await Client.PutAsJsonAsync("/themes", dto);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("code", out var code).Should().BeTrue();
            code.GetString().Should().Be("ValidationError");
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_UpdateTheme_Should_Return422_WhenNameIsInvalid] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_UpdateTheme_Should_PreserveOriginalDataIfNotAllFieldsProvided()
    {
        var originalTheme = await SeedThemeAsync("Оригинальная тема");
        var dto = new
        {
            ThemeId = originalTheme.ThemeId,
            Name = "Только имя изменилось",
            Emoji = "🆕",
            Colors = "{\"new\":\"colors\"}"
        };

        var response = await Client.PutAsJsonAsync("/themes", dto);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            var updatedTheme = json.GetProperty("theme");
            updatedTheme.GetProperty("themeId").GetInt32().Should().Be(originalTheme.ThemeId);
            updatedTheme.GetProperty("name").GetString().Should().Be("Только имя изменилось");
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_UpdateTheme_Should_PreserveOriginalDataIfNotAllFieldsProvided] Response: {jsonString}");
            throw;
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-999)]
    public async Task Endpoint_UpdateTheme_Should_Return422_WhenThemeIdIsInvalid(int invalidId)
    {
        var dto = new
        {
            ThemeId = invalidId,
            Name = "Какая-то тема",
            Emoji = "🎨",
            Colors = "{}"
        };

        var response = await Client.PutAsJsonAsync("/themes", dto);
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("code", out var code).Should().BeTrue();
            code.GetString().Should().Be("ValidationError");
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_UpdateTheme_Should_Return422_WhenThemeIdIsInvalid] Response: {jsonString}");
            throw;
        }
    }
}
