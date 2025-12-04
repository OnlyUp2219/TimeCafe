namespace Venue.TimeCafe.Test.Integration.Endpoints.Themes;

public class DeleteThemeTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_DeleteTheme_Should_Return200_WhenThemeExists()
    {
        var theme = await SeedThemeAsync("Тема для удаления");

        var response = await Client.DeleteAsync($"/themes/{theme.ThemeId}");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("message", out var message).Should().BeTrue();
            message.GetString().Should().NotBeNullOrWhiteSpace();
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_DeleteTheme_Should_Return200_WhenThemeExists] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_DeleteTheme_Should_Return404_WhenThemeNotFound()
    {
        var response = await Client.DeleteAsync("/themes/99999");
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
            Console.WriteLine($"[Endpoint_DeleteTheme_Should_Return404_WhenThemeNotFound] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_DeleteTheme_Should_ActuallyRemoveThemeFromDatabase()
    {
        var theme = await SeedThemeAsync("Проверка удаления");

        var deleteResponse = await Client.DeleteAsync($"/themes/{theme.ThemeId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var secondDeleteResponse = await Client.DeleteAsync($"/themes/{theme.ThemeId}");
        var jsonString = await secondDeleteResponse.Content.ReadAsStringAsync();
        try
        {
            secondDeleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("code", out var code).Should().BeTrue();
            code.GetString().Should().Be("ThemeNotFound");
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_DeleteTheme_Should_ActuallyRemoveThemeFromDatabase] Response: {jsonString}");
            throw;
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task Endpoint_DeleteTheme_Should_Return422_WhenThemeIdIsInvalid(int invalidId)
    {
        var response = await Client.DeleteAsync($"/themes/{invalidId}");
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
            Console.WriteLine($"[Endpoint_DeleteTheme_Should_Return422_WhenThemeIdIsInvalid] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_DeleteTheme_Should_NotAffectOtherThemes()
    {
        var theme1 = await SeedThemeAsync("Первая тема");
        var theme2 = await SeedThemeAsync("Вторая тема");

        await Client.DeleteAsync($"/themes/{theme1.ThemeId}");

        var response = await Client.GetAsync($"/themes/{theme2.ThemeId}");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("theme", out var theme).Should().BeTrue();
            theme.GetProperty("themeId").GetInt32().Should().Be(theme2.ThemeId);
            theme.GetProperty("name").GetString().Should().Be("Вторая тема");
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_DeleteTheme_Should_NotAffectOtherThemes] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_DeleteTheme_Should_ReturnSuccessMessage()
    {
        var theme = await SeedThemeAsync("Тема для проверки сообщения");

        var response = await Client.DeleteAsync($"/themes/{theme.ThemeId}");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;
            json.TryGetProperty("message", out var message).Should().BeTrue();
            message.GetString().Should().NotBeNullOrWhiteSpace();
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_DeleteTheme_Should_ReturnSuccessMessage] Response: {jsonString}");
            throw;
        }
    }
}
