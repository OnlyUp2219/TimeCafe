namespace Venue.TimeCafe.API.DTOs.Visit;

public record GetVisitByIdDto(string VisitId);

public class GetVisitByIdDtoExample : IExamplesProvider<GetVisitByIdDto>
{
    public GetVisitByIdDto GetExamples() =>
        new(VisitId: "a1111111-1111-1111-1111-111111111111");
}
