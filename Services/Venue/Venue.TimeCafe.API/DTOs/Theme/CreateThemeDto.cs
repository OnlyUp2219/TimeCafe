namespace Venue.TimeCafe.API.DTOs.Theme;

public record CreateThemeDto(string Name, string? Emoji, string? Colors);

public class CreateThemeDtoExample : IExamplesProvider<CreateThemeDto>
{
    public CreateThemeDto GetExamples() =>
        new(Name: "Игровая зона", Emoji: "🎮", Colors: "#FF5733");
}
