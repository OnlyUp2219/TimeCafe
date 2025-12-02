namespace Venue.TimeCafe.API.DTOs.Visit;

public record UpdateVisitDto(
    int VisitId,
    string UserId,
    int TariffId,
    DateTime EntryTime,
    DateTime? ExitTime,
    decimal? CalculatedCost,
    int Status);

public class UpdateVisitDtoExample : IExamplesProvider<UpdateVisitDto>
{
    public UpdateVisitDto GetExamples() =>
        new(
            VisitId: 1,
            UserId: "user-123-abc",
            TariffId: 1,
            EntryTime: DateTime.UtcNow.AddHours(-2),
            ExitTime: null,
            CalculatedCost: null,
            Status: 1);
}
