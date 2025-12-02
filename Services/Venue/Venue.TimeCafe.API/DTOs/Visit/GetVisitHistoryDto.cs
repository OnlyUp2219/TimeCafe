namespace Venue.TimeCafe.API.DTOs.Visit;

public record GetVisitHistoryDto(string UserId, int PageNumber, int PageSize);

public class GetVisitHistoryDtoExample : IExamplesProvider<GetVisitHistoryDto>
{
    public GetVisitHistoryDto GetExamples() =>
        new(UserId: "user-123-abc", PageNumber: 1, PageSize: 10);
}
