namespace Venue.TimeCafe.API.DTOs.Theme;

public record UpdateThemeDto(string ThemeId, string Name, string? Emoji, string? Colors);

public class UpdateThemeDtoExample : IExamplesProvider<UpdateThemeDto>
{
    public UpdateThemeDto GetExamples() =>
        new(ThemeId: Guid.NewGuid().ToString(), Name: "VIP зона", Emoji: "👑", Colors: "#FFD700");
}
