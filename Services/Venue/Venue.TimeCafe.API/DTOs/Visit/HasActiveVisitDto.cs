namespace Venue.TimeCafe.API.DTOs.Visit;

public record HasActiveVisitDto(string UserId);

public class HasActiveVisitDtoExample : IExamplesProvider<HasActiveVisitDto>
{
    public HasActiveVisitDto GetExamples() =>
        new(UserId: "user-123-abc");
}
