namespace Venue.TimeCafe.Application.CQRS.Tariffs.Queries;

public record GetAllTariffsQuery() : IRequest<GetAllTariffsResult>;

public record GetAllTariffsResult(
    bool Success,
    string? Code = null,
    string? Message = null,
    int? StatusCode = null,
    List<ErrorItem>? Errors = null,
    IEnumerable<TariffWithThemeDto>? Tariffs = null) : ICqrsResultV2
{
    public static GetAllTariffsResult GetFailed() =>
        new(false, Code: "GetTariffsFailed", Message: "Не удалось получить тарифы", StatusCode: 500);

    public static GetAllTariffsResult GetSuccess(IEnumerable<TariffWithThemeDto> tariffs) =>
        new(true, Tariffs: tariffs);
}

public class GetAllTariffsQueryValidator : AbstractValidator<GetAllTariffsQuery>
{
    public GetAllTariffsQueryValidator()
    {
    }
}

public class GetAllTariffsQueryHandler(ITariffRepository repository) : IRequestHandler<GetAllTariffsQuery, GetAllTariffsResult>
{
    private readonly ITariffRepository _repository = repository;

    public async Task<GetAllTariffsResult> Handle(GetAllTariffsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var tariffs = await _repository.GetAllAsync();
            return GetAllTariffsResult.GetSuccess(tariffs);
        }
        catch (Exception)
        {
            return GetAllTariffsResult.GetFailed();
        }
    }
}
