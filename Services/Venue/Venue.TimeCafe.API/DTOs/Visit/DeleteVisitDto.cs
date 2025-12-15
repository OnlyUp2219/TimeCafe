namespace Venue.TimeCafe.API.DTOs.Visit;

public record DeleteVisitDto(string VisitId);

public class DeleteVisitDtoExample : IExamplesProvider<DeleteVisitDto>
{
    public DeleteVisitDto GetExamples() =>
        new(VisitId: "a1111111-1111-1111-1111-111111111111");
}
