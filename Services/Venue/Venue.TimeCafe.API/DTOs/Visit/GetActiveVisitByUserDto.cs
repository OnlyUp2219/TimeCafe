namespace Venue.TimeCafe.API.DTOs.Visit;

public record GetActiveVisitByUserDto(string UserId);

public class GetActiveVisitByUserDtoExample : IExamplesProvider<GetActiveVisitByUserDto>
{
    public GetActiveVisitByUserDto GetExamples() =>
        new(UserId: "a1111111-1111-1111-1111-111111111111");
}
