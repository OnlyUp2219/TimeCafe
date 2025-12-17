namespace Venue.TimeCafe.API.DTOs.Visit;

public record GetVisitHistoryDto(string UserId, int PageNumber, int PageSize);

public class GetVisitHistoryDtoExample : IExamplesProvider<GetVisitHistoryDto>
{
    public GetVisitHistoryDto GetExamples() =>
        new(UserId: "a1111111-1111-1111-1111-111111111111", PageNumber: 1, PageSize: 10);
}
