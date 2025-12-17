using Venue.TimeCafe.Domain.DTOs;

namespace Venue.TimeCafe.Application.CQRS.Tariffs.Queries;

public record GetActiveTariffsQuery() : IRequest<GetActiveTariffsResult>;

public record GetActiveTariffsResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null,
    IEnumerable<TariffWithThemeDto>? Tariffs = null) : ICqrsResultV2
{
    public static GetActiveTariffsResult GetFailed() =>
        new(false, Code: "GetActiveTariffsFailed", Message: "Не удалось получить активные тарифы", StatusCode: 500);

    public static GetActiveTariffsResult GetSuccess(IEnumerable<TariffWithThemeDto> tariffs) =>
        new(true, Tariffs: tariffs);
}

public class GetActiveTariffsQueryHandler(ITariffRepository repository) : IRequestHandler<GetActiveTariffsQuery, GetActiveTariffsResult>
{
    private readonly ITariffRepository _repository = repository;

    public async Task<GetActiveTariffsResult> Handle(GetActiveTariffsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var tariffs = await _repository.GetActiveAsync();
            return GetActiveTariffsResult.GetSuccess(tariffs);
        }
        catch (Exception)
        {
            return GetActiveTariffsResult.GetFailed();
        }
    }
}
