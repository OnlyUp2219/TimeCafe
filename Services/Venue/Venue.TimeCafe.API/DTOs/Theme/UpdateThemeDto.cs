namespace Venue.TimeCafe.API.DTOs.Theme;

public record UpdateThemeDto(string ThemeId, string Name, string? Emoji, string? Colors);

public class UpdateThemeDtoExample : IExamplesProvider<UpdateThemeDto>
{
    public UpdateThemeDto GetExamples() =>
        new(ThemeId: "a1111111-1111-1111-1111-111111111111", Name: "VIP зона", Emoji: "👑", Colors: "#FFD700");
}
