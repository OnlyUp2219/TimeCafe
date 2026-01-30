namespace Venue.TimeCafe.Test.Integration.Endpoints.Themes;

public class GetAllThemesTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_GetAllThemes_Should_Return200_WhenThemesExist()
    {
        await ClearDatabaseAndCacheAsync();
        var guid = Guid.NewGuid();
        await SeedThemeAsync($"Тема 1 {guid}");
        await SeedThemeAsync($"Тема 2 {guid}");
        await SeedThemeAsync($"Тема 3 {guid}");

        var response = await Client.GetAsync("/venue/themes");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("themes", out var themes).Should().BeTrue();
            themes.ValueKind.Should().Be(JsonValueKind.Array);
            themes.GetArrayLength().Should().BeGreaterThanOrEqualTo(3);

            var firstTheme = themes[0];
            firstTheme.TryGetProperty("themeId", out _).Should().BeTrue();
            firstTheme.TryGetProperty("name", out var name).Should().BeTrue();
            name.GetString().Should().NotBeNullOrWhiteSpace();
            firstTheme.TryGetProperty("emoji", out _).Should().BeTrue();
            firstTheme.TryGetProperty("colors", out _).Should().BeTrue();
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetAllThemes_Should_Return200_WhenThemesExist] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_GetAllThemes_Should_Return200_WhenNoThemesExist()
    {
        await ClearDatabaseAndCacheAsync();
        var response = await Client.GetAsync("/venue/themes");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("themes", out var themes).Should().BeTrue();
            themes.ValueKind.Should().Be(JsonValueKind.Array);
            themes.GetArrayLength().Should().Be(0);
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetAllThemes_Should_Return200_WhenNoThemesExist] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_GetAllThemes_Should_ReturnAllProperties_WhenThemesExist()
    {
        await ClearDatabaseAndCacheAsync();
        var theme = await SeedThemeAsync("Специальная тема");

        var response = await Client.GetAsync("/venue/themes");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            var themes = json.GetProperty("themes");
            var foundTheme = themes.EnumerateArray().FirstOrDefault(t => t.GetProperty("name").GetString() == "Специальная тема");

            foundTheme.ValueKind.Should().NotBe(JsonValueKind.Undefined);
            foundTheme.GetProperty("themeId").GetGuid().Should().Be(theme.ThemeId);
            foundTheme.GetProperty("name").GetString().Should().Be("Специальная тема");
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_GetAllThemes_Should_ReturnAllProperties_WhenThemesExist] Response: {jsonString}");
            throw;
        }
    }
}
