namespace Venue.TimeCafe.API.DTOs.Visit;

public record CreateVisitDto(string UserId, int TariffId);

public class CreateVisitDtoExample : IExamplesProvider<CreateVisitDto>
{
    public CreateVisitDto GetExamples() =>
        new(UserId: "user-123-abc", TariffId: 1);
}
