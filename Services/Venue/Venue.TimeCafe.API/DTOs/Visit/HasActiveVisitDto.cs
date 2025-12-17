namespace Venue.TimeCafe.API.DTOs.Visit;

public record HasActiveVisitDto(string UserId);

public class HasActiveVisitDtoExample : IExamplesProvider<HasActiveVisitDto>
{
    public HasActiveVisitDto GetExamples() =>
        new(UserId: "a1111111-1111-1111-1111-111111111111");
}
