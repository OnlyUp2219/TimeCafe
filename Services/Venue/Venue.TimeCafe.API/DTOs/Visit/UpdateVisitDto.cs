namespace Venue.TimeCafe.API.DTOs.Visit;

public record UpdateVisitDto(
    string VisitId,
    string UserId,
    string TariffId,
    DateTimeOffset EntryTime,
    DateTimeOffset? ExitTime,
    decimal? CalculatedCost,
    VisitStatus Status);

public class UpdateVisitDtoExample : IExamplesProvider<UpdateVisitDto>
{
    public UpdateVisitDto GetExamples() =>
        new(
            VisitId: "a1111111-1111-1111-1111-111111111111",
            UserId: "a1111111-1111-1111-1111-111111111111",
            TariffId: "a1111111-1111-1111-1111-111111111111",
            EntryTime: DateTimeOffset.UtcNow.AddHours(-2),
            ExitTime: null,
            CalculatedCost: null,
            Status: VisitStatus.Active);
}
