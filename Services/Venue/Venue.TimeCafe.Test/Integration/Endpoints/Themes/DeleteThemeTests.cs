namespace Venue.TimeCafe.Test.Integration.Endpoints.Themes;

public class DeleteThemeTests(IntegrationApiFactory factory) : BaseEndpointTest(factory)
{
    [Fact]
    public async Task Endpoint_DeleteTheme_Should_Return204_WhenThemeExists()
    {
        var theme = await SeedThemeAsync("Тема для удаления");

        var response = await Client.DeleteAsync($"/venue/themes/{theme.ThemeId}");
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Endpoint_DeleteTheme_Should_Return404_WhenThemeNotFound()
    {
        var response = await Client.DeleteAsync($"/venue/themes/{TestData.NonExistingIds.NonExistingThemeId}");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            var json = JsonDocument.Parse(jsonString).RootElement;
            if (json.TryGetProperty("code", out var code))
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

        var deleteResponse = await Client.DeleteAsync($"/venue/themes/{theme.ThemeId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var secondDeleteResponse = await Client.DeleteAsync($"/venue/themes/{theme.ThemeId}");
        var jsonString = await secondDeleteResponse.Content.ReadAsStringAsync();
        try
        {
            secondDeleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
            var json = JsonDocument.Parse(jsonString).RootElement;
            if (json.TryGetProperty("code", out var code))
                code.GetString().Should().Be("ThemeNotFound");
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_DeleteTheme_Should_ActuallyRemoveThemeFromDatabase] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_DeleteTheme_Should_Return422_WhenThemeIdIsEmpty()
    {
        var response = await Client.DeleteAsync($"/venue/themes/{Guid.Empty}");
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
            Console.WriteLine($"[Endpoint_DeleteTheme_Should_Return422_WhenThemeIdIsEmpty] Response: {jsonString}");
            throw;
        }
    }

    [Fact]
    public async Task Endpoint_DeleteTheme_Should_NotAffectOtherThemes()
    {
        var theme1 = await SeedThemeAsync("Первая тема");
        var theme2 = await SeedThemeAsync("Вторая тема");

        await Client.DeleteAsync($"/venue/themes/{theme1.ThemeId}");

        var response = await Client.GetAsync($"/venue/themes/{theme2.ThemeId}");
        var jsonString = await response.Content.ReadAsStringAsync();
        try
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var json = JsonDocument.Parse(jsonString).RootElement;

            json.GetProperty("themeId").GetGuid().Should().Be(theme2.ThemeId);
            json.GetProperty("name").GetString().Should().Be("Вторая тема");
        }
        catch (Exception)
        {
            Console.WriteLine($"[Endpoint_DeleteTheme_Should_NotAffectOtherThemes] Response: {jsonString}");
            throw;
        }
    }
}







