namespace Venue.TimeCafe.API.DTOs.Theme;

public record GetThemeByIdDto(string ThemeId);

public class GetThemeByIdExample : IExamplesProvider<GetThemeByIdDto>
{
    public GetThemeByIdDto GetExamples() =>
        new(ThemeId: "a1111111-1111-1111-1111-111111111111");
}
