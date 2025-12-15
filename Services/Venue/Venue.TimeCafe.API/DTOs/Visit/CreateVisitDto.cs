namespace Venue.TimeCafe.API.DTOs.Visit;

public record CreateVisitDto(string UserId, string TariffId);

public class CreateVisitDtoExample : IExamplesProvider<CreateVisitDto>
{
    public CreateVisitDto GetExamples() =>
        new(UserId: "a1111111-1111-1111-1111-111111111111", TariffId: "a1111111-1111-1111-1111-111111111111");
}
