namespace Venue.TimeCafe.API.DTOs.Tariff;

public record GetTariffsPageDto(int PageNumber, int PageSize);

public class GetTariffsPageExample : IExamplesProvider<GetTariffsPageDto>
{
    public GetTariffsPageDto GetExamples() =>
        new(PageNumber: 1, PageSize: 10);
}