namespace Venue.TimeCafe.Test.Integration.Endpoints.Themes;

public class GetThemeByIdTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_GetThemeById_Should_Return200_WhenThemeExists()
    {
        var theme = await SeedThemeAsync("Тестовая тема");

        var response = await Client.GetAsync($"/venue/themes/{theme.ThemeId}");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("theme", out var returnedTheme).Should().BeTrue();
            returnedTheme.GetProperty("themeId").GetGuid().Should().Be(theme.ThemeId);
            returnedTheme.GetProperty("name").GetString().Should().Be("Тестовая тема");
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetThemeById_Should_Return200_WhenThemeExists] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_GetThemeById_Should_Return404_WhenThemeNotFound()
    {
        var response = await Client.GetAsync($"/venue/themes/{TestData.NonExistingIds.NonExistingThemeId}");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("code", out var code).Should().BeTrue();
            code.GetString().Should().Be("ThemeNotFound");
            json.TryGetProperty("message", out var message).Should().BeTrue();
            message.GetString().Should().NotBeNullOrWhiteSpace();
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetThemeById_Should_Return404_WhenThemeNotFound] Response: {jsonString}");
            throw;
        }
    }

    [Theory]
    [InlineData("invalid-guid")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public async Task Endpoint_GetThemeById_Should_Return422_WhenThemeIdIsInvalid(string invalidId)
    {
        var response = await Client.GetAsync($"/venue/themes/{invalidId}");
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
            Console.WriteLine($"[Endpoint_GetThemeById_Should_Return422_WhenThemeIdIsInvalid] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_GetThemeById_Should_Return200_WhenThemeIdIsEmpty()
    {
        var response = await Client.GetAsync($"/venue/themes/");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("themes", out var themes).Should().BeTrue();
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetThemeById_Should_Return200_WhenThemeIdIsEmpty] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_GetThemeById_Should_ReturnAllProperties_WhenThemeExists()
    {
        await ClearDatabaseAndCacheAsync();
        var theme = await SeedThemeAsync("Полная тема");

        var response = await Client.GetAsync($"/venue/themes/{theme.ThemeId}");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            var returnedTheme = json.GetProperty("theme");

            returnedTheme.GetProperty("themeId").GetGuid().Should().Be(theme.ThemeId);
            returnedTheme.GetProperty("name").GetString().Should().Be("Полная тема");
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetThemeById_Should_ReturnAllProperties_WhenThemeExists] Response: {jsonString}");
            throw;
        }
    }
}
