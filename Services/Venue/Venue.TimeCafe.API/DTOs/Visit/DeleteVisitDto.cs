namespace Venue.TimeCafe.API.DTOs.Visit;

public record DeleteVisitDto(int VisitId);

public class DeleteVisitDtoExample : IExamplesProvider<DeleteVisitDto>
{
    public DeleteVisitDto GetExamples() =>
        new(VisitId: 1);
}
