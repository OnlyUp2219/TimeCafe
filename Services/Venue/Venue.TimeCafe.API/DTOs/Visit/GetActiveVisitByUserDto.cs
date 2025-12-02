namespace Venue.TimeCafe.API.DTOs.Visit;

public record GetActiveVisitByUserDto(string UserId);

public class GetActiveVisitByUserDtoExample : IExamplesProvider<GetActiveVisitByUserDto>
{
    public GetActiveVisitByUserDto GetExamples() =>
        new(UserId: "user-123-abc");
}
