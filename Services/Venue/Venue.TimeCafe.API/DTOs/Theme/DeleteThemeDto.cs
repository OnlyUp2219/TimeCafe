namespace Venue.TimeCafe.API.DTOs.Theme;

public record DeleteThemeDto(string ThemeId);

public class DeleteThemeDtoExample : IExamplesProvider<DeleteThemeDto>
{
    public DeleteThemeDto GetExamples() =>
        new(ThemeId: "a1111111-1111-1111-1111-111111111111");
}
