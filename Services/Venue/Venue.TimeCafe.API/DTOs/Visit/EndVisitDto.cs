namespace Venue.TimeCafe.API.DTOs.Visit;

public record EndVisitDto(string VisitId);

public class EndVisitDtoExample : IExamplesProvider<EndVisitDto>
{
    public EndVisitDto GetExamples() =>
        new(VisitId: "a1111111-1111-1111-1111-111111111111");
}
