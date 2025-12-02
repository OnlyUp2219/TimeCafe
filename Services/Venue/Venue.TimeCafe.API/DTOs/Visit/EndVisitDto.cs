namespace Venue.TimeCafe.API.DTOs.Visit;

public record EndVisitDto(int VisitId);

public class EndVisitDtoExample : IExamplesProvider<EndVisitDto>
{
    public EndVisitDto GetExamples() =>
        new(VisitId: 1);
}
