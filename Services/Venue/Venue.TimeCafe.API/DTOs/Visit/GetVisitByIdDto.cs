namespace Venue.TimeCafe.API.DTOs.Visit;

public record GetVisitByIdDto(int VisitId);

public class GetVisitByIdDtoExample : IExamplesProvider<GetVisitByIdDto>
{
    public GetVisitByIdDto GetExamples() =>
        new(VisitId: 1);
}
