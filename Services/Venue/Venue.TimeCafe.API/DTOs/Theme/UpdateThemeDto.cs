namespace Venue.TimeCafe.API.DTOs.Theme;

public record UpdateThemeDto(int ThemeId, string Name, string? Emoji, string? Colors);

public class UpdateThemeDtoExample : IExamplesProvider<UpdateThemeDto>
{
    public UpdateThemeDto GetExamples() =>
        new(ThemeId: 1, Name: "VIP зона", Emoji: "👑", Colors: "#FFD700");
}
